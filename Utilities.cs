namespace PoF;

internal static class Utilities
{
    public static Player Owner(this Projectile projectile) => Main.player[projectile.owner];

    public static Vector2 DirectionTo(this Entity entity, Entity other) => entity.DirectionTo(other.Center);
    public static Vector2 SafeNormalize(ref this Vector2 vector) => vector = (vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector));

    public static bool CanHitLine(Entity entity, Entity other) => Collision.CanHitLine(entity.position, entity.width, entity.height, other.position, other.width, other.height);
}
