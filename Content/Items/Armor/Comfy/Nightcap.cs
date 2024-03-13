using Terraria.Localization;

namespace PoF.Content.Items.Armor.Comfy;

[AutoloadEquip(EquipType.Head)]
public class Nightcap : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 22;
        Item.value = Item.buyPrice(0, 0, 10);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<ComfyBlanket>() && legs.type == ModContent.ItemType<ComfyPants>();
    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.Summon).Flat += 1;

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = Language.GetTextValue("Mods.PoF.SetBonuses.Comfy");
        player.GetModPlayer<ComfyPlayer>().hasSet = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Silk, 6)
            .AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    class ComfyPlayer : ModPlayer
    {
        internal bool hasSet = false;

        public override void ResetEffects() => hasSet = false;

        public override void UpdateLifeRegen()
        {
            if (hasSet)
                Player.lifeRegen += 10;
        }
    }
}