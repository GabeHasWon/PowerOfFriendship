using System;
using Terraria.GameContent.Bestiary;

namespace PoF.Content.NPCs.EoD;

public class EmpressOfDeath : ModNPC
{
    private enum EoDState : byte
    {
        Spawn,
        Idle,
        TeleportDash,
        SwordSpam,
        DeathAura, // Only accessible in first phase, normal mode
        SummonAdds, // Ghosts (scale in speed), Ghouls (hung from noose, released after some time, verlet?), Gargoyles (bat but stronger)
        WhipAdds,
    }

    private Player Target => Main.player[NPC.target];
    private float LifeFactor => NPC.life / (float)NPC.lifeMax;

    private EoDState State
    {
        get => (EoDState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private ref float Timer => ref NPC.ai[1];
    private ref float CircleTimer => ref NPC.ai[2];

    private Vector2 storedPos = Vector2.Zero;

    public override void SetStaticDefaults() => Main.npcFrameCount[NPC.type] = 1;

    public override void SetDefaults()
    {
        NPC.width = 298;
        NPC.height = 256;
        NPC.damage = 100;
        NPC.defense = 35;
        NPC.lifeMax = 60000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = false;
        NPC.value = Item.buyPrice(0, 15, 0, 0);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Critter;
        NPC.DeathSound = SoundID.NPCDeath4;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Graveyard,
            new FlavorTextBestiaryInfoElement("Death."),
        ]);
    }

    public override void AI()
    {
        Timer++;
        NPC.rotation = NPC.velocity.X * 0.02f;

        switch (State)
        {
            case EoDState.Idle:
                IdleMovement();

                if (Timer > 120)
                {
                    //NPC.velocity = (targetPosition - NPC.Center) * 0.03f;
                    SwitchState(EoDState.SwordSpam);
                }

                break;

            case EoDState.TeleportDash:
                TeleportDash();
                break;

            case EoDState.SwordSpam:
                IdleMovement();

                if (Timer > 30 && Timer < 80)
                {
                    if (Timer % 5 == 0)
                    {
                        float rot = (Timer - 30) / 80f * MathHelper.PiOver2;
                        var vel = NPC.DirectionTo(Target.Center).RotatedByRandom(0.2f).RotatedBy(rot * -Target.direction) * Main.rand.NextFloat(5, 9);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileID.FairyQueenLance, 60, 0, Main.myPlayer, vel.ToRotation());
                    }
                }

                if (Timer == 100)
                    SwitchState(EoDState.Idle);
                break;

            default: // Spawn
                if (Timer > 120)
                    SwitchState(EoDState.Idle);
                break;
        }
    }

    private void IdleMovement()
    {
        CircleTimer++;
        Vector2 targetPosition = Target.Center - new Vector2(0, 300) + new Vector2(0, 100 + (50 * LifeFactor)).RotatedBy(CircleTimer * 0.05f);
        NPC.Center = Vector2.Lerp(NPC.Center, targetPosition, 0.03f);
        NPC.velocity = Vector2.Zero;
        NPC.damage = 0;
    }

    private void TeleportDash()
    {
        // Teleport past the player, bats reconglomerate on the other side, cooldown
        const float FadeTime = 15f;
        const float TeleportTime = 45f;

        NPC.velocity *= 0.9f;

        if (Timer < FadeTime)
            NPC.Opacity = 1 - (Timer / (FadeTime - 1));
        else if (Timer == FadeTime)
        {
            Vector2 dir = NPC.DirectionTo(Target.Center);
            Vector2 nextPos = Target.Center + (dir * 350);
            NPC.dontTakeDamage = true;
            storedPos = nextPos;
        }
        else if (Timer == FadeTime + TeleportTime)
            NPC.Center = storedPos;
        else if (Timer > FadeTime + TeleportTime && Timer < FadeTime * 2 + TeleportTime)
            NPC.Opacity = (Timer - (FadeTime + TeleportTime - 1)) / FadeTime;
        else if (Timer == FadeTime * 2 + TeleportTime)
            NPC.dontTakeDamage = false;
        else if (Timer > FadeTime * 2 + TeleportTime * 2)
            SwitchState(EoDState.Idle);
    }

    private void SwitchState(EoDState toState)
    {
        State = toState;
        Timer = 0;
        NPC.TargetClosest(false);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
        {
        }
    }
}