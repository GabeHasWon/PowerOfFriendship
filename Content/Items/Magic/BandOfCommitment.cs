using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Magic;

class BandOfCommitment : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.TopazStaff);
        Item.damage = 56;
        Item.useTime = 2;
        Item.useAnimation = 10;
        Item.reuseDelay = 40;
        Item.shoot = ModContent.ProjectileType<BlackHeart>();
        Item.shootSpeed = 14;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        => velocity = velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.8f, 1f);

    public class BlackHeart : ModProjectile
    {
        const int MaxTimeLeft = 240;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(26);
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxTimeLeft;
        }

        public override void AI()
        {
            Projectile.scale -= 0.002f;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Projectile.timeLeft < 30)
                Projectile.Opacity = Projectile.timeLeft / 30f;
        }
    }
}
