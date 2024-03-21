using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class RelicOfIce : Talisman
{
    protected override float TileRange => 45;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.LightPurple;
        Item.damage = 24;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 8;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<RelicOfFrosty>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.width = 50;
        Item.height = 54;
        Item.value = Item.buyPrice(0, 5);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<IcemanEmblem>()
            .AddIngredient(ItemID.FrostCore, 1)
            .AddIngredient(ItemID.SoulofLight, 5)
            .AddIngredient(ItemID.SoulofNight, 5)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    private class RelicOfFrosty : ModProjectile
    {
        private readonly Color[] AuraColor = [new Color(106, 147, 183), new Color(190, 210, 219), Color.White];

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        private bool SpawnedProjectiles
        {
            get => Projectile.ai[2] == 1;
            set => Projectile.ai[2] = value ? 1 : 0;
        }

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
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation = Main.GameUpdateCount * 0.06f;

            if (!SpawnedProjectiles && Projectile.owner == Main.myPlayer)
                SpawnProjectiles();

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 12;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.75f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<RelicOfIce>(Projectile, ref Time, 1f);

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

        private void SpawnProjectiles()
        {
            for (int i = 0; i < 20; ++i)
            {
                bool snowBall = i < 5;
                var src = Projectile.GetSource_FromAI();
                int type = snowBall ? ModContent.ProjectileType<RelicSnowballs>() : ModContent.ProjectileType<RelicIceShards>();
                int dam = (int)(Projectile.damage * 0.6f);
                float scale = Main.rand.NextFloat(0.6f, 1f);
                int proj = Projectile.NewProjectile(src, Projectile.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi), type, dam, snowBall ? 0.4f : 0, Projectile.owner, Projectile.whoAmI);

                Main.projectile[proj].scale = scale;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }

            SpawnedProjectiles = true;
            Projectile.netUpdate = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => BounceOffTiles(Projectile, oldVelocity);

        public static bool BounceOffTiles(Projectile proj, Vector2 oldVelocity)
        {
            Collision.HitTiles(proj.position, proj.velocity, proj.width, proj.height);
            SoundEngine.PlaySound(SoundID.Dig, proj.position);

            if (Math.Abs(proj.velocity.X - oldVelocity.X) > float.Epsilon)
                proj.velocity.X = -oldVelocity.X * 0.95f;

            if (Math.Abs(proj.velocity.Y - oldVelocity.Y) > float.Epsilon)
                proj.velocity.Y = -oldVelocity.Y * 0.95f;

            for (int i = 0; i < 12; ++i)
                Dust.NewDust(proj.position, proj.width, proj.height, ChooseDust(), oldVelocity.X, oldVelocity.Y);

            return false;
        }

        private static int ChooseDust() => !Main.rand.NextBool(3) ? DustID.Ice : DustID.Snow;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Frostburn, 420);

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(42, 52);
            Color baseColor = Projectile.GetAlpha(lightColor);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k % 2 == 0)
                    continue;

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = baseColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, baseColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            var tex = ModContent.Request<Texture2D>(Texture.Replace("RelicOfFrosty", "RelicAura")).Value;
            var pos = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < AuraColor.Length; ++i)
            {
                var col = Lighting.GetColor(Projectile.Center.ToTileCoordinates(), AuraColor[i]) with { A = 0 } * Projectile.Opacity;
                float mul = (i + 1) * 0.33f;
                float rotation = Main.GameUpdateCount * (0.08f + (i * 0.1f)) + i;

                Main.EntitySpriteDraw(tex, pos, null, col * mul * 0.7f, rotation, tex.Size() / 2f, 1.5f - mul + Projectile.scale, SpriteEffects.None, 0);
            }
        }
    }

    public class RelicIceShards : ModProjectile
    {
        private Projectile Owner => Main.projectile[(int)OwnerWho];

        private ref float OwnerWho => ref Projectile.ai[0];

        private bool OrbitsOwner
        {
            get => Projectile.ai[1] == 1;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        private ref float Timer => ref Projectile.ai[2];

        private Vector2 dir = Vector2.Zero;
        private Vector2 offset = Vector2.Zero;

        public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(24, 22);
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = 120;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.frame = Main.rand.Next(4);
            Projectile.timeLeft = 20;
        }

        public override void AI()
        {
            if (dir == Vector2.Zero)
            {
                OrbitsOwner = Main.rand.NextBool();
                Projectile.netUpdate = true;

                if (!OrbitsOwner)
                    dir = new Vector2(0, Main.rand.NextFloat(6, 10)).RotatedByRandom(MathHelper.TwoPi);
                else
                    dir = new Vector2(0, Main.rand.NextFloat(70)).RotatedByRandom(MathHelper.TwoPi);
            }

            Projectile.timeLeft++;
            Projectile.alpha = Owner.alpha;

            if (!OrbitsOwner)
            {
                if (offset.LengthSquared() > 80 * 80)
                    dir += offset.DirectionTo(Vector2.Zero) * 2f;
            }
            else
            {
                offset = dir;
                dir = dir.RotatedBy(0.08f * Projectile.scale);
            }

            if ((Timer + Projectile.whoAmI) % 60 == 0)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, Projectile.velocity.X, Projectile.velocity.Y);

            offset += dir;
            Projectile.Center = Owner.Center + offset;

            if (!Owner.active || Owner.type != ModContent.ProjectileType<RelicOfFrosty>())
                Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => RelicOfFrosty.BounceOffTiles(Projectile, oldVelocity);
    }

    public class RelicSnowballs : RelicIceShards
    {
        public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.Size = new Vector2(36);
            Projectile.frame = Main.rand.Next(4);
            Projectile.penetrate = 1;
        }
    }
}
