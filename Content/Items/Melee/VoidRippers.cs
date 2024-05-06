namespace PoF.Content.Items.Melee;

class VoidRippers : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.FetidBaghnakhs);
        Item.damage = 134;
        Item.useTime = Item.useAnimation = 6;
        Item.rare = ItemRarityID.Yellow;
    }
}
