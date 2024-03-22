using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using static PoF.Content.Items.Talismans.Flagellator;

namespace PoF.Content.Items.Talismans;

internal class SpookyBoard : Talisman
{
    protected override float TileRange => 70;

    protected override void Defaults()
    {
        Item.Size = new(54, 22);
        Item.rare = ItemRarityID.Blue;
        Item.damage = 80;
        Item.useTime = 26;
        Item.useAnimation = 26;
        Item.mana = 14;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<SpookySummoner>();
        Item.shootSpeed = 5;
        Item.value = Item.buyPrice(0, 0, 30);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SpookyWood, 100)
            .AddIngredient(ItemID.Smolstar)
            .AddIngredient<Flagellator>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    private class SpookySummoner : ModProjectile
    {
        private Projectile Whip => Main.projectile[(int)WhipWhoAmI];

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float WhipWhoAmI => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(26, 68);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.aiStyle = -1;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.X / 30f;
            Projectile.direction = Math.Sign(Projectile.velocity.X);

            if (!Despawning)
            {
                bool invalidWhip = !Whip.active || Whip.type != ModContent.ProjectileType<FlagellatorWhip>();

                if (invalidWhip && GetTarget(out NPC npc))
                {
                    var src = Projectile.GetSource_FromAI();
                    int type = ModContent.ProjectileType<FlagellatorWhip>();
                    var vel = Projectile.DirectionTo(npc.Center) * 1.2f;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        WhipWhoAmI = Projectile.NewProjectile(src, Projectile.Center + vel, vel, type, Projectile.damage, 0, Projectile.owner, ai2: Projectile.whoAmI);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, (int)WhipWhoAmI);
                    }

                    PayMana(Projectile);
                }

                if (!invalidWhip)
                {
                    Projectile.GetWhipSettings(Whip, out float timeToFlyOut, out int _, out float _);

                    if (Whip.ai[0] > timeToFlyOut * 0.66f)
                        Projectile.frame = 2;
                    else if (Whip.ai[0] > timeToFlyOut * 0.33f)
                        Projectile.frame = 1;
                    else
                        Projectile.frame = 0;
                }
                else
                    Projectile.frame = 0;

                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 9;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.3f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                Despawning = HandleBasicFunctions<SpookyBoard>(Projectile, ref Time, 0.45f, false);
            }
            else
            {
                Projectile.velocity *= 0.92f;

                if (Projectile.velocity.LengthSquared() < 0.2f * 0.2f)
                {
                    Projectile.Kill();

                    for (int i = 0; i < 12; ++i)
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone);
                }
            }
        }

        private bool GetTarget(out NPC npc)
        {
            HashSet<int> npcs = [];

            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                NPC cur = Main.npc[i];

                if (cur.CanBeChasedBy() && cur.DistanceSQ(Projectile.Center) < 500 * 500)
                    npcs.Add(i);
            }

            npc = null;

            if (npcs.Count > 0)
                npc = Main.npc[Main.rand.Next(npcs.ToArray())];

            return npc != null;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effect = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            var src = new Rectangle(0, 0, 26, 34);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, lightColor, Projectile.rotation, src.Size() / new Vector2(2, 1), 1f, effect, 0);

            src.X = Projectile.frame * 28;
            src.Y = 34;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, lightColor, Projectile.rotation, src.Size() * new Vector2(0.5f, 0), 1f, effect, 0);
            return false;
        }
    }
}
