using System;
using Terraria.Audio;

namespace PoF.Content.NPCs.EoD;

public class RottenGhoulHanging : ModNPC
{
    private Projectile Parent => Main.projectile[(int)NPC.ai[0]];
    private ref float Timer => ref NPC.ai[1];

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 2;

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
    }

    public override void AI()
    {
        if (!Parent.active || Parent.type != ModContent.ProjectileType<EoDPortal>())
        {
            NPC.StrikeInstantKill();
            return;
        }

        var portal = Parent.ModProjectile as EoDPortal;
        NPC.Center = portal.SwingOfRope;
        NPC.rotation = portal.endOfRopeRotation - MathHelper.Pi;
        NPC.direction = NPC.spriteDirection = -Math.Sign(MathF.Cos(Parent.ai[0] * 0.03f));

        Timer++;

        if (Main.expertMode && Timer % 360 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.TargetClosest();
            Vector2 vel = NPC.DirectionTo(Main.player[NPC.target].Center) * 12;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(8, 10), vel, ProjectileID.SalamanderSpit, 30, 1f, Main.myPlayer);
            SoundEngine.PlaySound(SoundID.NPCDeath9 with { Volume = 1f, PitchRange = (-0.8f, 0.2f ) });
        } 
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

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
        {
            for (int i = 0; i < 6; ++i)
                Dust.NewDust(NPC.Center, 26, 18, DustID.Grass, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
        }
    }
}
