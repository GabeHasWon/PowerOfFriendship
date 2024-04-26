using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class DevourerCharm : Talisman
{
    protected override float TileRange => 25;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<HemophilicHatch>();
    }

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 18;
        Item.useTime = 13;
        Item.useAnimation = 13;
        Item.mana = 7;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<EaterOfTreats>();
        Item.knockBack = 0.8f;
        Item.shootSpeed = 5;
        Item.width = 40;
        Item.height = 60;
        Item.value = Item.buyPrice(0, 1);
        Item.noMelee = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.DemoniteBar, 10)
        .AddIngredient(ItemID.RottenChunk, 8)
        .AddTile(TileID.Anvils)
        .Register();

    private class EaterOfTreats : ModProjectile
    {
        public class Segment : Entity
        {
            private readonly Entity _parent;
            private readonly bool _isTail;
            private readonly int _frame = 0;

            public float opacity = 1f;

            private float _rot = 0;

            public Segment(Vector2 pos, Entity parent, bool isTail)
            {
                position = pos;
                _parent = parent;
                _isTail = isTail;
                
                if (!isTail)
                    _frame = Main.rand.Next(2);
            }

            public void Update()
            {
                float bodyLength = _parent is Projectile ? 20 : 22f;

                if (DistanceSQ(_parent.Center) > bodyLength * bodyLength)
                    Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - bodyLength);

                if (_parent.Center != Center)
                    _rot = AngleTo(_parent.Center) + MathHelper.PiOver2;
            }

            public void Draw()
            {
                if (_parent is Projectile proj && proj.Opacity < 0.2f)
                    opacity *= 0.85f;
                else if (_parent is Segment segment && segment.opacity < 0.2f)
                    opacity *= 0.85f;

                var tex = TextureAssets.Projectile[ModContent.ProjectileType<EaterOfTreats>()].Value;
                var col = Lighting.GetColor(Center.ToTileCoordinates());
                var src = new Rectangle(_frame * 36, _isTail ? 62 : 38, 36, 22);
                Main.EntitySpriteDraw(tex, Center - Main.screenPosition, src, col * opacity, _rot, src.Size() / 2f, 1f, SpriteEffects.None, 0);
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
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => Utilities.CanHitLine(Projectile, Projectile.Owner()) ? null : false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            foreach (var item in _segments)
                item.Update();

            if (!Despawning)
            {
                Vector2 sine = new Vector2(0, MathF.Sin(Time * 0.12f) * 60).RotatedBy(Projectile.velocity.ToRotation());

                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 8;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld + sine) * 1.2f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<DevourerCharm>(Projectile, ref Time, 1.5f);
            }
            else
            {
                Projectile.Opacity *= 0.8f;
                Projectile.velocity *= 0.95f;

                if (KillTime++ > 100)
                    Projectile.Kill();
            }
        }

        public override void DrawBehind(int index, List<int> bNT, List<int> behindNPCs, List<int> behindProjs, List<int> players, List<int> wires) => bNT.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            if (_segments.Count == 0)
                SpawnBody();

            foreach (var item in _segments)
                item.Draw();

            var tex = TextureAssets.Projectile[ModContent.ProjectileType<EaterOfTreats>()].Value;
            Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 36, 36), col, Projectile.rotation, new(18), 1f, SpriteEffects.None, 0);

            return false;
        }

        private void SpawnBody()
        {
            const int Length = 3;

            Segment parent = null;

            for (int i = 0; i < Length; ++i)
            {
                parent = new Segment(Projectile.Center, parent ?? (Entity)Projectile, i == Length - 1);
                _segments.Add(parent);
            } 
        }
    }
}
