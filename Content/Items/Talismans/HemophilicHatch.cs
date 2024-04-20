namespace PoF.Content.Items.Talismans;

internal class HemophilicHatch : Talisman
{
    protected override float TileRange => 25;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DevourerCharm>();
    }

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 18;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.mana = 5;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<Crimterry>();
        Item.shootSpeed = 5;
        Item.knockBack = 0.2f;
        Item.width = 40;
        Item.height = 60;
        Item.value = Item.buyPrice(0, 1);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.CrimtaneBar, 10)
        .AddIngredient(ItemID.Vertebrae, 8)
        .AddTile(TileID.Anvils)
        .Register();

    private class Crimterry : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        private Player Owner => Projectile.Owner();

        public override void SetStaticDefaults() => Main.projFrames[Type] = 2;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(46, 46);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => Utilities.CanHitLine(Projectile, Owner) ? null : false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (!Despawning)
            {
                float moveSpeed = Projectile.velocity.Length() * 0.04f + 0.08f;

                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 10;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * moveSpeed;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<HemophilicHatch>(Projectile, ref Time, null);

                if (Projectile.DistanceSQ(Owner.Center) > GetRangeSq<HemophilicHatch>() || !Utilities.CanHitLine(Projectile, Owner))
                    Projectile.velocity += Projectile.DirectionTo(Owner.Center) * 1.5f * moveSpeed;

                Projectile.frame = (int)(Time / 5f % 2);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity.Y -= 0.4f;

                if (Projectile.Opacity < 0.1f)
                    Projectile.Kill();
            }
        }
    }
}
