using PoF.Common.Players;
using PoF.Content.Items.Talismans;
using Terraria.Localization;

namespace PoF.Content.Items.Armor.SpikedGuardian;

[AutoloadEquip(EquipType.Head)]
public class Hellmask : ModItem
{
    protected virtual int BrickType => ItemID.BlueBrick;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 26;
        Item.value = Item.buyPrice(0, 2);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 7;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.ModItem is SpikedGuardianRobe;

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Summon) += 0.12f;

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = Language.GetTextValue("Mods.PoF.SetBonuses.Dungeon");
        player.GetModPlayer<GuardianPlayer>().hasSet = true;
        player.GetModPlayer<GuardianPlayer>().hellMask = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 8)
            .AddIngredient<SpikedGuardianMask>()
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 8)
            .AddIngredient<SpikedGuardianMaskGreen>()
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 8)
            .AddIngredient<SpikedGuardianMaskPink>()
            .AddTile(TileID.Anvils)
            .Register();
    }
}
