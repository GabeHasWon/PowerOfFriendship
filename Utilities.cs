namespace PoF;

internal static class Utilities
{
    public static Player Owner(this Projectile projectile) => Main.player[projectile.owner];

    public static Vector2 DirectionTo(this Entity entity, Entity other) => entity.DirectionTo(other.Center);
    public static Vector2 SafeNormalize(ref this Vector2 vector) => vector = (vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector));
    public static Vector2 SafeDirectionTo(this Entity entity, Vector2 worldPosition) => Utils.SafeNormalize(worldPosition - entity.Center, Vector2.Zero);
    public static Vector2 SafeDirectionTo(this Entity entity, Entity other) => Utils.SafeNormalize(other.Center - entity.Center, Vector2.Zero);

    public static bool CanHitLine(Entity entity, Entity other) => Collision.CanHitLine(entity.position, entity.width, entity.height, other.position, other.width, other.height);

    public static int GetBobble(this Player player)
    {
        int playerFrame = player.bodyFrame.Y / player.bodyFrame.Height;
        bool lowFrame = (playerFrame >= 7 && playerFrame <= 9) || (playerFrame >= 14 && playerFrame <= 16);
        return !lowFrame ? 0 : -2;
    }
}
