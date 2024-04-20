using PoF.Content.Items.Talismans;
using System;

namespace PoF.Content.Items.Accessories;

public class DaisyChain : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(30, 36);
        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.sellPrice(gold: 2);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<DaisyPlayer>().equipped = true;

    class DaisyPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    class DaisyProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.DamageType.CountsAsClass<TalismanDamageClass>() && projectile.friendly && projectile.TryGetOwner(out var owner) && owner.GetModPlayer<DaisyPlayer>().equipped)
            {
                if (owner.statLife >= owner.statLifeMax2 || !Main.rand.NextBool(3))
                    return;

                int damage = (int)Math.Ceiling(damageDone / 8f);

                if (owner.statLife + damage > owner.statLifeMax2)
                    damage = owner.statLifeMax2 - owner.statLife;

                owner.Heal(damage);
            }
        }
    }
}