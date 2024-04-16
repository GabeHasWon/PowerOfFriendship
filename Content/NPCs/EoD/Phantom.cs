using System;
using Terraria;
using Terraria.GameContent;

namespace PoF.Content.NPCs.EoD;

public class Phantom : ModProjectile
{
    private const int MaxTimeLeft = 60 * 5;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(28, 30);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.aiStyle = -1;
        Projectile.Opacity = 0f;
    }

    public override void AI()
    {
        if (Projectile.timeLeft > 30)
        {
            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);

            Player nearest = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];

            if (!nearest.active || nearest.dead)
                return;

            float speed = 1 - Projectile.timeLeft / (float)MaxTimeLeft;
            Projectile.velocity += Projectile.DirectionTo(nearest.Center) * (0.5f + 3 * speed);

            if (Projectile.velocity.LengthSquared() > MathF.Pow(6f + 6f * speed, 2))
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * (6f + 6f * speed);
        }
        else
            Projectile.velocity *= 0.9f;
    }

    public override Color? GetAlpha(Color lightColor) => lightColor with { A = 0 };

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

        for (int k = 0; k < Projectile.oldPos.Length; k++)
        {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var baseCol = Color.Lerp(lightColor, Color.Black, k / (float)(Projectile.oldPos.Length - 1));
            Color color = Projectile.GetAlpha(baseCol) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * Projectile.Opacity;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
