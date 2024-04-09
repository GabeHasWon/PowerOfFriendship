using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace PoF.Content.NPCs.EoD;

class EoDPortal : ModProjectile
{
    public Vector2 EndOfRope => AttachedNPC == -2 ? SwingOfRope : Main.npc[(int)AttachedNPC].Center;
    public Vector2 SwingOfRope => Projectile.Center + new Vector2(0, Length * ExtendFactor).RotatedBy(MathF.Sin(Time * 0.03f) * 0.3f);
    public float ExtendFactor => MathHelper.Clamp(retractionTime / 120f, 0, 1);

    private ref float Time => ref Projectile.ai[0];
    private ref float Length => ref Projectile.ai[1];
    private ref float AttachedNPC => ref Projectile.ai[2];

    internal float endOfRopeRotation = 0;
    internal float retractionTime = 0;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;

    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.Size = new Vector2(56);
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60 * 10;
        Projectile.Opacity = 0f;
        Projectile.hide = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        Projectile.rotation += 0.1f;
        Projectile.timeLeft = 2;
        Time++;

        if (AttachedNPC == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            AttachedNPC = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<RottenGhoulHanging>(), 0, Projectile.whoAmI);
            Projectile.netUpdate = true;
        }

        if (AttachedNPC == -2 || AttachedNPC != 0 && (!Main.npc[(int)AttachedNPC].active || Main.npc[(int)AttachedNPC].type != ModContent.NPCType<RottenGhoulHanging>()))
        {
            AttachedNPC = -2;

            if (retractionTime > 121)
                retractionTime = 121;

            retractionTime--;
            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 0.05f);

            if (retractionTime < 0)
                Projectile.Kill();
        }
        else
        {
            retractionTime++;

            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.02f);
        }
    }

    public override void DrawBehind(int index, List<int> nt, List<int> behindNPCs, List<int> bp, List<int> op, List<int> ow) => behindNPCs.Add(index);

    public override void PostDraw(Color lightColor)
    {
        for (int i = 0; i < 3; ++i)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float factor = i / 3f;
            var color = Color.Lerp(Color.White, Color.Black, factor) * Projectile.Opacity;
            float rot = Projectile.rotation * (i % 2 == 0 ? -1 : 1) + i * MathHelper.PiOver4;

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, rot, tex.Size() / 2f, 1 - i / 4f, SpriteEffects.None, 0);
        }

        DrawBezier();
    }

    private void DrawBezier()
    {
        const float MaxIterations = 40;

        Vector2 start = Projectile.Center;
        Vector2 middle = Projectile.Center + new Vector2(0, Length / 2f * ExtendFactor);
        Vector2 end = EndOfRope;
        Vector2 lastPos = start;
        var tex = TextureAssets.Tile[TileID.Rope].Value;
        var src = new Rectangle(94, 0, 8, 16);

        for (int i = 0; i <= MaxIterations; ++i)
        {
            float factor = i / MaxIterations;
            var currentPos = Vector2.Lerp(Vector2.Lerp(start, middle, factor), Vector2.Lerp(middle, end, factor), factor);
            float rot = currentPos.AngleTo(lastPos) - MathHelper.PiOver2;
            var col = Color.White * Projectile.Opacity;

            if (i < 5)
                col *= i / 5f;

            if (i == MaxIterations)
                endOfRopeRotation = rot;

            Main.instance.LoadTiles(TileID.Rope);
            Main.spriteBatch.Draw(tex, currentPos - Main.screenPosition, src, col, rot, src.Size() / 2f, 1f, SpriteEffects.None, 0);
            lastPos = currentPos;
        }
    }
}