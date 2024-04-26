using PoF.Content.Items.Accessories;

namespace PoF.Common.Globals.ItemGlobals;

public class ExtendedWhipProjectile : GlobalProjectile
{
    public override void Load() => On_Projectile.GetWhipSettings += ModifyRange;

    private void ModifyRange(On_Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier)
    {
        orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);
        Player player = Main.player[proj.owner];

        if (player.GetModPlayer<ExtensionCord.ExtendedPlayer>().equipped)
            rangeMultiplier *= 1.2f;

        if (player.GetModPlayer<RetractionCord.UnextendedPlayer>().equipped)
            rangeMultiplier *= 0.4f;

        if (player.GetModPlayer<DemoniteHandle.DemoniteWhipPlayer>().equipped)
            rangeMultiplier *= 0.85f;

        if (player.GetModPlayer<CrimtaneHandle.CrimtaneWhipPlayer>().equipped)
            rangeMultiplier *= 0.85f;
    }
}
