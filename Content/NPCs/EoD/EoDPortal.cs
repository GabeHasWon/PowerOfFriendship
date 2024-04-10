using PoF.Common;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.RGB;

namespace PoF.Content.NPCs.EoD;

class EoDPortal : ModProjectile
{
    public Vector2 EndOfRope => AttachedNPC == -2 ? SwingOfRope : Main.npc[(int)AttachedNPC].Center;
    public Vector2 SwingOfRope => Projectile.Center + new Vector2(0, Length * ExtendFactor).RotatedBy(MathF.Sin(Time * 0.03f) * 0.3f);
    public float ExtendFactor => MathHelper.Clamp(retractionTime / 120f, 0, 1);

    private ref float Time => ref Projectile.ai[0];
    private ref float Length => ref Projectile.ai[1];
    private ref float AttachedNPC => ref Projectile.ai[2];
    
    private int attachedType = 0;
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
            bool isMoth = !Main.rand.NextBool(2);
            attachedType = !isMoth ? ModContent.NPCType<RottenGhoulHanging>() : ModContent.NPCType<DeathsHeadMoth>();

            if (isMoth)
                AttachedNPC = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, attachedType, 0, 0, Projectile.whoAmI, 250);
            else
                AttachedNPC = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, attachedType, 0, Projectile.whoAmI);

            Projectile.netUpdate = true;
        }

        if (AttachedNPC == -2 || AttachedNPC != 0 && (!Main.npc[(int)AttachedNPC].active || Main.npc[(int)AttachedNPC].type != attachedType))
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
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            float factor = i / 3f;
            var color = Color.Lerp(Color.White, Color.Black, factor) * Projectile.Opacity;
            float rot = Projectile.rotation * (i % 2 == 0 ? -1 : 1) + i * MathHelper.PiOver4;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, rot, texture.Size() / 2f, 1 - i / 4f, SpriteEffects.None, 0);
        }

        int tileType = attachedType == ModContent.NPCType<DeathsHeadMoth>() ? TileID.Chain : TileID.Rope;
        Main.instance.LoadTiles(TileID.Rope);

        var src = new Rectangle(94, 0, 8, 16);
        Vector2 start = Projectile.Center;
        Vector2 middle = tileType == TileID.Chain
            ? Vector2.Lerp(start, EndOfRope, 0.5f) + new Vector2(0, 150)
            : Projectile.Center + new Vector2(0, Length / 2f * ExtendFactor);
        var tex = TextureAssets.Tile[tileType];
        Bezier.DrawBezier(tex, src, tileType == TileID.Chain ? 60 : 36, start, middle, EndOfRope, Color.White * Projectile.Opacity, out endOfRopeRotation);
    }
}