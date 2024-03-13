using PoF.Content.Items.Talismans;

namespace PoF.Common.Globals.ProjectileGlobals;

internal class TalismanGlobal : GlobalProjectile
{
    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!projectile.DamageType.CountsAsClass(ModContent.GetInstance<TalismanDamageClass>()))
            return;

        Player plr = projectile.Owner();
        int buffIndex = plr.FindBuffIndex(BuffID.ManaSickness);

        if (buffIndex == -1)
            return;

        float mult = Player.manaSickLessDmg * (plr.buffTime[buffIndex] / (float)Player.manaSickTime);
        modifiers.FinalDamage *= mult;
    }
}
