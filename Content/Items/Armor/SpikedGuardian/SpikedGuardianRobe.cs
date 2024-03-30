using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Armor.SpikedGuardian;

[AutoloadEquip(EquipType.Body)]
public class SpikedGuardianRobe : ModItem
{
    protected virtual int BrickType => ItemID.BlueBrick;

    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 30;
        Item.value = Item.buyPrice(0, 2, 50);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 6;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Summon) += 0.1f;
        player.GetDamage(DamageClass.SummonMeleeSpeed) -= 0.1f; // Offset above bonus
        player.GetDamage<TalismanDamageClass>() -= 0.1f;
    }

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        if (type == EquipType.Body)
            player.legs = EquipLoader.GetEquipSlot(Mod, Name + "Legs", EquipType.Legs);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(BrickType, 16)
            .AddIngredient(ItemID.Spike, 4)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
class SpikedGuardianRobeGreen : SpikedGuardianMask
{
    protected override int BrickType => ItemID.GreenBrick;
    public override void UpdateEquip(Player player) => player.GetDamage(DamageClass.SummonMeleeSpeed) += 0.1f;

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        if (type == EquipType.Body)
            player.legs = Item.legSlot;
    }
}

[AutoloadEquip(EquipType.Body)]
class SpikedGuardianRobePink : SpikedGuardianMask
{
    protected override int BrickType => ItemID.PinkBrick;
    public override void UpdateEquip(Player player) => player.GetDamage<TalismanDamageClass>() += 0.1f;

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        if (type == EquipType.Body)
            player.legs = Item.legSlot;
    }
}