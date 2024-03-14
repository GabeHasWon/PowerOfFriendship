using PoF.Content.Items.Accessories;
using Terraria.GameContent.ItemDropRules;

namespace PoF.Common.Globals.ItemGlobals;

internal class BagGlobal : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type == ItemID.PlanteraBossBag)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DaisyChain>()));
    }
}
