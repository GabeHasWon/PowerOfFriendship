using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;
using ReLogic.Content;

namespace PoF.Content.Items.Accessories;

public class ThornBulb : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(32, 34);
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(silver: 5);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<ThornBulbPlayer>().equipped = true;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.JungleSpores, 6)
        .AddIngredient(ItemID.Stinger, 6)
        .AddTile(TileID.Anvils)
        .Register();

    public class ThornBulbPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    public class ThornBulbProjectile : GlobalProjectile
    {
        static Asset<Texture2D> _aura;

        public override void SetStaticDefaults() => _aura = ModContent.Request<Texture2D>("PoF/Content/Items/Accessories/ThornBulb_Circle");
        public override void Unload() => _aura = null;

        public override bool AppliesToEntity(Projectile proj, bool lateInstantiation)
            => proj.DamageType.CountsAsClass<TalismanDamageClass>() && !TalismanGlobal.IsMinorTalismanProjectile.Contains(proj.type);

        public override bool PreAI(Projectile projectile)
        {
            if (Main.player[projectile.owner].GetModPlayer<ThornBulbPlayer>().equipped)
                DoThornsAndDefenseAura(projectile);

            return true;
        }

        private static void DoThornsAndDefenseAura(Projectile projectile)
        {
            foreach (var item in Main.ActivePlayers)
            {
                if (item.DistanceSQ(projectile.Center) < 180 * 180)
                {
                    item.statDefense += 5;
                    item.AddBuff(BuffID.Thorns, 2);
                }
            }
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (Main.player[projectile.owner].GetModPlayer<ThornBulbPlayer>().equipped)
            {
                var pos = projectile.Center - Main.screenPosition;
                var color = lightColor * 0.5f * projectile.Opacity;
                Main.spriteBatch.Draw(_aura.Value, pos, null, color, Main.GameUpdateCount * 0.05f, _aura.Value.Size() / 2f, 1.5f, SpriteEffects.None, 0);
            }
        }
    }
}