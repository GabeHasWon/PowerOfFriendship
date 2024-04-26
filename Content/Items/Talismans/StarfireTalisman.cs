using PoF.Common.Globals.ProjectileGlobals;
using System;
using System.Collections.Generic;

namespace PoF.Content.Items.Talismans;

internal class StarfireTalisman : Talisman
{
    protected override float TileRange => 85;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.damage = 150;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.mana = 7;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<StarfireInvader>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.width = 32;
        Item.height = 44;
        Item.value = Item.buyPrice(0, 4);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.FragmentStardust, 16)
        .AddTile(TileID.LunarCraftingStation)
        .Register();

    private class StarfireInvader : ModProjectile
    {
        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float ProjectileTime => ref Projectile.ai[2];

        List<int> _ownedProjectiles = [];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(46, 38);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Projectile.frame = (int)(Projectile.frameCounter++ / 6f % 4);
            Projectile.rotation = 0.02f * Projectile.velocity.X;

            if (!Despawning)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 12;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 1.5f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                if (ProjectileTime++ > 60 * (Projectile.Owner().HeldItem.useTime / 15f) && _ownedProjectiles.Count < 3)
                {
                    ProjectileTime = 0;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        var src = Projectile.GetSource_FromAI();
                        int type = ModContent.ProjectileType<StarfireShot>();
                        int p = Projectile.NewProjectile(src, Projectile.Center, Vector2.Zero, type, Projectile.damage, 0f, Projectile.owner, Projectile.whoAmI);

                        _ownedProjectiles.Add(p);

                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, p);
                    }

                    Despawning = !PayMana(Projectile);
                }

                if (_ownedProjectiles.Count > 0 && Projectile.GetNearestNPCTarget(out NPC npc, 600f) && ProjectileTime > 0)
                {
                    int proj = Main.rand.Next(_ownedProjectiles);
                    Main.projectile[proj].ai[1] = 1;
                    Main.projectile[proj].velocity = Main.projectile[proj].DirectionTo(npc) * 12;

                    _ownedProjectiles.Remove(proj);
                    ProjectileTime = 0;
                }

                if (!Despawning)
                    Despawning = HandleBasicFunctions<StarfireTalisman>(Projectile, ref Time, 2.25f);

                if (Time % 20 == 0)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.KryptonMoss, Projectile.velocity.X, Projectile.velocity.Y);
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.velocity.Y += 0.4f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }
    }

    public class StarfireShot : ModProjectile
    {
        private Projectile Parent => Main.projectile[(int)ParentWhoAmI];
        private ref float ParentWhoAmI => ref Projectile.ai[0];
        
        private bool LetGo
        {
            get => Projectile.ai[1] == 1;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            TalismanGlobal.IsMinorTalismanProjectile.Add(Type);
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(14);
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.Opacity = Parent.Opacity;
            Projectile.frame = (int)(Projectile.frameCounter++ / 6f % 4);

            if (!Parent.active || Parent.type != ModContent.ProjectileType<StarfireInvader>())
            {
                Projectile.Kill();
                return;
            }

            if (!LetGo)
            {
                Projectile.timeLeft++;
                Time++;
                Projectile.Center = Parent.Center + new Vector2(MathF.Sin(Time * 0.04f) * 40, MathF.Sin((Time + MathHelper.PiOver2) * 0.08f) * 28);
            }
            else
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!LetGo)
                return true;

            var tex = ModContent.Request<Texture2D>(GlowTexture).Value;
            var col = Color.White * Projectile.Opacity;

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, col with { A = 0 }, Projectile.rotation, new Vector2(18) / 2, 1f, SpriteEffects.None, 0);
            return true;
        }
    }
}
