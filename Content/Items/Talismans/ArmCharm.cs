using System;
using Terraria.Audio;

namespace PoF.Content.Items.Talismans;

internal class ArmCharm : Talisman
{
    protected override float TileRange => 28;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 26;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.mana = 9;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<Humerus>();
        Item.shootSpeed = 5;
        Item.width = 38;
        Item.height = 46;
        Item.value = Item.buyPrice(0, 0, 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 30)
            .AddIngredient(ItemID.Cobweb, 8)
            .AddIngredient(ItemID.Chain, 6)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class Humerus : ModProjectile
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
            Projectile.Size = new Vector2(56);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X / 30f;

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 9;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.3f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                if (Projectile.DistanceSQ(Projectile.Owner().Center) > GetRangeSq<ArmCharm>())
                    Projectile.velocity += Projectile.DirectionTo(Projectile.Owner().Center) * 0.45f;

                Projectile.timeLeft++;

                bool paidMana = true;

                if (Time++ > Projectile.Owner().HeldItem.useTime)
                {
                    paidMana = Projectile.Owner().CheckMana(Projectile.Owner().HeldItem.mana, true);
                    Projectile.Owner().manaRegenDelay = (int)Projectile.Owner().maxRegenDelay;
                    Time = 0;
                }

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
                Projectile.velocity *= 0.92f;

                if (Projectile.velocity.LengthSquared() < 0.2f * 0.2f)
                {
                    Projectile.Kill();

                    for (int i = 0; i < 12; ++i)
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone);
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            bool xHit = Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon;
            bool yHit = Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon;

            if (xHit)
                Projectile.velocity.X = -oldVelocity.X;

            if (yHit)
                Projectile.velocity.Y = -oldVelocity.Y;

            if (xHit || yHit)
                Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4);

            for (int i = 0; i < 4; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, oldVelocity.X, oldVelocity.Y);

            return false;
        }
    }
}
