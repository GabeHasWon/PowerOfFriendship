using PoF.Content.Items.Accessories;
using PoF.Content.Items.Talismans;
using PoF.Content.Items.Whips;
using Terraria.GameContent.ItemDropRules;

namespace PoF.Common.Globals.ProjectileGlobals;

internal class LootGlobal : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.Plantera)
            AddNotExpertRules(npcLoot, ItemDropRule.Common(ModContent.ItemType<DaisyChain>()), ItemDropRule.Common(ModContent.ItemType<Catgrass>()));
        else if (npc.type == NPCID.QueenBee)
            AddNotExpertRules(npcLoot, ItemDropRule.Common(ModContent.ItemType<TheStinger>()));
        else if (npc.type == NPCID.Golem)
            AddNotExpertRules(npcLoot, ItemDropRule.Common(ModContent.ItemType<AncientSnare>()), ItemDropRule.Common(ModContent.ItemType<SerpentCharm>()));
        else if (npc.type == NPCID.HallowBoss)
            AddNotExpertRules(npcLoot, ItemDropRule.Common(ModContent.ItemType<RingOnAString>()));
        else if (npc.type == NPCID.DukeFishron)
            AddNotExpertRules(npcLoot, ItemDropRule.Common(ModContent.ItemType<DukeToothNecklace>()));
    }

    private static void AddNotExpertRules(NPCLoot npcLoot, params IItemDropRule[] rules)
    {
        var rule = new LeadingConditionRule(new Conditions.NotExpert());

        foreach (var item in rules)
            rule.OnSuccess(item);

        npcLoot.Add(rule);
    }
}
