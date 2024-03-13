namespace PoF.Content.Items.Armor.Comfy;

[AutoloadEquip(EquipType.Legs)]
public class ComfyPants : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = Item.buyPrice(0, 0, 15);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 2;
    }

    public override void UpdateEquip(Player player) => player.statManaMax2 += 20;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Silk, 9)
            .AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}