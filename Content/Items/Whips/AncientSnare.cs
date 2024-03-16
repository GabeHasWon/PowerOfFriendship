using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Whips;

public class AncientSnare : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<AncientSnareProj>(), 120, 6, 4, 40);
        Item.rare = ItemRarityID.LightPurple;
        Item.value = Item.buyPrice(0, 3);
    }

    public override bool MeleePrefix() => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            velocity *= Main.rand.NextFloat(1.8f, 2.4f);
            velocity = velocity.RotatedByRandom(0.1f);
            int proj = Projectile.NewProjectile(source, position, velocity, ProjectileID.SpikyBallTrap, damage / 3 * 2, 0.3f, player.whoAmI);
            Main.projectile[proj].hostile = false;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].timeLeft = 180;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
        }
        
        return true;
    }

    public class AncientSnareProj : ModProjectile
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
            Projectile.WhipSettings.Segments = 38;
            Projectile.WhipSettings.RangeMultiplier = 2.2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            Projectile.damage = (int)(Projectile.damage * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> whipPoints = [];
            Projectile.FillWhipControlPoints(Projectile, whipPoints);
            Main.instance.LoadProjectile(Type);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 pos = whipPoints[0];

            for (int i = 0; i < whipPoints.Count - 1; i++)
            {
                var frame = new Rectangle(0, 0, 14, 26);
                var origin = frame.Size() / 2f;
                float scale = 1;

                if (i == whipPoints.Count - 2)
                {
                    frame.Y = 80;
                    frame.Height = 18;

                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 26)
                {
                    frame.Y = 62;
                    frame.Height = 18;
                }
                else if (i > 13)
                {
                    frame.Y = 44;
                    frame.Height = 18;
                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Height = 18;
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