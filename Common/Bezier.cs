using ReLogic.Content;

namespace PoF.Common;

public static class Bezier
{
    public static void DrawBezier(Asset<Texture2D> tex, Rectangle src, float interations, Vector2 start, Vector2 middle, Vector2 end, Color color, out float endOfRopeRotation)
    {
        endOfRopeRotation = 0;
        Vector2 lastPos = start;

        for (int i = 0; i <= interations; ++i)
        {
            float factor = i / interations;
            var currentPos = Vector2.Lerp(Vector2.Lerp(start, middle, factor), Vector2.Lerp(middle, end, factor), factor);
            float rot = currentPos.AngleTo(lastPos) - MathHelper.PiOver2;
            var col = color;

            if (i < 5)
                col *= i / 5f;

            if (i == interations)
                endOfRopeRotation = rot;

            Main.spriteBatch.Draw(tex.Value, currentPos - Main.screenPosition, src, col, rot, src.Size() / 2f, 1f, SpriteEffects.None, 0);
            lastPos = currentPos;
        }
    }
}
