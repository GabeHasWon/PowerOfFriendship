namespace PoF.Content.Items.Armor.SpikedGuardian;

[AutoloadEquip(EquipType.Body)]
public class Hellrobe : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 30;
        Item.value = Item.buyPrice(0, 2);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 9;
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Summon) += 0.14f;

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        if (type == EquipType.Body)
            player.legs = EquipLoader.GetEquipSlot(Mod, "HellrobeLegs", EquipType.Legs);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 14)
            .AddIngredient<SpikedGuardianRobe>()
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 14)
            .AddIngredient<SpikedGuardianRobeGreen>()
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 14)
            .AddIngredient<SpikedGuardianRobePink>()
            .AddTile(TileID.Anvils)
            .Register();
    }
}
