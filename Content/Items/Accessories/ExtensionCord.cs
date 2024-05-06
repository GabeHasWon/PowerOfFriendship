
namespace PoF.Content.Items.Accessories;

public class ExtensionCord : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(30, 34);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(silver: 20);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<ExtendedPlayer>().equipped = true;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.Wire, 15)
        .AddRecipeGroup(RecipeGroupID.IronBar)
        .AddTile(TileID.TinkerersWorkbench)
        .Register();

    public class ExtendedPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }
}