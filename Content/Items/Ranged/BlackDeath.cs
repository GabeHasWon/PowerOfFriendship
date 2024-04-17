
namespace PoF.Content.Items.Ranged;

public class BlackDeath : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 250;
        Item.DamageType = DamageClass.Ranged;
        Item.Size = new Vector2(70, 26);
        Item.useTime = Item.useAnimation = 100;
        Item.useTurn = false;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 4;
        Item.channel = true;
        Item.value = Item.buyPrice(0, 10, 0, 0);
        Item.rare = ItemRarityID.Pink;
        Item.autoReuse = true;
        Item.shoot = ProjectileID.TorchGod;
        Item.shootSpeed = 10f;
        Item.useAmmo = AmmoID.Rocket;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        type = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher][source.AmmoItemIdUsed];
        Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.05f), type, damage, knockback, player.whoAmI);

        if (Main.rand.NextBool(3))
            Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.05f), type, damage, knockback, player.whoAmI);

        return false;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-20, 0);
}