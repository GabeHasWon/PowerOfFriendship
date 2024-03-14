namespace PoF.Content.Items.Accessories;

public class MundaneSkull : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(24, 26);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(silver: 5);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetArmorPenetration(DamageClass.SummonMeleeSpeed) += 7;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 12)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}