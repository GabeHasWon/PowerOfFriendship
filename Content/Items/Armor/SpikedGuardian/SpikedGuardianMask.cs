using PoF.Common.Players;
using PoF.Content.Items.Talismans;
using Terraria.Localization;

namespace PoF.Content.Items.Armor.SpikedGuardian;

[AutoloadEquip(EquipType.Head)]
public class SpikedGuardianMask : ModItem
{
    protected virtual int BrickType => ItemID.BlueBrick;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 26;
        Item.value = Item.buyPrice(0, 2);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 5;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.ModItem is SpikedGuardianRobe;

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Summon) += 0.08f;
        player.GetDamage(DamageClass.SummonMeleeSpeed) -= 0.08f; // Offset above bonus
        player.GetDamage<TalismanDamageClass>() -= 0.08f;
    }

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = Language.GetTextValue("Mods.PoF.SetBonuses.Dungeon");
        player.GetModPlayer<GuardianPlayer>().hasSet = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 16)
            .AddIngredient(BrickType, 10)
            .AddIngredient(ItemID.Spike, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
class SpikedGuardianMaskPink : SpikedGuardianMask
{
    protected override int BrickType => ItemID.PinkBrick;
    public override void UpdateEquip(Player player) => player.GetDamage<TalismanDamageClass>() += 0.08f;
}

[AutoloadEquip(EquipType.Head)]
class SpikedGuardianMaskGreen : SpikedGuardianMask
{
    protected override int BrickType => ItemID.GreenBrick;
    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.08f;
}