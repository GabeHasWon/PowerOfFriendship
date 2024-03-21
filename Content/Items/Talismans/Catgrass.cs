using Terraria.Audio;

namespace PoF.Content.Items.Talismans;

internal class Catgrass : Talisman
{
    const int MaxFireRate = 100;

    protected override float TileRange => 60;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 120;
        Item.useTime = Item.useAnimation = MaxFireRate;
        Item.mana = 20;
        Item.shoot = ModContent.ProjectileType<CatgrassProj>();
        Item.shootSpeed = 5;
        Item.width = 38;
        Item.height = 46;
        Item.value = Item.buyPrice(0, 5);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class CatgrassProj : ModProjectile
    {
        public override string Texture => base.Texture.Replace("Proj", "");

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

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
            Projectile.hide = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player owner = Projectile.Owner();

            Projectile.rotation += Projectile.velocity.X / 30f;
            Projectile.Center = owner.Center;

            if (!Despawning)
            {
                Despawning = HandleBasicFunctions<ArmCharm>(Projectile, ref Time, 0.45f);

                if (Time == owner.HeldItem.useTime / 2)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        var dir = new Vector2(owner.direction * 8, 0);
                        int type = ModContent.ProjectileType<CatgrassBubble>();
                        var pos = owner.Center + dir;
                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, dir / 2, type, Projectile.damage, 4f, Projectile.owner);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    }

                    SoundEngine.PlaySound(SoundID.Item85, Projectile.Center);
                }
            }
            else
            {
                Projectile.velocity *= 0.92f;

                if (Projectile.velocity.LengthSquared() < 0.2f * 0.2f)
                    Projectile.Kill();
            }
        }
    }

    public class CatgrassBubble : ModProjectile
    {
        private const int MaxTimeLeft = MaxFireRate * 3 - 20;
        private const int ExplosionTimeLeft = 4;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(56) * 2;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.scale = 0.5f;
            Projectile.timeLeft = MaxTimeLeft;
        }

        public override void AI()
        {
            Player owner = Projectile.Owner();

            if (Main.myPlayer == owner.whoAmI)
            {
                const float Speed = 10;

                Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.75f;

                if (Projectile.velocity.LengthSquared() > Speed * Speed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
            }

            if (!owner.channel && Projectile.timeLeft > ExplosionTimeLeft)
                Projectile.timeLeft = ExplosionTimeLeft;

            if (Projectile.scale < 1f)
                Projectile.scale *= 1.04f;

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            if (Projectile.timeLeft == ExplosionTimeLeft)
            {
                static int RandomDirection() => Main.rand.NextBool() ? -1 : 1;

                for (int i = 0; i < 20; ++i)
                {
                    Vector2 dir = new(Main.rand.NextFloat(3, 5f) * RandomDirection(), Main.rand.NextFloat(3, 5f) * RandomDirection());
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BubbleBlock, dir.X, dir.Y, 0);
                }

                Projectile.Opacity = 0;
                Projectile.Resize(112, 112);
                Projectile.Damage();

                SoundEngine.PlaySound(SoundID.Item54 with { PitchRange = (-1f, 0.6f) }, Projectile.Center);
            }

            BubbleCollision();
        }

        private void BubbleCollision()
        {
            for (int i = 0; i < Main.maxProjectiles; ++i)
            {
                if (i == Projectile.whoAmI)
                    continue;

                Projectile proj = Main.projectile[i];

                if (proj.active && proj.type == Type && proj.DistanceSQ(Projectile.Center) < 56 * 56)
                {
                    Projectile.velocity = Projectile.DirectionFrom(proj.Center) * Projectile.velocity.Length();
                    proj.velocity = proj.DirectionFrom(Projectile.Center) * proj.velocity.Length();
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > ExplosionTimeLeft + 1)
                Projectile.timeLeft = ExplosionTimeLeft + 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > ExplosionTimeLeft + 1)
                Projectile.timeLeft = ExplosionTimeLeft + 1;

            return false;
        }
    }
}