using PoF.Content.Items.Talismans;

namespace PoF.Common.Players;

public class GuardianPlayer : ModPlayer
{
    internal bool hasSet = false;

    public override void ResetEffects() => hasSet = false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!hasSet)
            return;

        if (target.life > 0)
            return;

        int damage = (int)Player.GetDamage(DamageClass.Summon).ApplyTo(26);
        var baseVel = new Vector2(0, Main.rand.NextFloat(3, 7)).RotatedByRandom(MathHelper.TwoPi);
        int proj = Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, baseVel, ModContent.ProjectileType<GhostSkull>(), damage, 3f);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
    }

    class GhostSkull : ModProjectile
    {
        public override string Texture => "PoF/Assets/Textures/GhostSkull";

        private ref float Time => ref Projectile.ai[0];
        private ref float Target => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (Time == 0)
                Target = -1;

            Time++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpectreStaff);

            NPC npc = null;

            if (Target < 0 && !Projectile.GetNearestNPCTarget(out npc))
                return;

            if (npc is not null)
                Target = npc.whoAmI;

            if (!Main.npc[(int)Target].active)
            {
                Target = -1;
                return;
            }

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Main.npc[(int)Target].Center) * MathHelper.Min(Time * 0.15f, 10f), 0.05f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White with { A = 0 };
    }
}
