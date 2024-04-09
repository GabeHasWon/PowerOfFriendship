using PoF.Content.Items.Talismans;
using System;

namespace PoF.Common.Players;

public class GuardianPlayer : ModPlayer
{
    internal bool hasSet = false;
    internal bool hellMask = false;

    public override void ResetEffects() => hasSet = hellMask = false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!hasSet)
            return;

        if (target.life > 0)
            return;

        if (!target.CanBeChasedBy())
            return;

        int damage = (int)Player.GetDamage(DamageClass.Summon).ApplyTo(hellMask ? 32 : 26);
        var baseVel = new Vector2(0, Main.rand.NextFloat(3, 7)).RotatedByRandom(MathHelper.TwoPi);
        int proj = Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, baseVel, ModContent.ProjectileType<GhostSkull>(), damage, 3f, Player.whoAmI);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
    }

    class GhostSkull : ModProjectile
    {
        public override string Texture => "PoF/Assets/Textures/GhostSkull";

        private ref float Time => ref Projectile.ai[0];
        private ref float Target => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(34);
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (Time == 0)
                Target = -1;

            if (Projectile.timeLeft < 30)
                Projectile.Opacity = Projectile.timeLeft / 30f;

            bool hellMask = Main.player[Projectile.owner].GetModPlayer<GuardianPlayer>().hellMask;

            Time++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, hellMask && Main.rand.NextBool(3) ? DustID.Torch : DustID.SpectreStaff);

            NPC npc = null;

            if (Target < 0 && !Projectile.GetNearestNPCTarget(out npc))
                return;

            if (npc is not null)
                Target = npc.whoAmI;

            if (!Main.npc[(int)Target].active)
            {
                Target = -1;
                return;
            }

            var targetVel = Projectile.DirectionTo(Main.npc[(int)Target].Center) * MathHelper.Min(Time * 0.15f, hellMask ? 14 : 10f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, hellMask ? 0.08f : 0.05f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White with { A = 0 };

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.player[Projectile.owner].GetModPlayer<GuardianPlayer>().hellMask)
            {
                var tex = ModContent.Request<Texture2D>(Texture + "_Flame").Value;
                var src = tex.Frame(1, 2, 0, (int)(Main.GameUpdateCount / 2) % 2, 0, 0);
                var col = (GetAlpha(default) ?? Color.White with { A = 0 }) * (MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.2f + 0.4f);
                var origin = Projectile.Size / 2f + new Vector2(0, 4);

                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, col * Projectile.Opacity, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
