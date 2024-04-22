namespace PoF.Content.Items.Misc;

class DeathsHeadEgg : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.LizardEgg);
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.shoot = ModContent.ProjectileType<BabyMoth>();
        Item.shootSpeed = 6;
        Item.rare = ItemRarityID.Purple;
    }

    public class BabyMoth : ModProjectile
    {
        const int MaxTimeLeft = 240;

        Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            Main.projFrames[Type] = 2;
        }

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
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 2;
            Projectile.frame = (int)(Projectile.frameCounter++ / 3f) % 2;

            if (Projectile.velocity.X != 0)
            {
                if (Projectile.velocity.X < 0)
                {
                    Projectile.rotation -= MathHelper.Pi;
                    Projectile.spriteDirection = -1;
                }
                else
                    Projectile.spriteDirection = 1;
            }

            var target = Owner.Center - new Vector2(60 * Owner.direction, 0);
            float maxSpeed = 8;

            Projectile.velocity += Projectile.DirectionTo(target);

            if (Projectile.DistanceSQ(target) < 40 * 40)
                maxSpeed *= Projectile.Distance(target) / 40f;

            if (Projectile.velocity.LengthSquared() > maxSpeed * maxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
        }
    }
}
