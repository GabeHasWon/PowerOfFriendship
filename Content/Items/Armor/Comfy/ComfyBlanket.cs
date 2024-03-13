namespace PoF.Content.Items.Armor.Comfy;

[AutoloadEquip(EquipType.Body)]
public class ComfyBlanket : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 18;
        Item.value = Item.buyPrice(0, 0, 20);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 3;
    }

    public override void UpdateEquip(Player player) => player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.05f;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Silk, 15)
            .AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}