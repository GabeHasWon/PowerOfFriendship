namespace PoF.Content.Items.Accessories;

[AutoloadEquip(EquipType.HandsOn)]
public class StingerNecklace : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(30, 36);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(copper: 80);
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.Stinger, 2)
        .AddIngredient(ItemID.JungleSpores, 8)
        .AddIngredient(ItemID.Cobweb, 10)
        .AddTile(TileID.Loom)
        .Register();

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<StingerPlayer>().equipped = true;

    class StingerPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (ProjectileID.Sets.IsAWhip[proj.type] && equipped)
            {
                modifiers.FinalDamage += 0.1f;

                if (target.HasBuff(BuffID.Poisoned))
                    modifiers.FinalDamage += 0.1f;
            }
        }
    }
}