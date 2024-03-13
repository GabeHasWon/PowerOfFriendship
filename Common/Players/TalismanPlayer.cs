using PoF.Content.Items.Talismans;

namespace PoF.Common.Players;

internal class TalismanPlayer : ModPlayer
{
    public Talisman heldTalisman = null;

    public override void ResetEffects() => heldTalisman = null;
}

internal class TalismanLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        TalismanPlayer talismanPlr = player.GetModPlayer<TalismanPlayer>();
        var talisman = talismanPlr.heldTalisman;
        
        if (talisman is not null && !player.dead)
        {
            var tex = talisman.HeldTexture.Value;
            var pos = player.Center - Main.screenPosition + new Vector2(12 * player.direction, player.GetBobble() + player.gfxOffY);
            var dir = player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var data = new DrawData(tex, pos.Floor(), null, Lighting.GetColor(player.Center.ToTileCoordinates()), 0f, tex.Size() / 2f, 1f, dir, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
