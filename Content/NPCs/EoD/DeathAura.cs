using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using System.Linq;

namespace PoF.Content.NPCs.EoD;

class DeathAura : ModProjectile
{
    private const float FadeInTime = 120;
    private const float FadeOutTime = 240;

    public NPC EmpressOwner => Main.npc[(int)EoDOwner];

    public override string Texture => "Terraria/Images/NPC_0";

    private static int MaxAuraPoints => 1200;

    private ref float Time => ref Projectile.ai[0];
    private ref float EoDOwner => ref Projectile.ai[1];

    readonly List<float> auraRotations = [];
    List<Vector2> auraPositions = [];
    bool auraInitialized = false;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(30);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1200;
        Projectile.Opacity = 0f;
    }

    public override bool CanHitPlayer(Player target) => Time > FadeInTime && Projectile.timeLeft > FadeOutTime;

    public override void AI()
    {
        if (!NPC.AnyNPCs(ModContent.NPCType<EmpressOfDeath>()))
        {
            Projectile.Kill();
            return;
        }

        if (!auraInitialized)
        {
            InitializeAura();
            auraInitialized = true;
        }

        Time++;

        if (Main.expertMode || Main.npc[(int)EoDOwner].life < Main.npc[(int)EoDOwner].lifeMax)
            Projectile.timeLeft++;

        if (Time < FadeInTime)
            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.5f, 0.01f);
        else 
            Projectile.Opacity = Time >= FadeInTime && Projectile.timeLeft > FadeOutTime
            ? MathHelper.Lerp(Projectile.Opacity, 1, 0.05f)
            : MathHelper.Lerp(Projectile.Opacity, 0, 0.05f);

        for (int i = 0; i < MaxAuraPoints; i++)
            ModifySingleAuraPosition(i);

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];
            float dist = plr.DistanceSQ(Projectile.Center);

            if (plr.active && !plr.dead && dist < 2400 * 2400 && dist > 1200 * 1200)
            {
                if (dist > 2300 * 2300)
                {
                    plr.Teleport(Projectile.Center, TeleportationStyleID.DemonConch);
                    plr.SetImmuneTimeForAllTypes(60);
                }
                else
                {
                    plr.velocity += plr.DirectionTo(Projectile.Center) * 2f;

                    if (plr.velocity.LengthSquared() > 12 * 12)
                        plr.velocity = Vector2.Normalize(plr.velocity * 12);
                }
            }
        }
    }

    private void ModifySingleAuraPosition(int i)
    {
        auraPositions[i] = Vector2.Normalize(auraPositions[i]);

        if (Time < FadeInTime)
            auraPositions[i] *= Time / FadeInTime * 1200;
        else if (Time >= FadeInTime && Projectile.timeLeft > FadeOutTime)
            auraPositions[i] *= 1200;
        else
            auraPositions[i] *= Projectile.timeLeft / FadeOutTime * 1200f;

        auraPositions[i] *= MathF.Sin((Time + i * 4f) * 0.05f) * 0.05f + 1f;
    }

    private void InitializeAura()
    {
        auraPositions = [];

        for (int i = 0; i < MaxAuraPoints; ++i)
        {
            float rot = i / (float)MaxAuraPoints * MathHelper.TwoPi * 10f;
            auraPositions.Add(new Vector2(0, 1).RotatedBy(rot));
            auraRotations.Add(rot);
        }
    }

    private Vector2[] GetRealAuraPositions()
    {
        Span<Vector2> positions = stackalloc Vector2[MaxAuraPoints];
        float adjustment = 1;
        float pullAngle = 0;

        if (EmpressOwner.ai[0] == (float)EmpressOfDeath.EoDState.PullAura)
        {
            var owner = EmpressOwner.ModNPC as EmpressOfDeath;
            adjustment = owner.auraPullStrength;
            pullAngle = owner.auraPullAngle;
        }

        for (int i = 0; i < MaxAuraPoints; ++i)
        {
            positions[i] = auraPositions[i];

            if (adjustment != 1 && GetAngleIsValid(positions, pullAngle, i, out float distance))
                positions[i] *= MathHelper.Lerp(adjustment, 1, distance);

            positions[i] += Projectile.Center;
        }

        return [..positions];
    }

    private static bool GetAngleIsValid(Span<Vector2> positions, float pullAngle, int i, out float distance)
    {
        float angle = positions[i].AngleTo(Vector2.Zero) + MathHelper.Pi;
        distance = MathF.Pow(Math.Abs(angle - pullAngle) / MathHelper.PiOver4, 2);
        return distance < MathHelper.PiOver4;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2[] auraPositions = GetRealAuraPositions();
        return auraPositions.Any(x => targetHitbox.Contains(x.ToPoint()));
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.HitDirectionOverride = 0;
        target.velocity = target.DirectionTo(Projectile.Center) * 16;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (NPC.AnyNPCs(NPCID.HallowBoss))
            target.AddBuff(BuffID.Blackout, 300);

        if (Main.expertMode)
            target.AddBuff(BuffID.Darkness, 240);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        AuraPrimDrawer drawer = default;
        drawer.Draw(this);
        return false;
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct AuraPrimDrawer
    {
        private static readonly VertexStrip _vertexStrip = new();

        public readonly void Draw(DeathAura proj)
        {
            MiscShaderData miscShaderData = GameShaders.Misc["PoF:UnholyFlame"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(2f * proj.Projectile.Opacity);
            miscShaderData.Apply();
            _vertexStrip.PrepareStripWithProceduralPadding(proj.GetRealAuraPositions(), [..proj.auraRotations], StripColors, StripWidth, -Main.screenPosition + proj.Projectile.Size / 2f);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private readonly Color StripColors(float progressOnStrip)
        {
            var result = Color.Lerp(Color.Purple, Color.DarkBlue, MathF.Pow(MathF.Sin(progressOnStrip * 15), 2) * 0.75f);
            result.A = (byte)(result.A * 0.7f);

            if (progressOnStrip < 0.1f)
                result *= progressOnStrip / 0.1f;
            return result;
        }

        private readonly float StripWidth(float progress) => 40;
    }
}