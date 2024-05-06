using PoF.Common.Players;

namespace PoF.Content.Items.Accessories;

public class ShroomiteScope : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(32, 16);
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!hideVisual)
            player.scope = true;

        player.GetModPlayer<TalismanPlayer>().rangeMultiplier *= 1.40f;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.ShroomiteBar, 3)
        .AddIngredient(ItemID.Lens, 2)
        .AddTile(TileID.Autohammer)
        .Register();
}