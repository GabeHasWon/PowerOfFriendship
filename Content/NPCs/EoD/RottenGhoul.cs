using NPCUtils;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;

namespace PoF.Content.NPCs.EoD;

public class RottenGhoulHanging : ModNPC, IStruckByWhipNPC
{
    private Projectile Parent => Main.projectile.FirstOrDefault(x => x.identity == (int)NPC.ai[0]);
    private ref float Timer => ref NPC.ai[1];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetDefaults()
    {
        NPC.width = 36;
        NPC.height = 48;
        NPC.damage = 50;
        NPC.defense = 20;
        NPC.lifeMax = 400;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit37;
        NPC.DeathSound = SoundID.NPCDeath40;
    }

    public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>());

    public override void AI()
    {
        Timer++;

        if (Timer > 20 * 60)
            NPC.Transform(ModContent.NPCType<RottenGhoul>());

        if (Main.expertMode && Timer % 360 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.netUpdate = true;
            NPC.TargetClosest();
            SpawnSpit(1.1f);
            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 1f, PitchRange = (-0.8f, 0.2f) });
        }
    }

    internal void UpdateFromParent(Projectile Parent)
    {
        if (Parent is null || !Parent.active || Parent.type != ModContent.ProjectileType<EoDPortal>())
        {
            NPC.StrikeInstantKill();
            return;
        }

        var portal = Parent.ModProjectile as EoDPortal;
        NPC.Center = portal.SwingOfRope;
        NPC.rotation = portal.endOfRopeRotation - MathHelper.Pi;
        NPC.direction = NPC.spriteDirection = -Math.Sign(MathF.Cos(Parent.ai[0] * 0.03f));
        NPC.velocity *= 0;
    }

    private void SpawnSpit(float speedBoost, float rotation = 0f)
    {
        Vector2 vel = NPC.DirectionTo(Main.player[NPC.target].Center).RotatedByRandom(rotation) * 5 * speedBoost;
        int damage = Utilities.ToActualDamage(40, 2f, 3);
        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(8, 10), vel, ModContent.ProjectileType<RottenGhoul.RottenSpit>(), damage, 1f, Main.myPlayer);
        Main.projectile[proj].extraUpdates++;
    }

    public override bool CheckDead()
    {
        NPC.Transform(ModContent.NPCType<RottenGhoul>());
        return false;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
        {
            for (int i = 0; i < 6; ++i)
                Dust.NewDust(NPC.Center, 26, 18, DustID.Grass, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
        }
    }

    public void OnHitByWhip(Projectile projectile)
    {
        NPC.TargetClosest();

        for (int i = 0; i < 8; ++i)
            SpawnSpit(Main.rand.NextFloat(1.1f, 1.7f), Main.rand.NextFloat(-0.2f, 0.2f));

        Timer = 1;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        if (Main.expertMode)
            target.AddBuff(BuffID.Darkness, 240);
    }
}

public class RottenGhoul : ModNPC
{
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 8;

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.DesertGhoul);
        NPC.damage = 80;
        NPC.defense = 30;
        NPC.lifeMax = 250;
        NPC.noGravity = false;
        NPC.knockBackResist = 0.4f;

        AnimationType = NPCID.DesertGhoul;
        AIType = NPCID.DesertGhoul;
    }

    public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>());

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddInfo(this, "Graveyard");
        bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[ModContent.NPCType<EmpressOfDeath>()], true);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < 3; ++i)
            Dust.NewDust(NPC.Center, 26, 18, DustID.Blood, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));

        if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
        {
            for (int i = 0; i < 10; ++i)
                Dust.NewDust(NPC.Center, 26, 18, DustID.Blood, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));

            for (int i = 0; i < 3; ++i)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>($"{Name}" + i).Type, NPC.scale);
            
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>($"{Name}1").Type, NPC.scale);
        }
    }

    public class RottenSpit : ModProjectile
    {
        const int MaxTimeLeft = 240;

        private bool HitPlayer
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.Size = new Vector2(16);
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.Opacity = 0.8f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.008f * Projectile.velocity.X;

            if (Projectile.timeLeft < 15f)
                Projectile.Opacity = Projectile.timeLeft / 15f * 0.8f;

            if (HitPlayer)
                Projectile.velocity.Y += 0.2f;

            if (Main.rand.NextBool(6))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => HitPlayer = true;
    }
}
