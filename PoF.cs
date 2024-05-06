global using Terraria;
global using Terraria.ID;
global using Terraria.DataStructures;
global using Terraria.ModLoader;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using System.IO;
using System.Collections.Generic;
using Terraria.Localization;
using PoF.Content.NPCs.EoD;
using PoF.Common.Globals;

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
        EquipLoader.AddEquipTexture(this, "PoF/Content/Items/Armor/SpikedGuardian/Hellrobe_Legs", EquipType.Legs, null, "HellrobeLegs");

        NPCUtils.NPCUtils.AutoloadModBannersAndCritters(this);
        NPCUtils.NPCUtils.TryLoadBestiaryHelper();
    }

    public override void PostSetupContent()
    {
        NetEasy.NetEasy.Register(this);

        if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            return;

        bossChecklist.Call(
            "LogBoss",
            this,
            nameof(EmpressOfDeath),
            15.1f,
            () => ModContent.GetInstance<DownedEoDSystem>().downedEoD,
            ModContent.NPCType<EmpressOfDeath>(),
            new Dictionary<string, object>()
            {
                ["spawnInfo"] = Language.GetText("Mods.PoF.EoDSpawnInfo"),
                ["customPortrait"] = (SpriteBatch spriteBatch, Rectangle rect, Color color) => {
                    Texture2D texture = ModContent.Request<Texture2D>("PoF/Content/NPCs/EoD/EmpressOfDeath_Checklist").Value;
                    var centered = new Vector2(rect.X + rect.Width / 2 - texture.Width / 2, rect.Y + rect.Height / 2 - texture.Height / 2);
                    spriteBatch.Draw(texture, centered, color);
                }
            }
        );
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetEasy.NetEasy.HandleModule(reader, whoAmI);

    public override void Unload()
    {
        NPCUtils.NPCUtils.UnloadMod(this);
        NPCUtils.NPCUtils.UnloadBestiaryHelper();
    }
}