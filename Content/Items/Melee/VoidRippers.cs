﻿namespace PoF.Content.Items.Melee;

class VoidRippers : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.FetidBaghnakhs);
        Item.damage = 98;
        Item.useTime = Item.useAnimation = 6;
    }
}
