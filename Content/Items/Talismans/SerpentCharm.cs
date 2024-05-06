using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class SerpentCharm : Talisman
{
    protected override float TileRange => 75;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Lime;
        Item.damage = 90;
        Item.useTime = 13;
        Item.useAnimation = 13;
        Item.mana = 5;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<SerpentOuroboros>();
        Item.knockBack = 0.8f;
        Item.shootSpeed = 5;
        Item.width = 40;
        Item.height = 60;
        Item.value = Item.buyPrice(0, 15);
        Item.noMelee = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class SerpentOuroboros : ModProjectile
    {
        public class Segment : Entity
        {
            private readonly Entity _parent;
            private readonly bool _isTail;

            public float opacity = 1f;

            private float _rot = 0;

            public Segment(Vector2 pos, Entity parent, bool isTail)
            {
                position = pos;
                _parent = parent;
                _isTail = isTail;
            }

            public void Update()
            {
                float bodyLength = _parent is Projectile ? 28f : 32f;

                if (DistanceSQ(_parent.Center) > bodyLength * bodyLength)
                    Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - bodyLength);

                if (_parent.Center != Center)
                    _rot = AngleTo(_parent.Center);
            }

            public void Draw()
            {
                var tex = TextureAssets.Projectile[ModContent.ProjectileType<SerpentOuroboros>()].Value;
                var col = Lighting.GetColor(Center.ToTileCoordinates());
                var src = new Rectangle(_isTail ? 0 : 36, 0, 34, 38);
                float rot = _rot;
                SpriteEffects flip = SpriteEffects.None;

                if (_parent.Center.X < Center.X)
                {
                    rot -= MathHelper.Pi;
                    flip = SpriteEffects.FlipHorizontally;
                }

                Main.EntitySpriteDraw(tex, Center - Main.screenPosition, src, col * opacity, rot, src.Size() / 2f, 1f, flip, 0);
            }
        }

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float KillTime => ref Projectile.ai[2];

        private readonly List<Segment> _segments = [];

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
            Projectile.localNPCHitCooldown = 20;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            foreach (var item in _segments)
                item.Update();

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    float dist = Projectile.Distance(Main.MouseWorld);
                    float speed = 8 * (dist > 80 ? 1f : dist / 80f);

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 1.2f;

                    if (Projectile.velocity.LengthSquared() > speed * speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * speed;
                }

                Despawning = HandleBasicFunctions<SerpentCharm>(Projectile, ref Time, 1.5f);
            }
            else
            {
                if (_segments.Count == 0)
                {
                    Dust.NewDust(Projectile.position, 36, 36, Main.rand.NextBool(5) ? DustID.GreenMoss : DustID.Lihzahrd);
                    Projectile.Kill();
                    return;
                }

                KillTime += 0.85f;

                if (KillTime > 2)
                {
                    var seg = _segments.Last();

                    for (int i = 0; i < 6; ++i)
                        Dust.NewDust(seg.position, 36, 36, Main.rand.NextBool(5) ? DustID.GreenMoss : DustID.Lihzahrd);

                    _segments.Remove(seg);
                    KillTime = 0;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox)) 
                return true;
            else
            {
                foreach (var item in _segments)
                {
                    if (targetHitbox.Intersects(item.Hitbox))
                        return true;
                }
            }

            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_segments.Count == 0 && !Despawning)
                SpawnBody();

            foreach (var item in _segments)
                item.Draw();

            var tex = TextureAssets.Projectile[ModContent.ProjectileType<SerpentOuroboros>()].Value;
            Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
            float rot = Projectile.rotation;
            SpriteEffects flip = SpriteEffects.None;

            if (Projectile.velocity.X < 0)
            {
                rot -= MathHelper.Pi;
                flip = SpriteEffects.FlipHorizontally;
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(72, 0, 60, 38), col, rot, new(30, 19), 1f, flip, 0);

            return false;
        }

        private void SpawnBody()
        {
            const int Length = 30;

            Segment parent = null;

            for (int i = 0; i < Length; ++i)
            {
                parent = new Segment(Projectile.Center, parent ?? (Entity)Projectile, i == Length - 1);
                _segments.Add(parent);
            } 
        }
    }
}
