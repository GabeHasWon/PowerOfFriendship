using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class KissOfDeath : Talisman
{
    protected override float TileRange => 70;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 70;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 6;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<KissOfDeathLance>();
        Item.shootSpeed = 5;
        Item.knockBack = 0.1f;
        Item.value = Item.buyPrice(0, 15, 0, 0);
        Item.Size = new(30, 40);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class KissOfDeathLance : ModProjectile
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
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (!Despawning)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = Rotation;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 oldPos = Projectile.Center;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.05f);

                    if (Projectile.DistanceSQ(Projectile.Owner().Center) > GetRangeSq<KissOfDeath>())
                        Projectile.Center += MovementSpeed();

                    Projectile.rotation = (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2;
                    Rotation = Projectile.rotation;
                }

                Despawning = HandleBasicFunctions<KissOfDeath>(Projectile, ref Time, null);

                if (Time % 15 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Clentaminator_Purple, Projectile.velocity.X, Projectile.velocity.Y);

                if (Despawning)
                    Projectile.velocity = Vector2.Lerp(Projectile.Center, Main.MouseWorld, 0.05f) - Projectile.Center;
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.scale *= 0.99f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        private Vector2 MovementSpeed() => Projectile.DirectionTo(Projectile.Owner().Center) * (Projectile.Distance(Projectile.Owner().Center) - GetRange<KissOfDeath>());
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.FinalDamage += (Projectile.Center - Projectile.oldPos[0]).Length() / 150f;

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
