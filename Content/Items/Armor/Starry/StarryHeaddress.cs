using PoF.Content.Items.Armor.Comfy;
using Terraria.Localization;

namespace PoF.Content.Items.Armor.Starry;

[AutoloadEquip(EquipType.Head)]
public class StarryHeaddress : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 24;
        Item.value = Item.buyPrice(0, 0, 80);
        Item.rare = ItemRarityID.Green;
        Item.defense = 4;
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Summon) += 0.1f;
    public override bool IsArmorSet(Item h, Item b, Item l) => b.type == ModContent.ItemType<StarryBreastplate>() && l.type == ModContent.ItemType<StarryLeggings>();

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = Language.GetTextValue("Mods.PoF.SetBonuses.Starry");
        player.maxMinions++;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 2)
            .AddIngredient(ItemID.Feather, 4)
            .AddIngredient(ItemID.ShadowScale, 5)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.FallenStar, 2)
            .AddIngredient(ItemID.Feather, 4)
            .AddIngredient(ItemID.TissueSample, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}