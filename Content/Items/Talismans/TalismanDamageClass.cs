using System.Collections.Generic;
using Terraria.Localization;

namespace PoF.Content.Items.Talismans;

internal class TalismanDamageClass : DamageClass
{
    public static TalismanDamageClass Self => ModContent.GetInstance<TalismanDamageClass>();

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
    {
        if (damageClass == Generic || damageClass == Summon)
            return StatInheritanceData.Full;
        
        return StatInheritanceData.None;
    }

    public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Summon;
}
