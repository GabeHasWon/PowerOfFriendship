using NetEasy;
using PoF.Content.Items.Accessories;
using System;
using System.Linq;

namespace PoF.Common.Syncing;

[Serializable]
public class ApplyAmbrosiaCellModule(int npc, int projIdentity, int damage) : Module
{
    private readonly short _npc = (short)npc;
    private readonly short _proj = (short)projIdentity;
    private readonly int _damage = damage;

    protected override void Receive()
    {
        Main.npc[_npc].GetGlobalNPC<StellarAmbrosia.StellarNPC>().ApplyCell(Main.npc[_npc], Main.projectile.First(x => x.identity == _proj), _damage);

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
