using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Whips;

public class TheStinger : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<TheStingerProj>(), 23, 2, 4);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.buyPrice(0, 0, 50);
    }

    public override bool MeleePrefix() => true;

    public class TheStingerProj : ModProjectile
    {
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults() => ProjectileID.Sets.IsAWhip[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 16;
            Projectile.WhipSettings.RangeMultiplier = 1.2f;
        }

        public override bool PreAI()
        {
            List<Vector2> whipPoints = [];
            Projectile.FillWhipControlPoints(Projectile, whipPoints);

            foreach (var item in whipPoints)
            {
                if (Main.rand.NextBool(82))
                    Dust.NewDust(item, 8, 8, DustID.Honey2, 0, 0, 150, Color.Lerp(Color.White, Color.Orange, Main.rand.NextFloat(0, 0.75f)), 1f);
            }

            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            if (target.life <= 0)
            {
                int damage = (int)(Projectile.damage * 0.6f);

                for (int i = 0; i < 3; ++i)
                    Projectile.NewProjectile(target.GetSource_OnHurt(Projectile), target.Center, Vector2.Zero, ProjectileID.Bee, damage, 0);
            }

            Projectile.damage = (int)(Projectile.damage * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return WhipCommon.Draw(Projectile, Timer, new(0, 0, 10, 26), new(74, 18), new(58, 16), new(42, 16), new(26, 16));
            List<Vector2> whipPoints = [];
            Projectile.FillWhipControlPoints(Projectile, whipPoints);
            Main.instance.LoadProjectile(Type);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 pos = whipPoints[0];

            for (int i = 0; i < whipPoints.Count - 1; i++)
            {
                var frame = new Rectangle(0, 0, 10, 26);
                var origin = new Vector2(5, 8);
                float scale = 1;

                if (i == whipPoints.Count - 2)
                {
                    frame.Y = 74;
                    frame.Height = 18;

                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 10)
                {
                    frame.Y = 58;
                    frame.Height = 16;
                }
                else if (i > 5)
                {
                    frame.Y = 42;
                    frame.Height = 16;
                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Height = 16;
                }

                Vector2 element = whipPoints[i];
                Vector2 diff = whipPoints[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
}