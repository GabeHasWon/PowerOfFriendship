﻿using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class IcemanEmblem : Talisman
{
    protected override float TileRange => 20;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 12;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.mana = 6;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<Frosty>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.width = 40;
        Item.height = 60;
        Item.value = Item.buyPrice(0, 0, 10);
        Item.noMelee = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SnowBlock, 20)
            .AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.FallenStar, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    private class Frosty : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(64);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation += 0.02f * Projectile.velocity.X;

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 9;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.5f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<IcemanEmblem>(Projectile, ref Time, 0.75f);

                if (Time % 20 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ChooseDust(), Projectile.velocity.X, Projectile.velocity.Y);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.velocity.Y += 0.4f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.95f;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.95f;

            for (int i = 0; i < 12; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ChooseDust(), oldVelocity.X, oldVelocity.Y);

            return false;
        }

        private static int ChooseDust() => Main.rand.NextBool(3) ? DustID.Ice : DustID.Snow;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
                target.AddBuff(BuffID.Frostburn, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k % 2 == 0)
                    continue;

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
