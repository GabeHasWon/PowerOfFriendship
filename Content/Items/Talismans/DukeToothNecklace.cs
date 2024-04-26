using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class DukeToothNecklace : Talisman
{
    protected override float TileRange => 75;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 96;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.mana = 8;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<DukePish>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.Size = new(36);
        Item.value = Item.buyPrice(0, 0, 10);
        Item.noMelee = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class DukePish : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float DashTime => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(54, 34);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.extraUpdates = Projectile.wet.ToInt();
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frame = (int)(Projectile.frameCounter++ / 10f % 4);

            float speedMult = 1 - Projectile.extraUpdates * 0.25f;

            if (Projectile.velocity.X < 0)
            {
                Projectile.rotation += MathHelper.Pi;
                Projectile.spriteDirection = 1;
            }
            else
                Projectile.spriteDirection = -1;

            if (!Despawning)
            {
                if (DashTime < 0)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        float maxSpeed = 12 * speedMult;

                        Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.5f * speedMult;

                        if (Projectile.velocity.LengthSquared() > maxSpeed * maxSpeed)
                            Projectile.velocity = Projectile.velocity.SafeNormalize() * maxSpeed;

                        if (Projectile.DistanceSQ(Main.MouseWorld) < 200 * 200)
                        {
                            Projectile.netUpdate = true;
                            Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * 18 * speedMult;
                            DashTime = 8;
                        }
                    }
                }
                else
                    DashTime--;

                Despawning = HandleBasicFunctions<DukeToothNecklace>(Projectile, ref Time, 0.75f);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.scale *= 0.95f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (DashTime < 0)
                return true;

            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                //if (k % 2 == 0)
                //    continue;

                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                Rectangle src = new(0, 36 * ((Projectile.frame + k) % 4), 54, 34);
                Main.EntitySpriteDraw(texture, drawPos, src, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
