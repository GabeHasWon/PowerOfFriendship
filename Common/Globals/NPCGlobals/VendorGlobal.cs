using PoF.Content.Items.Talismans;

namespace PoF.Common.Globals.ProjectileGlobals;

internal class VendorGlobal : GlobalNPC
{
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.PartyGirl)
            shop.Add(new NPCShop.Entry(ModContent.ItemType<PartyTrick>(), Condition.Hardmode));
    }
}
