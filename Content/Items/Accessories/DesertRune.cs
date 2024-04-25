using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Accessories;

public class DesertRune : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(24, 30);
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1, silver: 20);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.AncientBattleArmorMaterial)
            .AddIngredient(ItemID.AdamantiteBar, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.AncientBattleArmorMaterial)
            .AddIngredient(ItemID.TitaniumBar, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetDamage<TalismanDamageClass>() += 0.15f;
}