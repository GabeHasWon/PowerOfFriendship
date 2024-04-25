namespace PoF.Content.Items.Accessories;

public class DemoniteHandle : ModItem
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
        player.GetModPlayer<DemoniteWhipPlayer>().equipped = true;
        player.GetCritChance(DamageClass.SummonMeleeSpeed) += 0.15f;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.DemoniteBar, 4)
        .AddIngredient(ItemID.ShadowScale, 2)
        .AddIngredient(ItemID.RottenChunk, 2)
        .AddTile(TileID.Anvils)
        .Register();

    public class DemoniteWhipPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }
}