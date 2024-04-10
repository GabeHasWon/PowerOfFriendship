using NPCUtils;
using PoF.Content.Items.Melee;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;

namespace PoF.Content.NPCs.EoD;

public class EmpressOfDeath : ModNPC
{
    private enum EoDState : byte
    {
        Spawn,
        Idle,
        TeleportDash,
        SwordSpam,
        DeathAura, // Only accessible in first phase, normal mode
        SummonAdds, // Ghosts (scale in speed), Ghouls (hung from noose, released after some time, verlet?), Gargoyles (bat but stronger)
        WhipAdds,
    }

    private static Asset<Texture2D> RightHand;

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

    private readonly HashSet<int> portalWhoAmI = [];

    private Vector2 storedPos = Vector2.Zero;
    private int whipWho = -1;
    private int targettedAdd = -1;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.TrailCacheLength[Type] = 20;
        NPCID.Sets.TrailingMode[Type] = 3;

        RightHand ??= ModContent.Request<Texture2D>(Texture + "_Hand_Right");
    }

    public override void Unload() => RightHand = null;

    public override void SetDefaults()
    {
        NPC.width = 298;
        NPC.height = 256;
        NPC.damage = 100;
        NPC.defense = 35;
        NPC.lifeMax = 60000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = Item.buyPrice(0, 15, 0, 0);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Critter;
        NPC.DeathSound = SoundID.NPCDeath4;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Graveyard");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        LeadingConditionRule notExpert = new(new Conditions.NotExpert());
        notExpert.AddCommon<VoidRippers>();
        npcLoot.Add(notExpert);
    }

    public override void AI()
    {
        Timer++;

        portalWhoAmI.RemoveWhere(x => !Main.projectile[x].active || Main.projectile[x].type != ModContent.ProjectileType<EoDPortal>());

        switch (State)
        {
            case EoDState.Idle:
                bool switchState = Timer > 120 - LifeFactor * 60;
                IdleMovement(switchState);

                if (switchState)
                {
                    SwitchState(EoDState.SwordSpam);

                    if (AuraWho == -1 && (Main.expertMode || Main.rand.NextBool(6)))
                        SwitchState(EoDState.DeathAura);

                    if (portalWhoAmI.Count < 4 && Main.rand.NextBool(2))
                        SwitchState(EoDState.SummonAdds);

                    if (portalWhoAmI.Count > 0 && Main.rand.NextBool(1))
                        SwitchState(EoDState.WhipAdds);
                }

                break;

            case EoDState.TeleportDash:
                TeleportDash();
                break;

            case EoDState.SwordSpam:
                IdleMovement(false);

                if (Timer is > 30 and < 80)
                {
                    if (Timer % 3 == 0)
                    {
                        float rot = (Timer - 30) / 80f * MathHelper.Pi - MathHelper.PiOver4;
                        var vel = NPC.DirectionTo(Target.Center).RotatedByRandom(0.2f).RotatedBy(rot * -Target.direction) * 14;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<TelegraphSword>(), 60, 0, Main.myPlayer);
                    }
                }

                if (Timer == 100)
                    SwitchState(EoDState.Idle);
                break;

            case EoDState.DeathAura:
                SummonAura();
                break;

            case EoDState.SummonAdds:
                NPC.velocity *= 0.95f;

                if (Timer == 1)
                {
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<EoDPortal>(), 0, 0, Main.myPlayer, 0, Main.rand.NextFloat(350, 500));
                    portalWhoAmI.Add(proj);
                }

                if (Timer > 30)
                    SwitchState(EoDState.Idle);
                break;

            case EoDState.WhipAdds:
                WhipAdds();
                break;

            default: // Spawn
                AuraWho = -1;

                if (Timer > 120)
                    SwitchState(EoDState.Idle);
                break;
        }

        if (AuraWho == -1 || !Main.projectile[AuraWho].active || Main.projectile[AuraWho].type != ModContent.ProjectileType<DeathAura>())
            AuraWho = -1;
    }

    private void WhipAdds()
    {
        NPC.velocity *= 0.98f;

        if (targettedAdd == -1)
            targettedAdd = Main.rand.Next([..portalWhoAmI]);

        int npcSlot = (int)Main.projectile[targettedAdd].ai[2];

        if (npcSlot is (-2) or 0)
        {
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
        }
        else if (Timer == 60)
        {
            var vel = NPC.DirectionTo(target.Center) * 1.2f;
            whipWho = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + vel, vel, ModContent.ProjectileType<EoDWhip>(), 60, 0, Main.myPlayer, ai2: NPC.whoAmI);
        }
        else if (Timer >= 100)
            SwitchState(EoDState.Idle);
    }

    private void SummonAura()
    {
        NPC.velocity *= 0.95f;
        NPC.rotation *= 0.9f;

        if (Timer == 10)
            AuraWho = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeathAura>(), 40, 0, Main.myPlayer, 0, NPC.whoAmI);

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
        // Teleport past the player, bats reconglomerate on the other side, cooldown
        const float FadeTime = 10f;
        const float TeleportTime = 30f;

        NPC.velocity *= 0.9f;

        if (Timer < FadeTime)
            NPC.Opacity = 1 - (Timer / (FadeTime - 1));
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
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
        {
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
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
        }

        return true;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {

    }
}