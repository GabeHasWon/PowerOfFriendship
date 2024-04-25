using PoF.Content.Items.Accessories;

namespace PoF.Common.Globals.ItemGlobals;

public class ExtendedWhipProjectile : GlobalProjectile
{
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (!ProjectileID.Sets.IsAWhip[projectile.type])
            return;

        if (source is not EntitySource_ItemUse_WithAmmo itemUse)
            return;

        if (itemUse.Player.GetModPlayer<ExtensionCord.ExtendedPlayer>().equipped)
            projectile.WhipSettings.RangeMultiplier *= 1.2f;

        if (itemUse.Player.GetModPlayer<RetractionCord.UnextendedPlayer>().equipped)
            projectile.WhipSettings.RangeMultiplier *= 0.4f;

        if (itemUse.Player.GetModPlayer<DemoniteHandle.DemoniteWhipPlayer>().equipped)
            projectile.WhipSettings.RangeMultiplier *= 0.85f;

        if (itemUse.Player.GetModPlayer<CrimtaneHandle.CrimtaneWhipPlayer>().equipped)
            projectile.WhipSettings.RangeMultiplier *= 0.85f;
    }
}
