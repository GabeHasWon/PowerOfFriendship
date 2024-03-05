using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class Starcharm : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.StaffMinionSlotsRequired[Type] = 0;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 8;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 5;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = false;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<Starkid>();
        Item.shootSpeed = 5;
        Item.channel = true;
        Item.DamageType = TalismanDamageClass.Self;
        Item.width = 34;
        Item.height = 44;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Star, 5)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) => Lighting.AddLight(Item.Center, new Vector3(0.1f, 0.1f, 0f));

    private class Starkid : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.minionSlots = 0;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => Utilities.CanHitLine(Projectile, Projectile.Owner()) ? null : false;

        public override void AI()
        {
            Projectile.rotation += 0.3f;

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 8;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.7f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

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
                }

                if (Time % 15 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, Projectile.velocity.X, Projectile.velocity.Y);

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
                Projectile.Opacity *= 0.85f;
                Projectile.velocity *= 1.1f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
