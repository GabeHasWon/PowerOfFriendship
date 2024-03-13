using PoF.Common.Players;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Localization;

namespace PoF.Content.Items.Talismans;

internal abstract class Talisman : ModItem
{
    public static Dictionary<int, Asset<Texture2D>> HeldTextures = [];

    protected abstract float TileRange { get; }

    public virtual string HeldTexturePath => Texture + "_Held";

    public Asset<Texture2D> HeldTexture => HeldTextures[Type];

    protected float RealRange => TileRange * 16;
    protected float SquaredRange => RealRange * RealRange;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        HeldTextures.Add(Type, ModContent.Request<Texture2D>(HeldTexturePath));

        ItemID.Sets.StaffMinionSlotsRequired[Type] = 0;
    }

    public sealed override void SetDefaults()
    {
        Item.DamageType = TalismanDamageClass.Self;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
        Item.channel = true;
        Item.autoReuse = false;
        Item.noUseGraphic = true;

        Defaults();
    }

    protected virtual void Defaults()
    {
    }

    public override void HoldItem(Player player)
    {
        if (player.channel)
        {
            player.GetModPlayer<TalismanPlayer>().heldTalisman = this;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.PiOver2 * -player.direction);
        }
    }

    public static Item GetTalisman<T>() where T : Talisman => ContentSamples.ItemsByType[ModContent.ItemType<T>()];
    public static float GetRange<T>() where T : Talisman => (ContentSamples.ItemsByType[ModContent.ItemType<T>()].ModItem as T).RealRange;
    public static float GetRangeSq<T>() where T : Talisman => (ContentSamples.ItemsByType[ModContent.ItemType<T>()].ModItem as T).SquaredRange;

    public static bool PayMana(Projectile proj)
    {
        Player plr = proj.Owner();
        bool paidMana = plr.CheckMana(plr.HeldItem.mana, true);
        plr.manaRegenDelay = (int)plr.maxRegenDelay;
        return paidMana;
    }

    public override void ModifyTooltips(List<TooltipLine> tips)
    {
        int index = tips.FindIndex(x => x.Name == "UseMana");
        tips.Insert(index, new TooltipLine(Mod, "TalismanRange", Language.GetTextValue("Mods.PoF.TileRangeTooltip", TileRange.ToString("#0.##"))));
    }
}
