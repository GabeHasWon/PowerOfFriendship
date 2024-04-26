using System.IO;
using Terraria.ModLoader.IO;

namespace PoF.Common.Globals;

internal class DownedEoDSystem : ModSystem
{
    public bool downedEoD = false;

    public override void ClearWorld() => downedEoD = false;
    public override void SaveWorldData(TagCompound tag) => tag.Add(nameof(downedEoD), downedEoD);
    public override void LoadWorldData(TagCompound tag) => downedEoD = tag.GetBool(nameof(downedEoD));
    public override void NetSend(BinaryWriter writer) => writer.Write(downedEoD);
    public override void NetReceive(BinaryReader reader) => downedEoD = reader.ReadBoolean();
}
