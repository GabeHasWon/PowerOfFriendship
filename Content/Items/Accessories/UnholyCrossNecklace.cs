using PoF.Content.Items.Talismans;
using ReLogic.Content;
using System;
using Terraria.GameContent;

namespace PoF.Content.Items.Accessories;

public class UnholyCrossNecklace : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(26, 36);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.longInvince = true;
        player.GetModPlayer<UnholyPlayer>().equipped = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.CrossNecklace)
            .AddIngredient<UnholyAmulet>()
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }

    private class UnholyPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;

        public override void OnHurt(Player.HurtInfo info)
        {
            if (equipped && Player.ownedProjectileCounts[ModContent.ProjectileType<UnholyCross>()] == 0 && Player.whoAmI == Main.myPlayer)
            {
                var src = Player.GetSource_OnHurt(info.DamageSource);
                int proj = Projectile.NewProjectile(src, Player.Center, Vector2.Zero, ModContent.ProjectileType<UnholyCross>(), 0, 0, Player.whoAmI);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }
        }
    }

    public class UnholyCross : ModProjectile
    {
        const int MaxTimeLeft = 600;

        private ref float Timer => ref Projectile.ai[0];
        private ref float SpinSpeed => ref Projectile.ai[1];
        private ref float OffsetRotation => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(341, 443);
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.Opacity = 0f;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (SpinSpeed++ == 0)
                Projectile.rotation = MathHelper.PiOver2 * (Main.rand.NextBool() ? -1 : 1);
            else if (SpinSpeed % 100 == 0)
                OffsetRotation -= Main.rand.NextFloat(MathHelper.PiOver2) - MathHelper.PiOver4;

            float sine = MathF.Pow(MathF.Sin(Timer++ * 0.02f), 2);
            Projectile.rotation = Utils.AngleLerp(Utils.AngleLerp(Projectile.rotation, 0, 0.03f), OffsetRotation, 0.15f);
            OffsetRotation *= 0.6f;

            if (Projectile.timeLeft > MaxTimeLeft - 60)
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.33f, 0.05f);
            else if (Projectile.timeLeft > 30)
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, sine * 0.33f + 0.15f, 0.2f);
            else
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0, 0.2f);

            UpdateNPCs();
        }

        private void UpdateNPCs()
        {
            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                NPC npc = Main.npc[i];

                if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < 500 * 500)
                    npc.GetGlobalNPC<UnholyCrossNPC>().cursed = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.Red with { A = 0 } * Projectile.Opacity;

        public override void PostDraw(Color lightColor)
        {
            var projTex = TextureAssets.Projectile[Type].Value;
            var baseCol = Color.Red with { A = 0 };
            var drawPos = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < 2; ++i)
            {
                Color col = baseCol * (1 - i * 0.25f) * Projectile.Opacity;
                Main.spriteBatch.Draw(projTex, drawPos, null, col, Projectile.rotation, projTex.Size() / 2f, 1 + (i * 0.15f), SpriteEffects.None, 0);
            }

            var tex = ModContent.Request<Texture2D>("PoF/Content/Items/Talismans/UnholyPentagramAlpha").Value;

            for (int i = 0; i < 3; ++i)
            {
                float rot = Projectile.rotation * (i % 2 == 0 ? -1 : 1) + (i / MathHelper.TwoPi) + Main.GameUpdateCount * 0.02f;
                Color color = Color.Red with { A = 0 } * Projectile.Opacity * (1 - (i * 0.2f));
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, rot, tex.Size() / 2f, 4.2f + (i * 0.25f), SpriteEffects.None, 0);
            }
        }
    }

    public class UnholyCrossNPC : GlobalNPC
    {
        private static Asset<Texture2D> CrossIcon;

        public override bool InstancePerEntity => true;

        public bool cursed = false;

        private float _crossAlpha = 0;

        public override void SetStaticDefaults() => CrossIcon = ModContent.Request<Texture2D>("PoF/Content/Items/Accessories/UnholyCrossIcon");

        public override void ResetEffects(NPC npc) => cursed = false;

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (cursed)
                modifiers.FinalDamage *= 1.15f;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (cursed)
                _crossAlpha = MathHelper.Lerp(_crossAlpha, 1, 0.1f);
            else
                _crossAlpha = MathHelper.Lerp(_crossAlpha, 0, 0.1f);
            
            if (_crossAlpha < 0.01f)
                return;

            float rot = MathF.Sin(Main.GameUpdateCount * 0.06f + npc.whoAmI) * 0.2f;
            var pos = npc.Top - new Vector2(0, 26) - screenPos;
            spriteBatch.Draw(CrossIcon.Value, pos, null, Color.Red with { A = 0 } * _crossAlpha, rot, CrossIcon.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }
}