namespace PoF.Content.Items.Armor.Starry;

[AutoloadEquip(EquipType.Legs)]
public class StarryLeggings : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = Item.buyPrice(0, 1, 0, 0);
        Item.rare = ItemRarityID.Green;
        Item.defense = 4;
    }

    public override void UpdateEquip(Player player) => player.GetModPlayer<StarryMovementPlayer>().hasStarry = true;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 3)
            .AddIngredient(ItemID.Feather, 6)
            .AddIngredient(ItemID.ShadowScale, 6)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 3)
            .AddIngredient(ItemID.Feather, 6)
            .AddIngredient(ItemID.TissueSample, 6)
            .Register();
    }

    private class StarryMovementPlayer : ModPlayer
    {
        internal bool hasStarry = false;

        public override void ResetEffects() => hasStarry = false;

        public override void PostUpdateRunSpeeds()
        {
            if (hasStarry)
                Player.maxRunSpeed *= 1.2f;
        }
    }
}