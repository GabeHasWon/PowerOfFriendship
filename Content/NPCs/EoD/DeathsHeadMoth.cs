using NPCUtils;
using System;
using System.IO;
using System.Linq;
using Terraria.GameContent.Bestiary;

namespace PoF.Content.NPCs.EoD;

public class DeathsHeadMoth : ModNPC, IStruckByWhipNPC
{
    private Projectile Parent => Main.projectile.FirstOrDefault(x => x.identity == PortalIdentity);
    private Player Target => Main.player[NPC.target];

    private bool IsChained
    {
        get => NPC.ai[0] == 0;
        set => NPC.ai[0] = value ? 0 : 1;
    }

    private int PortalIdentity
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private ref float ChainDistance => ref NPC.ai[2];

    internal float hitCount = 1;

    public override void SetStaticDefaults() => Main.npcFrameCount[NPC.type] = 4;

    public override void SetDefaults()
    {
        NPC.width = 26;
        NPC.height = 18;
        NPC.damage = 45;
        NPC.defense = 20;
        NPC.lifeMax = 200;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Zombie77;
        NPC.DeathSound = SoundID.Zombie73;
    }

    public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>());
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Graveyard");

    public override void AI()
    {
        NPC.direction = NPC.spriteDirection = -Math.Sign(Target.Center.X - NPC.Center.X);

        if (IsChained && PortalIdentity == -1 || Parent is null || !Parent.active)
            IsChained = false;

        if (IsChained)
        {
            NPC.TargetClosest();
            NPC.velocity += NPC.DirectionTo(Target.Center) * 1.5f;

            if (NPC.velocity.LengthSquared() > 12 * 12)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * 12;

            if (hitCount > 10)
            {
                IsChained = false;
                NPC.life = NPC.lifeMax;
            }
        }
        else
        {
            NPC.TargetClosest();
            NPC.velocity += NPC.DirectionTo(Target.Center) * (0.65f + hitCount * 0.25f);
            NPC.rotation = NPC.velocity.ToRotation();

            if (NPC.velocity.X < 0)
            {
                NPC.rotation += MathHelper.Pi;
                NPC.spriteDirection = 1;
            }
            else
                NPC.spriteDirection = -1;

            float maxSpeed = 4 + (hitCount * 0.5f);

            if (NPC.velocity.LengthSquared() > maxSpeed * maxSpeed)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * maxSpeed;
        }
    }

    internal void UpdateFromParent(Projectile parent)
    {
        NPC.rotation = (parent.ModProjectile as EoDPortal).endOfRopeRotation;

        if (NPC.DistanceSQ(parent.Center) > ChainDistance * ChainDistance * (parent.ModProjectile as EoDPortal).ExtendFactor)
            NPC.velocity += NPC.DirectionTo(parent.Center) * 2f;
    }

    public override Color? GetAlpha(Color drawColor) => Color.Lerp(drawColor, Lighting.GetColor(NPC.Center.ToTileCoordinates()), MathF.Min(hitCount, 10) / 10f);

    public override bool CheckDead()
    {
        if (IsChained)
        {
            IsChained = false;
            NPC.life = NPC.lifeMax;
            return false;
        }

        return true;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = (int)(NPC.frameCounter / 2 % 4) * frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < 3; ++i)
            Dust.NewDust(NPC.Center, 26, 18, DustID.Blood, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));

        if (NPC.life <= 0 && Main.netMode != NetmodeID.Server && !IsChained)
        {
            for (int i = 0; i < 10; ++i)
                Dust.NewDust(NPC.Center, 26, 18, DustID.Blood, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));

            for (int i = 0; i < 3; ++i)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>($"{Name}" + i).Type, NPC.scale);
        }
    }

    public void OnHitByWhip(Projectile projectile)
    {
        hitCount++;

        if (NPC.life <= 0)
        {
            NPC.life = NPC.lifeMax;
            NPC.netUpdate = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write((byte)hitCount);
    public override void ReceiveExtraAI(BinaryReader reader) => hitCount = reader.ReadByte();
}
