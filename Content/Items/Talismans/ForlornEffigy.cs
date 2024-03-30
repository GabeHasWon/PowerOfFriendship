using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class ForlornEffigy : Talisman
{
    protected override float TileRange => 50;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 57;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.mana = 6;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<ForlornThing>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.Size = new(28, 32);
        Item.value = Item.buyPrice(0, 10);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<RelicOfIce>()
            .AddIngredient<UnholyAmulet>()
            .AddIngredient<HemophilicHatch>()
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    private class ForlornThing : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float ProjectileTime => ref Projectile.ai[2];

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
            Projectile.Size = new Vector2(52);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.X * 0.06f;

            if (!Despawning)
            {
                ProjectileTime++;

                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 9;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.85f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                if (ProjectileTime > 60 && Projectile.GetNearestNPCTarget(out NPC npc))
                {
                    Vector2 velocity = Projectile.DirectionTo(npc.Center) * 12;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        int type = ModContent.ProjectileType<ForlornSpit>();
                        int subType = Main.rand.Next(3);
                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, type, Projectile.damage, 2f, Projectile.owner, subType);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    }

                    for (int i = 0; i < 20; ++i)
                    {
                        Vector2 dustVel = velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat();
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ChooseDust(), dustVel.X, dustVel.Y);
                    }

                    if (Main.netMode != NetmodeID.Server)
                        SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.5f, PitchRange = (-0.2f, 0.2f) }, Projectile.Center);

                    ProjectileTime = 0;
                }

                Despawning = HandleBasicFunctions<ForlornEffigy>(Projectile, ref Time, 1.3f);

                if (Time % 6 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ChooseDust(), Projectile.velocity.X, Projectile.velocity.Y);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.scale *= 0.97f;

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

            for (int i = 0; i < 20; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ChooseDust(), oldVelocity.X, oldVelocity.Y);

            return false;
        }

        private static int ChooseDust() => Main.rand.NextBool() ? DustID.Blood : DustID.Crimslime;

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
            Vector2 sineScale = new(MathF.Sin(Main.GameUpdateCount * 0.14f + MathHelper.PiOver2) * 0.1f + 1, MathF.Sin(Main.GameUpdateCount * 0.14f) * 0.1f + 1);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k % 2 == 0)
                    continue;

                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.75f;
                Main.EntitySpriteDraw(texture, drawPos, null, color * Projectile.Opacity, Projectile.rotation, drawOrigin, sineScale * Projectile.scale, SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates(), Projectile.GetAlpha(Color.White)) * Projectile.Opacity;
            Main.EntitySpriteDraw(texture, pos, null, col, Projectile.rotation, drawOrigin, sineScale * Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class ForlornSpit : ModProjectile
    {
        const int MaxTimeLeft = 240;

        private ref float SubType => ref Projectile.ai[0];

        private bool HitEnemy
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(22);
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxTimeLeft;
        }

        public override void AI()
        {
            Projectile.scale -= 0.002f;
            Projectile.frame = (int)SubType;
            Projectile.rotation += 0.008f * Projectile.velocity.X;

            if (HitEnemy)
                Projectile.velocity.Y += 0.1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitEnemy = true;

            target.AddBuff(SubType switch
            {
                0 => BuffID.Frostburn,
                1 => BuffID.Ichor,
                _ => BuffID.OnFire
            }, 180);
        }
    }
}
