using PoF.Content.Items.Accessories;
using PoF.Content.Items.Talismans;
using PoF.Content.Items.Whips;
using Terraria.GameContent.ItemDropRules;

namespace PoF.Common.Globals.ItemGlobals;

internal class BagGlobal : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type == ItemID.PlanteraBossBag)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DaisyChain>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Catgrass>()));
        }
        else if (item.type == ItemID.QueenBeeBossBag)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TheStinger>()));
        else if (item.type == ItemID.GolemBossBag)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AncientSnare>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SerpentCharm>()));
        }
        else if (item.type == ItemID.FairyQueenBossBag)
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RingOnAString>()));
    }
}
