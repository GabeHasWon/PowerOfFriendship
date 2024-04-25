
namespace PoF.Content.Items.Accessories;

public class RetractionCord : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(30, 34);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 20);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<UnextendedPlayer>().equipped = true;
        player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.4f;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.Wire, 4)
        .AddRecipeGroup(RecipeGroupID.IronBar)
        .AddTile(TileID.TinkerersWorkbench)
        .Register();

    public class UnextendedPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }
}