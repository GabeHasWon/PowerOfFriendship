namespace PoF.Content.Items.Talismans;

internal class TwoTopTalisman : Talisman
{
    protected override float TileRange => 22;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 7;
        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.mana = 2;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<TwoTop>();
        Item.shootSpeed = 5;
        Item.width = 38;
        Item.height = 48;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Mushroom, 8)
            .AddIngredient(ItemID.GlowingMushroom, 20)
            .AddIngredient(ItemID.WhiteString, 6)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
    public override void Update(ref float gravity, ref float maxFallSpeed) => Lighting.AddLight(Item.Center, new Vector3(0.1f, 0.1f, 0.2f));

    private class TwoTop : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float KillTime => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.X / 10f;

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 6;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.7f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                if (Projectile.DistanceSQ(Projectile.Owner().Center) > GetRangeSq<TwoTopTalisman>())
                    Projectile.velocity += Projectile.DirectionTo(Projectile.Owner().Center) * 1.15f;

                Projectile.timeLeft++;

                bool paidMana = true;

                if (Utilities.CanHitLine(Projectile, Projectile.Owner()))
                    Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);
                else
                    Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.1f, 0.1f);

                if (Time++ > Projectile.Owner().HeldItem.useTime)
                {
                    paidMana = Projectile.Owner().CheckMana(Projectile.Owner().HeldItem.mana, true);
                    Projectile.Owner().manaRegenDelay = (int)Projectile.Owner().maxRegenDelay;
                    Time = 0;

                    SpawnProj(Projectile.GetSource_FromAI(), 1f);
                }

                if (Time % 15 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GlowingMushroom, Projectile.velocity.X, Projectile.velocity.Y);

                if (!Projectile.Owner().channel)
                    Despawning = true;

                if (!paidMana)
                {
                    Projectile.Owner().channel = false;
                    Despawning = true;
                }
            }
            else
            {
                Projectile.Opacity *= 0.94f;
                Projectile.velocity *= 0.95f;

                KillTime++;

                if (KillTime == 60)
                {
                    for (int i = 0; i < 8; ++i)
                        SpawnProj(Projectile.GetSource_Death(), Main.rand.NextFloat(0.2f, 1.2f));

                    Projectile.scale = 0.5f;
                }
                else if (KillTime > 180)
                    Projectile.Kill();

            }
        }

        private void SpawnProj(IEntitySource source, float velMul)
        {
            Vector2 vel = Projectile.velocity * 0.4f + new Vector2(0, 4).RotatedByRandom(MathHelper.Pi) * velMul;
            Projectile.NewProjectile(source, Projectile.Center, vel, ModContent.ProjectileType<TwoTopSpores>(), Projectile.damage, 0, Projectile.owner);
        }
    }

    public class TwoTopSpores : ModProjectile
    {
        const int MaxTimeLeft = 240;

        private ref float BaseOpacity => ref Projectile.ai[0];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(22);
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.frame = Main.rand.Next(4);
            Projectile.timeLeft = MaxTimeLeft;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == MaxTimeLeft)
                BaseOpacity = Main.rand.NextFloat(0.5f, 1f);

            Projectile.velocity *= 0.94f;
            Projectile.Opacity = Projectile.timeLeft / (float)MaxTimeLeft * BaseOpacity;
            Projectile.rotation += Projectile.velocity.X / 200f;
        }
    }
}
