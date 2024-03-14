using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Accessories;

public class ManaMushroom : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(24, 40);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(gold: 1, silver: 10);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.manaFlower = true;
        player.GetModPlayer<ManaMushroomPlayer>().equipped = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ManaFlower)
            .AddIngredient(ItemID.GlowingMushroom)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }

    class ManaMushroomPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    class ManaMushroomProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if (!projectile.DamageType.CountsAsClass<TalismanDamageClass>() || ProjectileLoader.CanDamage(projectile) == false)
                return;

            int type = ModContent.ProjectileType<TwoTopTalisman.TwoTopSpores>();

            if (projectile.damage > 0 && projectile.friendly && projectile.TryGetOwner(out var owner) && owner.GetModPlayer<ManaMushroomPlayer>().equipped && projectile.type != type)
            {
                if (Main.myPlayer == projectile.owner && Main.rand.NextBool(30))
                {
                    Vector2 vel = projectile.velocity * Main.rand.NextFloat(0.6f, 0.95f) + new Vector2(0, Main.rand.NextFloat(4f, 7f)).RotatedByRandom(MathHelper.Pi);
                    int proj = Projectile.NewProjectile(projectile.GetSource_FromAI(), projectile.Center, vel, type, projectile.damage / 2, 0, projectile.owner);

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                }
            }
        }
    }
}