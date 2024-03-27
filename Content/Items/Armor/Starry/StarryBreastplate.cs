using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Armor.Starry;

[AutoloadEquip(EquipType.Body)]
public class StarryBreastplate : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 18;
        Item.value = Item.buyPrice(0, 1, 20);
        Item.rare = ItemRarityID.Green;
        Item.defense = 5;
    }

    public override void UpdateEquip(Player player)
    {
        player.jumpSpeedBoost += 2f;
        player.GetDamage<TalismanDamageClass>() += 0.05f;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Feather, 5)
            .AddIngredient(ItemID.ShadowScale, 8)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Feather, 5)
            .AddIngredient(ItemID.TissueSample, 8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}