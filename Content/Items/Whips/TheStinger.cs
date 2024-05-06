using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.Items.Whips;

public class TheStinger : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<TheStingerProj>(), 24, 2, 4, 25);
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.buyPrice(0, 0, 50);
    }

    public override bool MeleePrefix() => true;

    public class TheStingerProj : ModProjectile
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
            Projectile.WhipSettings.Segments = 16;
            Projectile.WhipSettings.RangeMultiplier = 1.2f;
        }

        public override bool PreAI()
        {
            List<Vector2> whipPoints = [];
            Projectile.FillWhipControlPoints(Projectile, whipPoints);

            foreach (var item in whipPoints)
            {
                if (Main.rand.NextBool(82))
                    Dust.NewDust(item, 8, 8, DustID.Honey2, 0, 0, 150, Color.Lerp(Color.White, Color.Orange, Main.rand.NextFloat(0, 0.75f)), 1f);
            }

            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            if (target.life <= 0)
            {
                int damage = (int)(Projectile.damage * 0.6f);

                for (int i = 0; i < 3; ++i)
                    Projectile.NewProjectile(target.GetSource_OnHurt(Projectile), target.Center, Vector2.Zero, ProjectileID.Bee, damage, 0);
            }

            Projectile.damage = (int)(Projectile.damage * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor) => WhipCommon.Draw(Projectile, Timer, new(0, 0, 10, 26), new(74, 18), new(58, 16), new(42, 16), new(26, 16));
    }
}