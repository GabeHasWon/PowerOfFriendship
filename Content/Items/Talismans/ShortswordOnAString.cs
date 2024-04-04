using Microsoft.CodeAnalysis.Operations;
using System;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class ShortswordOnAString : Talisman
{
    protected override float TileRange => 60;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Purple;
        Item.damage = 68;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 5;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<ShortswordOnAStringProj>();
        Item.shootSpeed = 5;
        Item.knockBack = 0.2f;
        Item.Size = new(44, 30);
        Item.value = Item.buyPrice(0, 10);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.CopperShortsword)
            .AddIngredient(ItemID.SoulofMight, 5)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(ItemID.Cobweb, 16)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class ShortswordOnAStringProj : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float Rotation => ref Projectile.ai[2];

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
            Projectile.Size = new Vector2(22);
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            if (!Despawning)
            {
                Projectile.rotation = Rotation;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 oldPos = Projectile.Center;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.15f);

                    if (Projectile.DistanceSQ(Projectile.Owner().Center) > GetRangeSq<ShortswordOnAString>())
                        Projectile.Center += Projectile.DirectionTo(Projectile.Owner().Center) * (Projectile.Distance(Projectile.Owner().Center) - GetRange<ShortswordOnAString>());

                    Projectile.rotation = (Projectile.Center - oldPos).ToRotation();
                    Rotation = Projectile.rotation;
                }

                Despawning = HandleBasicFunctions<ShortswordOnAString>(Projectile, ref Time, null);

                if (Time % 15 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, Projectile.velocity.X, Projectile.velocity.Y);
            }
            else
            {
                Projectile.Opacity *= 0.85f;
                Projectile.Center = Vector2.Lerp(Projectile.Center, Projectile.Owner().Center, 0.1f);

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

            DrawString();
            return true;
        }

        private void DrawString()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 mountedCenter = player.MountedCenter;
            float polePosX = mountedCenter.X;
            float polePosY = mountedCenter.Y;
            polePosY += player.gfxOffY;

            if (player.mount.Active && player.mount.Type == 52)
            {
                polePosX -= player.direction * 14;
                polePosY -= -10f;
            }
            
            //stringColor = Projectile.TryApplyingPlayerStringColor(Main.player[Projectile.owner].stringColor, stringColor);

            Color stringColor = Color.White;
            float gravDir = Main.player[Projectile.owner].gravDir;

            if (gravDir == -1f)
                polePosY -= 12f;

            Vector2 drawPosition = new(polePosX, polePosY);
            drawPosition = Main.player[Projectile.owner].RotatedRelativePoint(drawPosition + new Vector2(8f)) - new Vector2(8f);
            float num = Projectile.Center.X * 0.5f - drawPosition.X;
            float num2 = Projectile.Center.Y - drawPosition.Y;
            bool runWhile = true;

            if (num == 0f && num2 == 0f)
                runWhile = false;
            else
            {
                float num4 = (float)Math.Sqrt(num * num + num2 * num2);
                num4 = 12f / num4;
                num2 *= num4;
                drawPosition.Y -= num2;

                Vector2 offset = new Vector2(-16, -1).RotatedBy(Projectile.rotation) - drawPosition;
                num = Projectile.Center.X + offset.X;
                num2 = Projectile.Center.Y + offset.Y;
            }

            Vector2 origin = new(TextureAssets.FishingLine.Width() * 0.5f, 0f);

            while (runWhile)
            {
                float num5 = 12f;
                float distance = (float)Math.Sqrt(num * num + num2 * num2);
                float num7 = distance;

                if (float.IsNaN(distance) || float.IsNaN(num7))
                    break;

                if (distance < 20f)
                {
                    num5 = distance - 8f;
                    runWhile = false;
                }
                distance = 12f / distance;
                num *= distance;
                num2 *= distance;
                drawPosition.X += num;
                drawPosition.Y += num2;

                Vector2 offset = new Vector2(-16, -1).RotatedBy(Projectile.rotation) - drawPosition;
                num = Projectile.Center.X + offset.X;
                num2 = Projectile.Center.Y + offset.Y;

                if (num7 > 12f)
                {
                    float num8 = 0.3f;
                    float velocityDot = Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y);

                    if (velocityDot > 16f)
                        velocityDot = 16f;

                    velocityDot = 1f - velocityDot / 16f;
                    num8 *= velocityDot;
                    velocityDot = num7 / 80f;
                    if (velocityDot > 1f)
                        velocityDot = 1f;
                    num8 *= velocityDot;
                    if (num8 < 0f)
                        num8 = 0f;
                    velocityDot = 1f - Projectile.localAI[0] / 100f;
                    num8 *= velocityDot;
                    if (num2 > 0f)
                    {
                        num2 *= 1f + num8;
                        num *= 1f - num8;
                    }
                    else
                    {
                        velocityDot = Math.Abs(Projectile.velocity.X) / 3f;
                        if (velocityDot > 1f)
                            velocityDot = 1f;
                        velocityDot -= 0.5f;
                        num8 *= velocityDot;
                        if (num8 > 0f)
                            num8 *= 2f;
                        num2 *= 1f + num8;
                        num *= 1f - num8;
                    }
                }

                float rotation = (float)Math.Atan2(num2, num) - MathHelper.PiOver2;
                Color color = Lighting.GetColor((int)drawPosition.X / 16, (int)(drawPosition.Y / 16f), stringColor);
                var pos = new Vector2(drawPosition.X + TextureAssets.FishingLine.Width() * 0.5f, drawPosition.Y + TextureAssets.FishingLine.Height() * 0.5f) - Main.screenPosition;
                var src = new Rectangle(0, 0, TextureAssets.FishingLine.Width(), (int)num5);

                Main.EntitySpriteDraw(TextureAssets.FishingLine.Value, pos, src, color * Projectile.Opacity, rotation, origin, 1f, 0, 0f);
            }
        }
    }
}
