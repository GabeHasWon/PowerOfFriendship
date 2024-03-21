using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class PartyTrick : Talisman
{
    protected override float TileRange => 40;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 38;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 6;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<PartyTrickBalloon>();
        Item.shootSpeed = 5;
        Item.knockBack = 0.2f;
        Item.value = Item.buyPrice(0, 10, 0, 0);
        Item.width = 34;
        Item.height = 44;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) => gravity *= 0.2f;
    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class PartyTrickBalloon : ModProjectile
    {
        private static int[] BuffIds = [BuffID.OnFire, BuffID.Midas, BuffID.Poisoned, BuffID.Lovestruck, BuffID.Wet, BuffID.Venom, BuffID.Ichor, BuffID.CursedInferno, BuffID.Frostburn];

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(36);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation += 0.02f * Projectile.velocity.Length();

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 12;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.8f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<PartyTrick>(Projectile, ref Time, 1.5f);

                if (Time % 15 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, Projectile.velocity.X, Projectile.velocity.Y);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity = Projectile.velocity.RotatedBy(0.1f);

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            bool x = Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon;
            bool y = Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon;

            if (x)
                Projectile.velocity.X = -oldVelocity.X;

            if (y)
                Projectile.velocity.Y = -oldVelocity.Y;

            if (x || y)
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.3f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity *= -1.6f;
            Projectile.velocity = Projectile.velocity.RotatedByRandom(0.5f);

            target.AddBuff(Main.rand.Next(BuffIds), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k % 2 == 1)
                    continue;

                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.8f;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
