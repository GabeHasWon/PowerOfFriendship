namespace PoF.Content.Buffs;

public class FuryBuff : ModBuff
{
    public const int Cap = 300;

    public override void SetStaticDefaults() => Main.debuff[Type] = true;
    public override void Update(Player plr, ref int slot) => plr.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 1 / 3f * MathHelper.Min(1, plr.buffTime[slot] / (float)Cap);
}
