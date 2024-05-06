using PoF.Content.Items.Misc;

namespace PoF.Content.Buffs;

public class MothPetBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.lightPet[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.buffTime[buffIndex] = 18000;
        bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<DeathsHeadEgg.BabyMoth>()] <= 0;

        if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, ModContent.ProjectileType<DeathsHeadEgg.BabyMoth>(), 0, 0f, player.whoAmI);
    }
}
