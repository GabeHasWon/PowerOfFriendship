using PoF.Content.NPCs.EoD;

namespace PoF.Common.Globals.ItemGlobals;

internal class SpawnEoDGlobal : GlobalNPC
{
    private static bool OldRemix = false;

    public override void Load() => On_NPC.DoDeathEvents += HijackDeathEvents;

    private void HijackDeathEvents(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
    {
        if (self.type != NPCID.EmpressButterfly)
        {
            orig(self, closestPlayer);
            return;
        }

        if (closestPlayer.ZoneHallow)
            orig(self, closestPlayer);

        if (closestPlayer.ZoneGraveyard)
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>()))
            {
                Vector2 spawnPosition = self.Center + Main.rand.NextVector2Circular(50f, 50f);
                NPC.SpawnBoss((int)spawnPosition.X, (int)spawnPosition.Y, ModContent.NPCType<EmpressOfDeath>(), closestPlayer.whoAmI);
            }
        }

        if (NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>()) && NPC.AnyNPCs(NPCID.HallowBoss))
        {
            Main.npc[NPC.FindFirstNPC(NPCID.HallowBoss)].AddBuff(BuffID.Lovestruck, 60 * 10);
            Main.npc[NPC.FindFirstNPC(ModContent.NPCType<EmpressOfDeath>())].AddBuff(BuffID.Lovestruck, 60 * 10);
        }
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.EmpressButterfly;

    public override bool PreAI(NPC npc)
    {
        OldRemix = Main.remixWorld;
        Main.remixWorld = true;
        return true;
    }

    public override void PostAI(NPC npc) => Main.remixWorld = OldRemix;
}
