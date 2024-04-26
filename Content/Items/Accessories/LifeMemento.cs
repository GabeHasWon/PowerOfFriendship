using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;
using ReLogic.Content;

namespace PoF.Content.Items.Accessories;

public class LifeMemento : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(36, 50);
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 2);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<LifeMementoPlayer>().equipped = true;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.LifeFruit, 1)
        .AddIngredient(ItemID.ChlorophyteBar, 8)
        .AddTile(TileID.MythrilAnvil)
        .Register();

    public class LifeMementoPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    public class LifeMementoProjectile : GlobalProjectile
    {
        static Asset<Texture2D> _aura;

        public override bool InstancePerEntity => true;

        private int _healTime = 0;
        
        public override void SetStaticDefaults() => _aura = ModContent.Request<Texture2D>("PoF/Content/Items/Accessories/LifeMemento_Circle");
        public override void Unload() => _aura = null;

        public override bool AppliesToEntity(Projectile proj, bool lateInstantiation)
            => proj.DamageType.CountsAsClass<TalismanDamageClass>() && !TalismanGlobal.IsMinorTalismanProjectile.Contains(proj.type);

        public override bool PreAI(Projectile projectile)
        {
            if (Main.player[projectile.owner].GetModPlayer<LifeMementoPlayer>().equipped && _healTime++ > Main.player[projectile.owner].HeldItem.useTime * 2)
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
                if (item.DistanceSQ(projectile.Center) < 160 * 160 && item.statLife < item.statLifeMax2)
                    item.Heal((int)(projectile.damage * 0.2f) + 1);
            }
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (Main.player[projectile.owner].GetModPlayer<LifeMementoPlayer>().equipped)
            {
                var pos = projectile.Center - Main.screenPosition;
                var color = lightColor * 0.4f * projectile.Opacity;
                Main.spriteBatch.Draw(_aura.Value, pos, null, color, Main.GameUpdateCount * -0.03f, _aura.Value.Size() / 2f, 1.8f, SpriteEffects.None, 0);
            }
        }
    }
}
