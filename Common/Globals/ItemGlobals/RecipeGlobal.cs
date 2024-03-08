namespace PoF.Common.Globals.ItemGlobals;

internal class RecipeGlobal : GlobalItem
{
    public override void AddRecipes()
    {
        // Wand of Sparking recipe
        Recipe.Create(ItemID.WandofSparking)
            .AddIngredient(ItemID.Wood, 5)
            .AddIngredient(ItemID.Torch, 2)
            .AddTile(TileID.WorkBenches)
            .Register();

        // Leather from Vertebrae recipe
        Recipe.Create(ItemID.Leather)
            .AddIngredient(ItemID.Vertebrae, 5)
            .AddTile(TileID.WorkBenches)
            .Register();

        // Leather Whip recipe
        Recipe.Create(ItemID.BlandWhip)
            .AddIngredient(ItemID.Leather, 8)
            .AddIngredient(ItemID.Rope, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
