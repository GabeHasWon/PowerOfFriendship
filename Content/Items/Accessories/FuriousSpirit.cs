using PoF.Content.Buffs;

namespace PoF.Content.Items.Accessories;

public class FuriousSpirit : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(24, 44);
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 4);
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.Ectoplasm, 15)
        .AddIngredient(ItemID.HellstoneBar, 10)
        .AddTile(TileID.MythrilAnvil)
        .Register();

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<SpiritPlayer>().equipped = true;

    class SpiritPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (equipped && target.life <= 0)
                Player.AddBuff(ModContent.BuffType<FuryBuff>(), FuryBuff.Cap);
        }
    }
}