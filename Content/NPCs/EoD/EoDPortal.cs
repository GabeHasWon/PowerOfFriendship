using PoF.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
    private bool _spawnedNPC = false;

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

        Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 0.25f, 0.05f));

        if (!_spawnedNPC && Main.netMode != NetmodeID.MultiplayerClient)
        {
            bool isMoth = true;// !Main.rand.NextBool(2);
            attachedType = !isMoth ? ModContent.NPCType<RottenGhoulHanging>() : ModContent.NPCType<DeathsHeadMoth>();

            if (isMoth)
                AttachedNPC = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, attachedType, 0, 0, Projectile.identity, 250);
            else
                AttachedNPC = NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, attachedType, 0, Projectile.identity);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, (int)AttachedNPC);

            _spawnedNPC = Projectile.netUpdate = true;
        }

        if (!_spawnedNPC)
            return;

        if (AttachedNPC == -2 || (!Main.npc[(int)AttachedNPC].active || Main.npc[(int)AttachedNPC].type != attachedType) || MothNotAttached())
        {
            if (AttachedNPC != -2 && attachedType == ModContent.NPCType<DeathsHeadMoth>() && Main.netMode != NetmodeID.Server)
            {
                Vector2 start = Projectile.Center;
                Vector2 middle = Vector2.Lerp(start, EndOfRope, 0.5f) + new Vector2(0, 150);
                var bezier = Bezier.GetBezier(20, start, middle, EndOfRope);

                foreach (var item in bezier)
                    Gore.NewGore(Projectile.GetSource_FromAI(), item, Vector2.Zero, Mod.Find<ModGore>("Chain").Type);
            }

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

            if (AttachedNPC != 0 && !MothNotAttached())
                (Main.npc[(int)AttachedNPC].ModNPC as DeathsHeadMoth).UpdateFromParent();
        }
    }

    private bool MothNotAttached() => attachedType == ModContent.NPCType<DeathsHeadMoth>() && Main.npc[(int)AttachedNPC].ai[0] == 1;
    public override void DrawBehind(int index, List<int> nt, List<int> behindNPCs, List<int> bp, List<int> op, List<int> ow) => behindNPCs.Add(index);

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_spawnedNPC);
        writer.Write((short)attachedType);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _spawnedNPC = reader.ReadBoolean();
        attachedType = reader.ReadInt16();
    }

    public override void PostDraw(Color lightColor)
    {
        for (int i = 0; i < 3; ++i)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            float factor = i / 3f;
            var color = Color.Lerp(Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Color.Black, factor) * Projectile.Opacity;
            float rot = Projectile.rotation * (i % 2 == 0 ? -1 : 1) + i * MathHelper.PiOver4;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, rot, texture.Size() / 2f, 1 - i / 4f, SpriteEffects.None, 0);
        }

        if (attachedType == ModContent.NPCType<DeathsHeadMoth>() && AttachedNPC == -2)
            return;

        int tileType = attachedType == ModContent.NPCType<DeathsHeadMoth>() ? TileID.Chain : TileID.Rope;
        Main.instance.LoadTiles(tileType);

        var src = new Rectangle(94, 0, 8, 16);
        Vector2 start = Projectile.Center;
        Vector2 middle = tileType == TileID.Chain
            ? Vector2.Lerp(start, EndOfRope, 0.5f) + new Vector2(0, 150)
            : Projectile.Center + new Vector2(0, Length / 2f * ExtendFactor);
        var tex = TextureAssets.Tile[tileType];
        Bezier.DrawBezier(tex, src, 36, start, middle, EndOfRope, Color.White * Projectile.Opacity, out endOfRopeRotation);
    }
}