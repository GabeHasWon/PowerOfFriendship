namespace PoF.Content.Items.Accessories;

public class CrystalSkull : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(30, 36);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 5);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.07f;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Diamond, 12)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}