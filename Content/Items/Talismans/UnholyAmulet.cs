using System;
using System.Runtime.InteropServices;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace PoF.Content.Items.Talismans;

internal class UnholyAmulet : Talisman
{
    protected override float TileRange => 30;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.CrossNecklace;
        ItemID.Sets.ShimmerTransformToItem[ItemID.CrossNecklace] = Type;
    }

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 32;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 7;
        Item.UseSound = SoundID.DD2_BetsyFireballShot;
        Item.shoot = ModContent.ProjectileType<UnholyFlame>();
        Item.shootSpeed = 5;
        Item.knockBack = 2f;
        Item.width = 40;
        Item.height = 56;
        Item.value = Item.buyPrice(0, 0, 75);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Hellstone, 20)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ownedProjectileCounts[Item.shoot] > 0)
            return false;

        if (player.ownedProjectileCounts[ModContent.ProjectileType<UnholyPentagram>()] <= 1)
            return true;

        Projectile aoe = null;

        for (int i = 0; i < Main.maxProjectiles; ++i)
        {
            Projectile proj = Main.projectile[i];

            if (proj.active && proj.type == ModContent.ProjectileType<UnholyPentagram>() && proj.timeLeft > 30 && proj.owner == player.whoAmI)
            {
                if (aoe is null)
                    aoe = proj;
                else if (aoe.timeLeft > proj.timeLeft)
                    aoe = proj;
            }
        }

        if (aoe is not null)
            aoe.timeLeft = 30;
        return true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) => Lighting.AddLight(Item.Center, new Vector3(0.1f, 0.1f, 0f));

    private class UnholyFlame : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float AnimTime => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;

            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(44);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => null;// Utilities.CanHitLine(Projectile, Projectile.Owner()) ? null : false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frame = (int)(AnimTime++ / 8f % 2);

            Lighting.AddLight(Projectile.Center, TorchID.Torch);

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 12;
                    const float MinSpeed = 4;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.8f;

                    float len = Projectile.velocity.LengthSquared();

                    if (len > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                    else if (len < MinSpeed * MinSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * MinSpeed;
                }

                Despawning = HandleBasicFunctions<UnholyAmulet>(Projectile, ref Time, 1.2f, dummyTime: (int)(Projectile.Owner().HeldItem.useTime * 1.5f));

                Projectile.Opacity = Utilities.CanHitLine(Projectile, Projectile.Owner())
                    ? MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f)
                    : MathHelper.Lerp(Projectile.Opacity, 0.1f, 0.1f);

                if (Main.rand.NextBool(3))
                {
                    float scale = Main.rand.NextFloat(0.8f, 2f);
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X, Projectile.velocity.Y, Scale: scale);
                }
            }
            else
            {
                Projectile.Opacity *= 0.85f;
                Projectile.velocity *= 0.97f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<UnholyPentagram>(), Projectile.damage, 0);

            for (int i = 0; i < 30; ++i)
            {
                float xSpeed = Main.rand.NextFloat(-6, 6);
                float ySpeed = Main.rand.NextFloat(-6, 6);
                Vector2 adjSpeed = Projectile.velocity + new Vector2(xSpeed, ySpeed);
                float scale = Main.rand.NextFloat(0.8f, 2f);

                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, adjSpeed.X, adjSpeed.Y, Scale: scale);
                Dust.NewDust(Projectile.position - new Vector2(100), 216, 216, DustID.RedTorch, xSpeed, ySpeed, Scale: Main.rand.NextFloat(0.8f, 2f));
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 12; ++i)
                {
                    Vector2 vel = new Vector2(0, Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.Pi);
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, vel, GoreID.Smoke1 + Main.rand.Next(3));
                }

                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.OnFire, 60);

        public override bool PreDraw(ref Color lightColor)
        {
            UnholyFlameDrawer magicMissileDrawer = default;
            magicMissileDrawer.Draw(Projectile);

            var tex = TextureAssets.Projectile[Type].Value;
            var src = tex.Frame(1, 2, 0, Projectile.frame);
            var col = Color.White * Projectile.Opacity;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, col, Projectile.rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
            return false;
        }
    }

    public class UnholyPentagram : ModProjectile
    {
        const int MaxTimeLeft = 600;

        private ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(216);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.Opacity = 0f;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            float sine = MathF.Pow(MathF.Sin(Timer++ * 0.02f), 2);
            Projectile.rotation += sine * 0.02f + 0.005f;

            if (Projectile.timeLeft > MaxTimeLeft - 60)
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.5f, 0.05f);
            else 
                Projectile.Opacity = Projectile.timeLeft > 30
                ? MathHelper.Lerp(Projectile.Opacity, sine * 0.5f + 0.25f, 0.2f)
                : MathHelper.Lerp(Projectile.Opacity, 0, 0.2f);
        }

        public override void PostDraw(Color lightColor)
        {
            var projTex = TextureAssets.Projectile[Type].Value;
            var baseCol = Color.White * Projectile.Opacity * 0.8f;
            Main.spriteBatch.Draw(projTex, Projectile.Center - Main.screenPosition, null, baseCol, Projectile.rotation, projTex.Size() / 2f, 1.1f, SpriteEffects.None, 0);

            var tex = ModContent.Request<Texture2D>(Texture + "Alpha").Value;

            for (int i = 0; i < 3; ++i)
            {
                float rot = Projectile.rotation * (i % 2 == 0 ? -1 : 1) + (i / MathHelper.TwoPi);
                Color color = Color.Red with { A = 0 } * Projectile.Opacity * (1 - (i * 0.33f));
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, rot, tex.Size() / 2f, 1f + (i * 0.25f), SpriteEffects.None, 0);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct UnholyFlameDrawer
    {
        private static readonly VertexStrip _vertexStrip = new();

        public readonly void Draw(Projectile proj)
        {
            MiscShaderData miscShaderData = GameShaders.Misc["PoF:UnholyFlame"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(2f * proj.Opacity);
            miscShaderData.Apply();
            _vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private readonly Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.Yellow, Color.Red, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A = (byte)(result.A * 0.7f);
            return result;
        }

        private readonly float StripWidth(float progress) => MathHelper.Lerp(30, 42f, Utils.GetLerpValue(0f, 0.2f, progress, true)) * Utils.GetLerpValue(0f, 0.07f, progress, true);
    }
}
