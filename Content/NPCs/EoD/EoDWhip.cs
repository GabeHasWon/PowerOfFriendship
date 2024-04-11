using PoF.Content.Items.Whips;
using System;
using System.Collections.Generic;

namespace PoF.Content.NPCs.EoD;

public class EoDWhip : ModProjectile
{
    private ref float Timer => ref Projectile.ai[0];

    private ref float ProjectileOwner => ref Projectile.ai[2];

    public override void Load()
    {
        On_Projectile.FillWhipControlPoints += HijackControlPointsForFlagellator;
        On_Projectile.GetWhipSettings += HijackGetTimeToFlyOut;
    }

    private void HijackGetTimeToFlyOut(On_Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier)
    {
        orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);

        if (proj.type != ModContent.ProjectileType<EoDWhip>())
            return;

        timeToFlyOut = 60;
    }

    private void HijackControlPointsForFlagellator(On_Projectile.orig_FillWhipControlPoints orig, Projectile proj, List<Vector2> controlPoints)
    {
        if (proj.type != ModContent.ProjectileType<EoDWhip>())
        {
            orig(proj, controlPoints);
            return;
        }

        // Brutal decompiled code, stolen from vanilla. Used to make this whip fire out of the controlled handle instead of the player

        Projectile.GetWhipSettings(proj, out var timeToFlyOut, out var segments, out var rangeMultiplier);
        float swingTime = proj.ai[0] / timeToFlyOut;
        float num11 = 1.5f;
        float num12 = (float)Math.PI * 10f * (1f - swingTime * 1.5f) * (-proj.spriteDirection) / segments;
        float maxUseRange = swingTime * 1.5f;
        float num14 = 0f;

        if (maxUseRange > 1f)
        {
            num14 = (maxUseRange - 1f) / 0.5f;
            maxUseRange = MathHelper.Lerp(1f, 0f, num14);
        }

        Player player = Main.player[proj.owner];
        NPC ownerNpc = Main.npc[(int)proj.ai[2]];

        float useRange = 80 * swingTime * player.whipRangeMultiplier;
        float num16 = 8 * useRange * maxUseRange * rangeMultiplier / segments;

        Vector2 npcCenter = ownerNpc.Center + new Vector2(60, 20) - new Vector2(24, 0 + (ownerNpc.ModNPC as EmpressOfDeath).leftHandOffset);

        Vector2 vector = npcCenter;
        float num2 = -(float)Math.PI / 2f;
        Vector2 vector2 = vector;
        float num3 = (float)Math.PI / 2f + (float)Math.PI / 2f * proj.spriteDirection;
        Vector2 vector3 = vector;
        float num4 = (float)Math.PI / 2f;
        controlPoints.Add(npcCenter);

        for (int i = 0; i < segments; i++) // This is all unclean because I care not to understand it
        {
            float segmentFactor = (float)i / segments;
            float num6 = num12 * segmentFactor;
            Vector2 vector4 = vector + num2.ToRotationVector2() * num16;
            Vector2 vector5 = vector3 + num4.ToRotationVector2() * (num16 * 2f);
            Vector2 val = vector2 + num3.ToRotationVector2() * (num16 * 2f);
            float num7 = 1f - maxUseRange;
            float num8 = 1f - num7 * num7;
            Vector2 value = Vector2.Lerp(vector5, vector4, num8 * 0.9f + 0.1f);
            Vector2 vector6 = Vector2.Lerp(val, value, num8 * 0.7f + 0.3f);
            Vector2 spinPoint = npcCenter + (vector6 - npcCenter) * new Vector2(1f, num11);
            float num9 = num14;
            num9 *= num9;
            Vector2 item = spinPoint.RotatedBy(proj.rotation + 4.712389f * num9 * proj.spriteDirection, npcCenter);
            controlPoints.Add(item);
            num2 += num6;
            num4 += num6;
            num3 += num6;
            vector = vector4;
            vector3 = vector5;
            vector2 = val;
        }
    }

    public override void SetStaticDefaults() => ProjectileID.Sets.IsAWhip[Type] = true;

    public override void SetDefaults()
    {
        Projectile.DefaultToWhip();
        Projectile.WhipSettings.Segments = 50;
        Projectile.WhipSettings.RangeMultiplier = 1f;
        Projectile.minionSlots = 0;
        Projectile.hostile = true;
        Projectile.friendly = true;
    }

    public override bool? CanHitNPC(NPC target) => target.type == ModContent.NPCType<EmpressOfDeath>() ? false : null;

    public override bool PreAI()
    {
        if (!Main.npc[(int)ProjectileOwner].active)
        {
            Projectile.Kill();
            return false;
        }

        return true;
    }

    public override bool PreDraw(ref Color l) => WhipCommon.Draw(Projectile, Timer, new(0, 0, 18, 20), new(86, 22), new(66, 20), new(46, 20), new(26, 20), Projectile.Opacity);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.ModNPC is IStruckByWhipNPC struck)
            struck.OnHitByWhip(Projectile);
    }
}
