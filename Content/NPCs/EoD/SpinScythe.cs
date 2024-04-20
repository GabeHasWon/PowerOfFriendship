using Terraria.GameContent;

namespace PoF.Content.NPCs.EoD;

public class SpinScythe : ModProjectile
{
    private NPC Owner => Main.npc[(int)OwnerWho];
    private ref float OwnerWho => ref Projectile.ai[0];
    private ref float Timer => ref Projectile.ai[1];
    private ref float RotationOffset => ref Projectile.ai[2];

    private float dist = 0;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(66);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 10;
        Projectile.aiStyle = -1;
        Projectile.Opacity = 0f;
    }

    public override void AI()
    {
        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.02f);
        Projectile.rotation += 0.2f;
        Projectile.timeLeft++;

        if (!Owner.active)
        {
            Projectile.Kill();
            return;
        }

        dist += 2f;
        Timer++;

        if (dist > 100)
            dist = 100;

        Projectile.Center = Owner.Center + new Vector2(0, dist).RotatedBy(Timer * 0.02f + RotationOffset);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

        for (int k = 0; k < Projectile.oldPos.Length; k++)
        {
            if (k % 2 == 0)
                continue;

            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var baseCol = Color.Lerp(lightColor, Color.Black, k / (float)(Projectile.oldPos.Length - 1));
            Color color = Projectile.GetAlpha(baseCol) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * Projectile.Opacity;
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
