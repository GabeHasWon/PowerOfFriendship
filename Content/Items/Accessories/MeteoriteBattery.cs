using PoF.Common.Globals.ProjectileGlobals;
using PoF.Content.Items.Talismans;

namespace PoF.Content.Items.Accessories;

public class MeteoriteBattery : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(28, 26);
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(silver: 30);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<BatteryPlayer>().equipped = true;
        player.GetDamage<TalismanDamageClass>() += 0.08f;
    }

    class BatteryPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    class BatteryProjectile : GlobalProjectile
    {
        public override void PostAI(Projectile projectile)
        {
            if (projectile.DamageType.CountsAsClass<TalismanDamageClass>() && projectile.friendly && projectile.TryGetOwner(out var owner) 
                && owner.GetModPlayer<BatteryPlayer>().equipped && !TalismanGlobal.IsMinorTalismanProjectile.Contains(projectile.type))
                Lighting.AddLight(projectile.Center, new Vector3(0.4f));
        }
    }
}