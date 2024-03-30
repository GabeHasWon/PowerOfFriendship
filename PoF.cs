global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.ModLoader;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace PoF;

public class PoF : Mod
{
    public override void Load()
    {
        GameShaders.Misc["PoF:UnholyFlame"] = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(doUse: true);
        GameShaders.Misc["PoF:UnholyFlame"].UseImage0("Images/Extra_191");
        GameShaders.Misc["PoF:UnholyFlame"].UseImage1(ModContent.Request<Texture2D>("PoF/Assets/Textures/ShaderFlame", ReLogic.Content.AssetRequestMode.ImmediateLoad));
        GameShaders.Misc["PoF:UnholyFlame"].UseImage2("Images/Extra_190");

        EquipLoader.AddEquipTexture(this, "PoF/Content/Items/Armor/SpikedGuardian/SpikedGuardianRobe_Legs", EquipType.Legs, null, "SpikedGuardianRobeLegs");
        EquipLoader.AddEquipTexture(this, "PoF/Content/Items/Armor/SpikedGuardian/SpikedGuardianRobeGreen_Legs", EquipType.Legs, null, "SpikedGuardianRobeGreenLegs");
        EquipLoader.AddEquipTexture(this, "PoF/Content/Items/Armor/SpikedGuardian/SpikedGuardianRobePink_Legs", EquipType.Legs, null, "SpikedGuardianRobePinkLegs");
    }
}