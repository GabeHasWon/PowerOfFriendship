using PoF.Common.Players;
using ReLogic.Content;
using System;
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
        Item.noMelee = true;

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
    internal static float GetRange<T>() where T : Talisman => (ContentSamples.ItemsByType[ModContent.ItemType<T>()].ModItem as T).RealRange;
    internal static float GetRangeSq<T>() where T : Talisman => (ContentSamples.ItemsByType[ModContent.ItemType<T>()].ModItem as T).SquaredRange;

    /// <summary>
    /// Gets the real range for the given talisman, including range buffs.
    /// </summary>
    /// <typeparam name="T">Talisman to be referencing.</typeparam>
    /// <param name="player">Player to reference.</param>
    /// <returns>The range, in pixels, that the given talisman can reach.</returns>
    public static float GetRange<T>(Player player) where T : Talisman => GetRange<T>() * player.GetModPlayer<TalismanPlayer>().rangeMultiplier;

    /// <summary>
    /// Gets the real squared range for the given talisman, including range buffs.
    /// </summary>
    /// <typeparam name="T">Talisman to be referencing.</typeparam>
    /// <param name="player">Player to reference.</param>
    /// <returns>The squared range, in pixels, that the given talisman can reach.</returns>
    public static float GetRangeSq<T>(Player player) where T : Talisman => GetRangeSq<T>() * MathF.Pow(player.GetModPlayer<TalismanPlayer>().rangeMultiplier, 2);

    public static bool PayMana(Projectile proj)
    {
        Player plr = proj.Owner();
        bool paidMana = plr.CheckMana(plr.HeldItem.mana, true);
        plr.manaRegenDelay = (int)plr.maxRegenDelay;

        if (Main.myPlayer == proj.owner)
            NetMessage.SendData(MessageID.PlayerMana, -1, -1, null, Main.myPlayer);
        return paidMana;
    }

    /// <summary>
    /// Handles all basic functionality of a given Talisman projectile, like net syncing, paying mana, respecting range, and more.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="projectile"></param>
    /// <param name="time"></param>
    /// <param name="returnVelocity"></param>
    /// <param name="autoPayMana"></param>
    /// <returns>Whether the projectile should start despawning.</returns>
    public static bool HandleBasicFunctions<T>(Projectile projectile, ref float time, float? returnVelocity, bool autoPayMana = true) where T : Talisman
    {
        bool paidMana = true;

        projectile.netUpdate = true;
        projectile.Owner().SetDummyItemTime(2);
        projectile.timeLeft++;

        if (returnVelocity.HasValue && projectile.DistanceSQ(projectile.Owner().Center) > GetRangeSq<T>(projectile.Owner()))
            projectile.velocity += projectile.DirectionTo(projectile.Owner().Center) * 1.2f;

        if (time++ > projectile.Owner().HeldItem.useTime && autoPayMana)
        {
            paidMana = PayMana(projectile);
            time = 0;
        }

        if (Main.myPlayer == projectile.owner && !projectile.Owner().channel)
            return true;

        if (!paidMana)
        {
            projectile.Owner().channel = false;
            return true;
        }

        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tips)
    {
        int index = tips.FindIndex(x => x.Name == "UseMana");
        tips.Insert(index, new TooltipLine(Mod, "TalismanRange", Language.GetTextValue("Mods.PoF.TileRangeTooltip", TileRange.ToString("#0.##"))));
    }
}
