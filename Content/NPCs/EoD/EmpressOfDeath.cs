using NPCUtils;
using PoF.Common.Globals;
using PoF.Content.Items.Blocks;
using PoF.Content.Items.Magic;
using PoF.Content.Items.Melee;
using PoF.Content.Items.Misc;
using PoF.Content.Items.Ranged;
using PoF.Content.Items.Talismans;
using PoF.Content.Tiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;

namespace PoF.Content.NPCs.EoD;

public class EmpressOfDeath : ModNPC
{
    public enum EoDState : byte
    {
        // All phases
        Spawn,
        Idle,
        TeleportDash,
        SwordSpam,
        DeathAura,
        SummonAdds,
        WhipAdds,
        // Second phase below
        SpawnPhantoms,
        PullAura,
        CardinalDaggers,
        Death,
    }

    private static Asset<Texture2D> Hands;
    private static Asset<Texture2D> Wings;
    private static Asset<Texture2D> TailWings;
    private static Asset<Texture2D> Depth;

    private Player Target => Main.player[NPC.target];
    private float LifeFactor => NPC.life / (float)NPC.lifeMax;

    private EoDState State
    {
        get => (EoDState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private ref float Timer => ref NPC.ai[1];
    private ref float CircleTimer => ref NPC.ai[2];

    private int AuraWho
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    internal float leftHandOffset = 0;
    internal float auraPullAngle = 0;
    internal float auraPullStrength = 0;

    private readonly HashSet<int> portalWhoAmI = [];

    private Vector2 storedPos = Vector2.Zero;
    private int whipWho = -1;
    private int targettedAdd = -1;
    private float visualTimer = 0;
    private byte cardinalDir = 0;
    private float fadeOut = 0;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.TrailCacheLength[Type] = 20;
        NPCID.Sets.TrailingMode[Type] = 3;

        Hands ??= ModContent.Request<Texture2D>(Texture + "_Hands");
        Wings ??= ModContent.Request<Texture2D>(Texture + "_Wings");
        Depth ??= ModContent.Request<Texture2D>(Texture + "_Wings_Depths");
        TailWings ??= ModContent.Request<Texture2D>(Texture + "_TailWings");
    }

    public override void Unload() => Hands = null;

    public override void SetDefaults()
    {
        NPC.width = 298;
        NPC.height = 256;
        NPC.damage = 0;
        NPC.defense = 35;
        NPC.lifeMax = 60000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = Item.buyPrice(0, 15, 0, 0);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Zombie54;
        NPC.DeathSound = SoundID.NPCDeath55;
        NPC.Opacity = 0;
        NPC.boss = true;

        Music = MusicID.Boss5;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Graveyard");
    public override void BossLoot(ref string name, ref int potionType) => potionType = ItemID.GreaterHealingPotion;

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.AddMasterModeRelicAndPet<EoDRelicItem, DeathsHeadEgg>();
        npcLoot.AddBossBag<EoDBag>();

        LeadingConditionRule notExpert = new(new Conditions.NotExpert());
        notExpert.AddOneFromOptions<VoidRippers, KissOfDeath, BandOfCommitment, BlackDeath>();
        notExpert.AddCommon<EoDTrophyItem>(10);
        npcLoot.Add(notExpert);
    }

    public override bool CheckDead()
    {
        if (State != EoDState.Death)
        {
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            SwitchState(EoDState.Death);
            return false;
        }

        ModContent.GetInstance<DownedEoDSystem>().downedEoD = true;
        NetMessage.SendData(MessageID.WorldData);
        return true;
    }

    public override bool CheckActive() => true;

    public override void AI()
    {
        Main.GraveyardVisualIntensity = 1;

        if (State is not EoDState.Death and not EoDState.Spawn)
        {
            NPC.Opacity = NPC.life / (float)NPC.lifeMax * 0.75f + 0.25f;
            fadeOut = NPC.Opacity;
        }
        else if (State == EoDState.Death)
        {
            NPC.rotation *= 0.99f;
            NPC.Opacity = fadeOut;
            fadeOut -= 0.002f;
            NPC.velocity.Y += 0.05f;

            if (NPC.Opacity <= 0)
            {
                NPC.life = 0;
                NPC.checkDead();

                if (Main.netMode != NetmodeID.Server)
                {
                    PunchCameraModifier modifier = new(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 15, 3f, 20, 8000, "EmpressOfDeath");
                    Main.instance.CameraModifiers.Add(modifier);

                    static float Speed() => Main.rand.NextFloat(8, 16) * Main.rand.NextFloatDirection();

                    for (int i = 0; i < 80; ++i)
                        Dust.NewDust(NPC.Center, 1, 1, DustID.Smoke, Speed(), Speed(), 160, default, Main.rand.NextFloat(1, 2f));

                    for (int i = 0; i < 10; ++i)
                        Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new(Speed(), Speed()), GoreID.Smoke1 + Main.rand.Next(3));
                }
            }

            return;
        }

        Timer++;
        visualTimer++;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            portalWhoAmI.RemoveWhere(x => !Main.projectile[x].active || Main.projectile[x].type != ModContent.ProjectileType<EoDPortal>() || Main.projectile[x].ai[2] == -2);

        switch (State)
        {
            case EoDState.Idle:
                leftHandOffset = MathHelper.Lerp(leftHandOffset, 0, 0.08f);

                bool switchState = Timer > 100 - (1 - LifeFactor) * 90;
                IdleMovement(switchState);

                if (switchState)
                {
                    SwitchState(DetermineNextState());

                    if (portalWhoAmI.Count < (!Main.masterMode ? (Main.expertMode ? 20 : 14) : 35) && Main.rand.NextBool(3))
                        SwitchState(EoDState.SummonAdds);

                    if (portalWhoAmI.Count > 0 && Main.rand.NextBool(3))
                        SwitchState(EoDState.WhipAdds);

                    if (AuraWho == -1 && (Main.expertMode || Main.rand.NextBool(6)))
                        SwitchState(EoDState.DeathAura);
                }

                break;

            case EoDState.TeleportDash:
                TeleportDash();
                break;

            case EoDState.SwordSpam:
                IdleMovement(false);

                float adjTime = Timer + (int)((1 - LifeFactor) * 30);

                if (adjTime is > 30 and < 80 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (adjTime % 3 == 0)
                    {
                        float rot = (adjTime - 30) / 80f * MathHelper.Pi - MathHelper.PiOver4;
                        var vel = NPC.DirectionTo(Target.Center).RotatedByRandom(0.2f).RotatedBy(rot * -Target.direction) * 14;
                        int damage = Utilities.ToActualDamage(120, 1.6f, 2.2f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<TelegraphSword>(), damage, 0, Main.myPlayer);
                    }
                }

                if (adjTime > 100)
                    SwitchState(EoDState.Idle);
                break;

            case EoDState.DeathAura:
                SummonAura();
                break;

            case EoDState.SummonAdds:
                NPC.velocity *= 0.95f;

                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int type = ModContent.ProjectileType<EoDPortal>();
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, 0, 0, Main.myPlayer, -1, Main.rand.NextFloat(350, 500));
                    portalWhoAmI.Add(Main.projectile[proj].identity);
                }

