using Steamworks;
using System.Collections.Generic;
using System.Linq;

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

    public static bool GetNearestNPCTarget(this Entity entity, out NPC npc, float distance = 500)
    {
        HashSet<int> npcs = [];

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC cur = Main.npc[i];

            if (cur.CanBeChasedBy() && cur.DistanceSQ(entity.Center) < distance * distance)
                npcs.Add(i);
        }

        npc = null;

        if (npcs.Count > 0)
            npc = Main.npc[Main.rand.Next(npcs.ToArray())];

        return npc != null;
    }

    public static int ToActualDamage(float damageValue, float expertScaling = 1, float masterScaling = 1)
    {
        if (Main.masterMode)
            damageValue = damageValue / 6 * masterScaling;
        else if (Main.expertMode)
            damageValue = damageValue / 4 * expertScaling;
        else
            damageValue /= 2;

        return (int)damageValue;
    }
}
