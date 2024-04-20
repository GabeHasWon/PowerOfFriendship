using PoF.Common.Globals.ProjectileGlobals;
using ReLogic.Content;
using System.IO;
using Terraria.GameContent;

namespace PoF.Content.Items.Talismans;

internal class RingOnAString : Talisman
{
    protected override float TileRange => 70;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Purple;
        Item.damage = 36;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.mana = 8;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<RingController>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.Size = new(44, 40);
        Item.value = Item.buyPrice(0, 10);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class RingController : ModProjectile
    {
        private static Asset<Texture2D> RingTex;

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];

        private bool SpawnedButterflies
        {
            get => Projectile.ai[2] == 1;
            set => Projectile.ai[2] = value ? 1 : 0;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;

            RingTex = ModContent.Request<Texture2D>(Texture + "Ring");
        }

        public override void Unload() => RingTex = null;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(160);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.frame = (int)(Main.GameUpdateCount * 0.3f) % 4;

            if (!SpawnedButterflies)
            {
                for (int i = 0; i < 6; ++i)
                {
                    var src = Projectile.GetSource_FromAI();
                    int type = ModContent.ProjectileType<RingButterfly>();
                    int frame = Main.rand.Next(8) + 1;
                    var pos = Projectile.Center + new Vector2((i - 3) * 20, i * 20);

                    Projectile.NewProjectile(src, pos, Vector2.Zero, type, Projectile.damage, 0, Projectile.owner, Projectile.whoAmI, frame);
                }

                SpawnedButterflies = true;
            }

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 14;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 1.2f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;

                    Projectile.velocity *= 0.99f;
                }

                Despawning = HandleBasicFunctions<RingOnAString>(Projectile, ref Time, 1.5f);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.scale *= 1.03f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0 && Main.rand.NextBool(3))
            {
                int i = Item.NewItem(Projectile.GetSource_OnHit(target), target.Hitbox, ItemID.Heart);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 vector = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < 3; ++i)
            {
                float opacity = (0.6f - (i * 0.1f)) * Projectile.Opacity;
                float rot = (Main.GlobalTimeWrappedHourly * 6f + i) * (i % 2 == 0 ? -1 : 1);
                Color col = lightColor * opacity;

                Main.spriteBatch.Draw(RingTex.Value, vector, null, col, rot, RingTex.Size() / 2f, 0.6f + (i * 0.1f) * Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }

    public class RingButterfly : ModProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.Butterfly}";

        private Projectile Parent => Main.projectile[(int)ParentWhoAmI];

        private ref float ParentWhoAmI => ref Projectile.ai[0];
        private ref float ButterflyType => ref Projectile.ai[1];

        private Vector2 dir = Vector2.Zero;
        private Vector2 offset = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 24;
            TalismanGlobal.IsMinorTalismanProjectile.Add(Type);
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(160);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 45;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (dir == Vector2.Zero)
            {
                Projectile.netUpdate = true;
                dir = new Vector2(0, Main.rand.NextFloat(2, 10)).RotatedByRandom(MathHelper.TwoPi);
            }

            if (ButterflyType == 0)
                ButterflyType = Main.rand.Next(8) + 1;

            Projectile.frameCounter++;
            Projectile.frame = (int)(Projectile.frameCounter / 3f % 3);
            Projectile.Opacity = Parent.Opacity;

            if (!Parent.active || Parent.type != ModContent.ProjectileType<RingController>())
                Projectile.Kill();

            if (offset.LengthSquared() > 56 * 56)
                dir += offset.DirectionTo(Vector2.Zero) * 1.4f;

            offset += dir;
            Projectile.Center = Parent.Center + offset;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(dir);
        public override void ReceiveExtraAI(BinaryReader reader) => dir = reader.ReadVector2();

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0 && Main.rand.NextBool(3))
            {
                int i = Item.NewItem(Projectile.GetSource_OnHit(target), target.Hitbox, ItemID.Heart);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = TextureAssets.Projectile[Type].Value;
            var src = texture.Frame(1, 24, 0, (int)ButterflyType + Projectile.frame, 0, 0);
            var flip = dir.X <= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var col = lightColor * Projectile.Opacity;
            
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, src, col, Parent.velocity.X * 0.02f, src.Size() / 2f, 1f, flip, 0);
            return false;
        }
    }
}
