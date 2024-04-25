using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;
using ReLogic.Content;

namespace PoF.Content.Items.Accessories;

public class HeartMemento : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(32, 42);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<MementoPlayer>().equipped = true;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.GoldBar, 5)
        .AddIngredient(ItemID.LifeCrystal, 1)
        .AddTile(TileID.Anvils)
        .Register();

    public class MementoPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    public class MementoProjectile : GlobalProjectile
    {
        static Asset<Texture2D> _aura;

        public override bool InstancePerEntity => true;

        private int _healTime = 0;
        
        public override void SetStaticDefaults() => _aura = ModContent.Request<Texture2D>("PoF/Content/Items/Accessories/HeartMemento_Circle");
        public override void Unload() => _aura = null;

        public override bool AppliesToEntity(Projectile proj, bool lateInstantiation)
            => proj.DamageType.CountsAsClass<TalismanDamageClass>() && !TalismanGlobal.IsMinorTalismanProjectile.Contains(proj.type);

        public override bool PreAI(Projectile projectile)
        {
            if (Main.player[projectile.owner].GetModPlayer<MementoPlayer>().equipped && _healTime++ > 60f)
            {
                DoHealAura(projectile);
                _healTime = 0;
            }

            return true;
        }

        private static void DoHealAura(Projectile projectile)
        {
            foreach (var item in Main.ActivePlayers)
            {
                if (item.DistanceSQ(projectile.Center) < 120 * 120)
                    item.Heal((int)(projectile.damage * 0.1f));
            }
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (Main.player[projectile.owner].GetModPlayer<MementoPlayer>().equipped)
            {
                var pos = projectile.Center - Main.screenPosition;
                var color = lightColor * 0.4f * projectile.Opacity;
                Main.spriteBatch.Draw(_aura.Value, pos, null, color, Main.GameUpdateCount * -0.03f, _aura.Value.Size() / 2f, 1.8f, SpriteEffects.None, 0);
            }
        }
    }
}

public class HeartMementoPlat : HeartMemento
{
    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.PlatinumBar, 5)
        .AddIngredient(ItemID.LifeCrystal, 1)
        .AddTile(TileID.Anvils)
        .Register();
}