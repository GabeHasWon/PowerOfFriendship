using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class ToothTalisman : Talisman
{
    protected override float TileRange => 50;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 49;
        Item.useTime = 13;
        Item.useAnimation = 13;
        Item.mana = 7;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<ToothCreature>();
        Item.knockBack = 0.8f;
        Item.shootSpeed = 5;
        Item.width = 40;
        Item.height = 60;
        Item.value = Item.buyPrice(0, 1);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<RelicOfIce>()
            .AddIngredient<UnholyAmulet>()
            .AddIngredient<DevourerCharm>()
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    private class ToothCreature : ModProjectile
    {
        public class Segment : Entity
        {
            private readonly Entity _parent;
            private readonly int _frame = 0;
            private readonly bool _baby = false;

            public float opacity = 1f;

            private float _rot = 0;

            public Segment(Vector2 pos, Entity parent, bool baby)
            {
                position = pos;
                _parent = parent;
                _frame = Main.rand.Next(2);
                _baby = baby;
            }

            public void Update()
            {
                float bodyLength = _baby ? (_parent is Projectile ? 6 : 8) : (_parent is Projectile ? 20 : 16);

                if (DistanceSQ(_parent.Center) > bodyLength * bodyLength)
                    Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - bodyLength);

                if (_parent.Center != Center)
                    _rot = AngleTo(_parent.Center);
            }

            public void Draw(float baseOpacity, int childId)
            {
                float factor = _parent is Projectile projectile ? projectile.Opacity : (_parent as Segment).opacity;
                opacity = factor;

                var tex = _baby ? BabyTexture.Value : TextureAssets.Projectile[ModContent.ProjectileType<ToothCreature>()].Value;
                var col = Lighting.GetColor(Center.ToTileCoordinates());
                var src = _baby ? new Rectangle(_frame * 10, (childId - 1) * 20, 8, 18) : new Rectangle(_frame * 22, 0, 20, 40);
                Main.EntitySpriteDraw(tex, Center - Main.screenPosition, src, col * opacity * baseOpacity, _rot, src.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
        }

        public static Asset<Texture2D> BabyTexture;

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float KillTime => ref Projectile.ai[2];

        private Projectile Parent => Main.projectile[_parentWhoAmI];

        private readonly List<Segment> _segments = [];
        
        private int _childId = 0;
        private int _parentWhoAmI = 0;

        public override void SetStaticDefaults() => BabyTexture = ModContent.Request<Texture2D>(Texture + "Mini");

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(40);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (_childId == 0)
            {
                _childId = -1;

                for (int i = 0; i < 3; ++i)
                {
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, Type, Projectile.damage / 4, 0, Projectile.owner);

                    Projectile projectile = Main.projectile[proj];
                    projectile.Size = new Vector2(18);

                    ToothCreature newCreature = projectile.ModProjectile as ToothCreature;
                    newCreature._childId = i + 1;
                    newCreature._parentWhoAmI = Projectile.whoAmI;
                }
            }

            foreach (var item in _segments)
                item.Update();

            if (!Despawning)
            {
                const float Speed = 10;

                Vector2 sine = new Vector2(0, MathF.Sin(Time * 0.16f) * 90).RotatedBy(Projectile.velocity.ToRotation());

                if (_childId <= 0)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld + sine) * 1.2f;

                        if (Projectile.velocity.LengthSquared() > Speed * Speed)
                            Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                    }

                    if (Projectile.DistanceSQ(Projectile.Owner().Center) > GetRangeSq<ToothTalisman>())
                        Projectile.velocity += Projectile.DirectionTo(Projectile.Owner().Center + sine) * 1.5f;

                    bool paidMana = true;

                    if (Time++ % Projectile.Owner().HeldItem.useTime == 0)
                    {
                        paidMana = Projectile.Owner().CheckMana(Projectile.Owner().HeldItem.mana, true);
                        Projectile.Owner().manaRegenDelay = (int)Projectile.Owner().maxRegenDelay;
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
                    Vector2 offset = new Vector2(0, 80).RotatedBy(MathHelper.TwoPi / 3 * _childId);
                    Projectile.velocity = Projectile.velocity += Projectile.DirectionTo(Parent.Center + sine + offset) * 1.2f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;

                    if (!Parent.active || Parent.type != Type || Parent.Opacity < 0.3f)
                        Despawning = true;

                    if (Main.rand.NextBool(12))
                    {
                        int dustType = _childId switch
                        {
                            1 => DustID.GreenTorch,
                            2 => DustID.Frost,
                            _ => DustID.Torch
                        };
                        var last = _segments[^1];

                        Dust.NewDust(last.position, 16, 16, dustType);
                    }
                }

                Projectile.timeLeft++;
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.97f;

                if (Projectile.Opacity < 0.04f)
                    Projectile.Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_childId <= 0)
                return;

            target.AddBuff(_childId switch
            {
                1 => BuffID.CursedInferno,
                2 => BuffID.Frostburn,
                _ => BuffID.OnFire
            }, 300);
        }

        public override void DrawBehind(int index, List<int> bhT, List<int> bN, List<int> bP, List<int> players, List<int> wires) => bhT.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            if (_segments.Count == 0)
                SpawnBody();

            for (int i = 0; i < _segments.Count; i++)
            {
                Segment item = _segments[i];
                item.Draw(1 - (i / (float)_segments.Count), _childId);
            }

            var tex = _childId > 0 ? BabyTexture.Value : TextureAssets.Projectile[Type].Value;
            var src = _childId <= 0 ? new Rectangle(44, 0, 44, 40) : new Rectangle(20, 20 * (_childId - 1), 22, 18);
            Color col = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
            SpriteEffects flip = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, col, Projectile.rotation, src.Size() / 2f, 1f, flip, 0);

            return false;
        }

        private void SpawnBody()
        {
            const int Length = 14;

            Segment parent = null;

            for (int i = 0; i < Length; ++i)
            {
                parent = new Segment(Projectile.Center, parent ?? (Entity)Projectile, _childId > 0);
                _segments.Add(parent);
            } 
        }
    }
}
