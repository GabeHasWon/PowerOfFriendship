using Terraria.Localization;
using Terraria.ObjectData;

namespace PoF.Content.Items.Blocks;

public class EoDTrophy : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.addTile(Type);

        DustType = DustID.WoodFurniture;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(120, 85, 60), name);
    }
}

public class EoDTrophyItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<EoDTrophy>());
        Item.rare = ItemRarityID.Blue;
    }
}