using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Accessories;

public class DarkOrb : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(36);
        Item.rare = ItemRarityID.Expert;
        Item.value = Item.sellPrice(gold: 2);
        Item.expert = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<OrbPlayer>().equipped = true;

    class OrbPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    class OrbProjectile : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.DamageType.CountsAsClass<TalismanDamageClass>() && projectile.friendly && projectile.TryGetOwner(out var owner) && owner.GetModPlayer<OrbPlayer>().equipped
                && !TalismanGlobal.IsMinorTalismanProjectile.Contains(projectile.type))
            {
                int type = ModContent.ProjectileType<DarkDagger>();

                for (int i = 0; i < 3; ++i)
                    Projectile.NewProjectile(source, projectile.Center, Vector2.Zero, type, projectile.damage / 2, 1f, projectile.owner, projectile.whoAmI, i / 3f * MathHelper.TwoPi);
            }
        }
    }

    public class DarkDagger : ModProjectile
    {
        private Projectile Parent => Main.projectile[(int)Projectile.ai[0]];
        private ref float BaseRot => ref Projectile.ai[1];
        private ref float Timer => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(20);
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.timeLeft++;

            if (Projectile.ai[0] == -1 || !Parent.active)
            {
                Projectile.ai[0] = -1;
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 0.1f);

                if (Projectile.Opacity < 0.036f)
                    Projectile.Kill();

                return;
            }

            Timer++;
            Projectile.Center = Parent.Center + (Vector2.UnitY * 120).RotatedBy(BaseRot + Timer * 0.04f);
            Projectile.rotation = BaseRot + Timer * 0.04f - MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor) => lightColor with { A = 0 };
    }
}