                if (Timer > 30)
                    SwitchState(EoDState.Idle);
                break;

            case EoDState.WhipAdds:
                WhipAdds();
                break;

            case EoDState.SpawnPhantoms:
                SpawnPhantoms();
                break;

            case EoDState.PullAura:
                PullAura();
                break;

            case EoDState.CardinalDaggers:
                CardinalDaggers();
                break;

            default: // Spawn
                AuraWho = -1;
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1f, 0.1f);
                NPC.velocity.Y = (1 - Timer / 120f) * -8;

                if (Timer == 1 && Main.netMode != NetmodeID.Server)
                    SoundEngine.PlaySound(SoundID.NPCDeath52, NPC.Center);

                if (Timer > 120)
                {
                    SwitchState(EoDState.Idle);

                    if (Main.expertMode)
                    {
                        int type = ModContent.ProjectileType<SpinScythe>();
                        int damage = Utilities.ToActualDamage(40, 1.4f, 2f);

                        for (int i = 0; i < 2; ++i)
                        {
                            float rot = MathHelper.TwoPi / 2 * i;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, damage, 0, Main.myPlayer, NPC.whoAmI, 0, rot);
                        }
                    }
                }

                break;
        }

        if (AuraWho == -1 || !Main.projectile[AuraWho].active || Main.projectile[AuraWho].type != ModContent.ProjectileType<DeathAura>())
            AuraWho = -1;
    }

    private void CardinalDaggers()
    {
        if (Timer == 1)
        {
            cardinalDir = (byte)Main.rand.Next(4);
            NPC.netUpdate = true;
        }

        NPC.velocity *= 0.8f;

        if (Timer < 60)
            NPC.Center = Vector2.Lerp(NPC.Center, CardinalPosition(false), 0.05f);
        else if (Timer < 240)
        {
            NPC.Center = Vector2.Lerp(CardinalPosition(false), CardinalPosition(true), (Timer - 60) / 180f);

            if (Timer % 5 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = new Vector2(0, 14).RotatedBy(cardinalDir switch
                {
                    0 => 0,
                    1 => MathHelper.Pi,
                    2 => MathHelper.PiOver2,
                    _ => MathHelper.Pi + MathHelper.PiOver2
                });
                int damage = Utilities.ToActualDamage(120, 1.6f, 2.5f);

                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<TelegraphSword>(), damage, 0, Main.myPlayer);
            }
        }
        else if (Timer >= 240)
            SwitchState(EoDState.Idle);
    }

    private Vector2 CardinalPosition(bool add) => Main.projectile[AuraWho].Center + cardinalDir switch
    {
        0 => new Vector2(1200, 0),
        1 => new Vector2(-1200, 0),
        2 => new Vector2(0, -1200),
        _ => new Vector2(0, 1200)
    } * (add ? 1 : -1);

    private void PullAura()
    {
        if (Timer == 1)
        {
            auraPullAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            auraPullStrength = 1;
            NPC.netUpdate = true;
        }

        NPC.Center = Vector2.Lerp(NPC.Center, Main.projectile[AuraWho].Center, 0.02f);
        NPC.velocity.X *= 0.9f;

        if (Timer is > 10 and < 120)
        {
            auraPullStrength -= 0.01f;

            if (auraPullStrength < 0.2f)
                auraPullStrength = 0.2f;
        }
        else if (Timer is >= 480 and < 600)
        {
            auraPullStrength += 0.01f;

            if (auraPullStrength > 1f)
                auraPullStrength = 1f;
        }
        else if (Timer >= 600)
            SwitchState(EoDState.Idle);

        Dust.NewDust(Main.projectile[AuraWho].Center + new Vector2(1206 * auraPullStrength, 10).RotatedBy(auraPullAngle), 1, 1, DustID.BubbleBurst_Pink);
    }

    private void SpawnPhantoms()
    {
        IdleMovement(false);

        if (Timer % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            int type = ModContent.ProjectileType<Phantom>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, type, Utilities.ToActualDamage(45, 1.5f, 2f), 0, Main.myPlayer);
        }

        if (Timer > 70)
            SwitchState(EoDState.Idle);
    }

    private EoDState DetermineNextState()
    {
        if (NPC.life > NPC.lifeMax / 2)
            return EoDState.SwordSpam;

        int random = Main.rand.Next(5);

        return random switch
        {
            0 or 1 => EoDState.SwordSpam,
            2 => EoDState.CardinalDaggers,
            3 or _ => EoDState.SpawnPhantoms,
        };
    }

    private void WhipAdds()
    {
        NPC.velocity *= 0.98f;

        if (targettedAdd == -1 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            targettedAdd = Main.rand.Next([..portalWhoAmI]);
            NPC.netUpdate = true;
        }

        if (targettedAdd == -1)
            return;

        Projectile proj = Main.projectile.First(x => x.identity == targettedAdd);

        if (proj is null)
            return;

        int npcSlot = (int)proj.ai[2];

        if (npcSlot is (-2) or 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                SwitchState(EoDState.Idle);

            return;
        }

        NPC target = Main.npc[npcSlot];

        if (!target.active)
        {
            SwitchState(EoDState.Idle);
            return;
        }

        if (Timer < 60)
        {
            Vector2 targetPosition = target.Center + NPC.DirectionFrom(target.Center) * 300;
            NPC.Center = Vector2.Lerp(NPC.Center, targetPosition, 0.04f);
            NPC.rotation = (NPC.Center.X - targetPosition.X) * -0.001f;
            leftHandOffset = MathHelper.Lerp(leftHandOffset, 60, 0.05f);
        }
        else if (Timer == 60)
        {
            var vel = NPC.DirectionTo(target.Center) * 1.2f;
            int damage = Utilities.ToActualDamage(100);
            whipWho = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + vel, vel, ModContent.ProjectileType<EoDWhip>(), damage, 0, Main.myPlayer, ai2: NPC.whoAmI);
        }
        else if (Timer >= 100)
            SwitchState(EoDState.Idle);
    }

    private void SummonAura()
    {
        NPC.velocity *= 0.95f;
        NPC.rotation *= 0.9f;

        if (Timer == 10 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            int damage = Utilities.ToActualDamage(60);
            AuraWho = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeathAura>(), damage, 0, Main.myPlayer, 0, NPC.whoAmI);
            NPC.netUpdate = true;
        }

        if (Timer == 60)
            SwitchState(EoDState.Idle);
    }

    private void IdleMovement(bool setVelocity)
    {
        Vector2 targetPosition = Target.Center - new Vector2(0, 300) + new Vector2(0, 100 + 50 * LifeFactor).RotatedBy(CircleTimer * 0.05f);
        NPC.rotation = (NPC.Center.X - targetPosition.X) * -0.001f;
        NPC.velocity = Vector2.Zero;
        NPC.damage = 0;
        CircleTimer++;

        if (setVelocity)
            NPC.velocity = (targetPosition - NPC.Center) * 0.025f;
        else
            NPC.Center = Vector2.Lerp(NPC.Center, targetPosition, 0.03f);
    }

    private void TeleportDash()
    {
        const float FadeTime = 10f;
        const float TeleportTime = 30f;

        NPC.velocity *= 0.9f;

        if (Timer < FadeTime)
            NPC.Opacity = 1 - Timer / (FadeTime - 1);
        else if (Timer == FadeTime)
        {
            Vector2 dir = NPC.DirectionTo(Target.Center);
            Vector2 nextPos = Target.Center + dir * 650;
            NPC.dontTakeDamage = true;
            storedPos = nextPos;
        }
        else if (Timer == FadeTime + TeleportTime)
        {
            NPC.Center = storedPos;

            for (int i = 0; i < NPC.oldPos.Length; ++i)
                NPC.oldPos[i] = NPC.position;
        }
        else if (Timer is > (FadeTime + TeleportTime) and < (FadeTime * 2 + TeleportTime))
            NPC.Opacity = (Timer - (FadeTime + TeleportTime - 1)) / FadeTime;
        else if (Timer == FadeTime * 2 + TeleportTime)
            NPC.dontTakeDamage = false;
        else if (Timer > FadeTime * 2 + TeleportTime * 2)
            SwitchState(EoDState.Idle);
    }

    private void SwitchState(EoDState toState)
    {
        State = toState;
        Timer = 0;
        targettedAdd = -1;
        NPC.TargetClosest(false);
        NPC.netUpdate = true;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((short)targettedAdd);
        writer.Write((byte)portalWhoAmI.Count);

        for (int i = 0; i < portalWhoAmI.Count; ++i)
            writer.Write((short)portalWhoAmI.ElementAt(i));
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        targettedAdd = reader.ReadInt16();
        int count = reader.ReadByte();

        portalWhoAmI.Clear();

        for (int i = 0; i < count; ++i)
            portalWhoAmI.Add(reader.ReadInt16());
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Main.EntitySpriteDraw(tex, NPC.Center - screenPos + new Vector2(0, 50), null, drawColor, NPC.rotation, tex.Size() / 2f, NPC.scale, SpriteEffects.None, 0);
            return true;
        }

        Texture2D texture = TextureAssets.Npc[Type].Value;
        var drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);

        for (int k = 0; k < NPC.oldPos.Length; k++)
        {
            if (k % 2 == 0)
                continue;

            float factor = k / (float)NPC.oldPos.Length;
            Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY + 4f);
            var fadeColor = Color.Lerp(Color.Lerp(drawColor, Color.Blue, factor), Color.Lerp(Color.Blue, Color.Transparent, factor), factor);
            Color color = NPC.GetAlpha(fadeColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0);
            //DrawWings(drawPos, color);
        }

        DrawWings(NPC.Center - Main.screenPosition, drawColor);
        return true;
    }

    private void DrawWings(Vector2 drawPos, Color color)
    {
        Point center = NPC.Center.ToTileCoordinates();
        float bright = Lighting.Brightness(center.X, center.Y);
        Main.EntitySpriteDraw(TailWings.Value, drawPos, null, color * NPC.Opacity * bright, NPC.rotation, TailWings.Size() / 2f, NPC.scale, SpriteEffects.None);

        Effect effect = ModContent.Request<Effect>("PoF/Assets/Effects/EoDWingFlap").Value;

        if (effect is null)
            return;

        Main.spriteBatch.End();

        effect.Parameters["topWing"].SetValue(GetWingOffset(0) * 1f - 0.3f);
        effect.Parameters["bottomWing"].SetValue(GetWingOffset(MathHelper.PiOver4) * 1.5f - 0.45f);
        effect.Parameters["depthFactor"].SetValue(GetWingOffset(MathHelper.PiOver2) * 0.9f);
        effect.Parameters["depthMap"].SetValue(Depth.Value);
        effect.Parameters["opacity"].SetValue(NPC.Opacity * bright);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, Main.Rasterizer, effect, Main.GameViewMatrix.EffectMatrix);

        var wingFrame = Wings.Value.Frame(1, 1, 0, 0, 0, 0);
        Main.EntitySpriteDraw(Wings.Value, drawPos - new Vector2(0, 46), wingFrame, color, NPC.rotation, wingFrame.Size() / 2f, NPC.scale, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    private static float GetWingOffset(float off) => MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.15f + off), 2) * 0.4f;

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = Hands.Value;
        var pos = NPC.Center - screenPos;
        var origin = new Vector2(40, -20 + MathF.Sin(visualTimer * 0.04f + 1.5f) * 10f) + tex.Size() / 4f;
        Main.EntitySpriteDraw(tex, pos, new Rectangle(0, 0, 22, 26), Color.MediumPurple with { A = 0 }* NPC.Opacity, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
        DrawLeftHand(tex, pos);
    }

    private void DrawLeftHand(Texture2D tex, Vector2 pos)
    {
        var origin = new Vector2(-40, -20 + MathF.Sin(visualTimer * 0.04f) * 10f + leftHandOffset) + tex.Size() / 4f;
        var col = Color.MediumPurple with { A = 0 } * NPC.Opacity;

        if (State != EoDState.WhipAdds)
            Main.EntitySpriteDraw(tex, pos, new Rectangle(24, 0, 22, 26), col, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
        else
            Main.EntitySpriteDraw(tex, pos, new Rectangle(2, 28, 16, 12), col, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);
    }
}