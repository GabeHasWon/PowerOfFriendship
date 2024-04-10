using System;

namespace PoF.Content.NPCs.EoD;

public class DeathsHeadMoth : ModNPC
{
    private Projectile Parent => Main.projectile[PortalWhoAmI];
    private Player Target => Main.player[NPC.target];

    private bool IsChained
    {
        get => NPC.ai[0] == 0;
        set => NPC.ai[0] = value ? 0 : 1;
    }

    private int PortalWhoAmI
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private ref float ChainDistance => ref NPC.ai[2];

    public override void SetStaticDefaults() => Main.npcFrameCount[NPC.type] = 4;

    public override void SetDefaults()
    {
        NPC.width = 26;
        NPC.height = 18;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 120;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Critter;
        NPC.DeathSound = SoundID.Critter;
    }

    public override void AI()
    {
        NPC.direction = NPC.spriteDirection = -Math.Sign(Target.Center.X - NPC.Center.X);

        if (IsChained)
        {
            NPC.TargetClosest();
            NPC.velocity += NPC.DirectionTo(Target.Center) * 1.5f;

            if (NPC.velocity.LengthSquared() > 12 * 12)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * 12;

            if (NPC.DistanceSQ(Parent.Center) > (ChainDistance * ChainDistance) * (Parent.ModProjectile as EoDPortal).ExtendFactor)
                NPC.velocity += NPC.DirectionTo(Parent.Center) * 2f;

            NPC.rotation = (Parent.ModProjectile as EoDPortal).endOfRopeRotation;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = (int)(NPC.frameCounter / 2 % 4) * frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
    }
}
