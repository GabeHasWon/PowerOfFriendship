using System;
using Terraria.GameContent;

namespace PoF.Content.NPCs.EoD;

public class TelegraphSword : ModProjectile
{
    private ref float Timer => ref Projectile.ai[0];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(26);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60 * 10;
        Projectile.aiStyle = -1;
        Projectile.Opacity = 0f;
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        Timer++;

        Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.02f);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Main.rand.NextBool(30))
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Gold);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 adjVel = Vector2.Normalize(Projectile.velocity) * 29;
        float _ = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center + adjVel, Projectile.Center - adjVel, 12, ref _);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (Main.expertMode)
            target.AddBuff(BuffID.BrokenArmor, 120);
    }

    public override bool ShouldUpdatePosition() => Timer >= 90;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

        if (Timer < 90)
        {
            Texture2D spotLight = TextureAssets.Extra[ExtrasID.PortalGateHalo2].Value;
            var drawPos = Projectile.Center - Main.screenPosition;
            var origin = new Vector2(spotLight.Width / 2f, spotLight.Height / 2 + 12);
            float rot = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            var color = Color.Gold with { A = 0 } * 0.5f;

            if (Timer < 10)
                color *= Timer / 10f;

            if (Timer > 80)
                color *= 1 - (Timer - 80) / 10f;

            Main.EntitySpriteDraw(spotLight, drawPos, null, color, rot, origin, new Vector2(0.15f, 20f), SpriteEffects.None, 0);
        }

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
