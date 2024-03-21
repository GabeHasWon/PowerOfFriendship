using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Whips;

internal static class WhipCommon
{
    public static bool Draw(Projectile proj, float timer, Rectangle source, Point tipInfo, Point firstInfo, Point secondInfo, Point thirdInfo, float opacity = 1f)
    {
        List<Vector2> whipPoints = [];
        Projectile.FillWhipControlPoints(proj, whipPoints);
        Main.instance.LoadProjectile(proj.type);

        SpriteEffects flip = proj.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Texture2D texture = TextureAssets.Projectile[proj.type].Value;
        Vector2 pos = whipPoints[0];

        for (int i = 0; i < whipPoints.Count - 1; i++)
        {
            var frame = source;
            var origin = frame.Size() / 2f;
            float scale = 1;

            if (i == whipPoints.Count - 2)
            {
                frame.Y = tipInfo.X;
                frame.Height = tipInfo.Y;

                Projectile.GetWhipSettings(proj, out float timeToFlyOut, out int _, out float _);
                float t = timer / timeToFlyOut;
                scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
            }
            else if (i > 26)
            {
                frame.Y = firstInfo.X;
                frame.Height = firstInfo.Y;
            }
            else if (i > 13)
            {
                frame.Y = secondInfo.X;
                frame.Height = secondInfo.Y;
            }
            else if (i > 0)
            {
                frame.Y = thirdInfo.X;
                frame.Height = thirdInfo.Y;
            }

            Vector2 element = whipPoints[i];
            Vector2 diff = whipPoints[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color * opacity, rotation, origin, scale, flip, 0);

            pos += diff;
        }

        return false;
    }
}
