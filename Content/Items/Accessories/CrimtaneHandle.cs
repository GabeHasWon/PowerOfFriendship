namespace PoF.Content.Items.Accessories;

public class CrimtaneHandle : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(20);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 5);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<CrimtaneWhipPlayer>().equipped = true;
        player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.10f;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.CrimtaneBar, 4)
        .AddIngredient(ItemID.TissueSample, 2)
        .AddIngredient(ItemID.Vertebrae, 2)
        .AddTile(TileID.Anvils)
        .Register();

    public class CrimtaneWhipPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }
}