using PoF.Content.Items.Accessories;
using Terraria.GameContent.ItemDropRules;

namespace PoF.Common.Globals.ProjectileGlobals;

internal class LootGlobal : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.Plantera)
        {
            var rule = new LeadingConditionRule(new Conditions.NotExpert());
            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DaisyChain>()));
            npcLoot.Add(rule);
        }
    }
}
