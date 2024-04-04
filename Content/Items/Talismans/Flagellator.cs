using PoF.Content.Items.Whips;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoF.Content.Items.Talismans;

internal class Flagellator : Talisman
{
    protected override float TileRange => 55;

    protected override void Defaults()
    {
        Item.rare = ItemRarityID.Purple;
        Item.damage = 62;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.mana = 8;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<FlagellatorHandle>();
        Item.shootSpeed = 5;
        Item.knockBack = 1f;
        Item.Size = new(44, 40);
        Item.value = Item.buyPrice(0, 10);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    private class FlagellatorHandle : ModProjectile
    {
        private Projectile Whip => Main.projectile[(int)WhipWhoAmI];

        private bool Despawning
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        private ref float Time => ref Projectile.ai[1];
        private ref float WhipWhoAmI => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = TalismanDamageClass.Self;
            Projectile.Size = new Vector2(22, 18);
            Projectile.minion = true;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (!Despawning)
            {
                bool invalidWhip = !Whip.active || Whip.type != ModContent.ProjectileType<FlagellatorWhip>();

                if (invalidWhip && Projectile.GetNearestNPCTarget(out NPC npc))
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

                    Projectile.rotation = vel.ToRotation();

                    bool paid = PayMana(Projectile);

                    if (!paid)
                        Despawning = true;
                }

                if (invalidWhip)
                    Projectile.rotation = Projectile.velocity.ToRotation();

                if (Main.myPlayer == Projectile.owner)
                {
                    const float Speed = 9;

                    Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 0.85f;

                    if (Projectile.velocity.LengthSquared() > Speed * Speed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize() * Speed;
                }

                bool stop = HandleBasicFunctions<Flagellator>(Projectile, ref Time, 1.3f, false);

                if (!Despawning && stop)
                    Despawning = true;
            }
            else
            {
                Projectile.Opacity *= 0.9f;
                Projectile.velocity *= 0.95f;
                Projectile.scale *= 0.97f;

                if (Projectile.Opacity < 0.05f)
                    Projectile.Kill();
            }
        }
    }

    public class FlagellatorWhip : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        private ref float ProjectileOwner => ref Projectile.ai[2];

        public override void Load() => On_Projectile.FillWhipControlPoints += HijackControlPointsForFlagellator;

        private void HijackControlPointsForFlagellator(On_Projectile.orig_FillWhipControlPoints orig, Projectile proj, List<Vector2> controlPoints)
        {
            if (proj.type != ModContent.ProjectileType<FlagellatorWhip>())
            {
                orig(proj, controlPoints);
                return;
            }

            // Brutal decompiled code, stolen from vanilla. Used to make this whip fire out of the controlled handle instead of the player

            Projectile.GetWhipSettings(proj, out var timeToFlyOut, out var segments, out var rangeMultiplier);
            float swingTime = proj.ai[0] / timeToFlyOut;
            float num11 = 1.5f;
            float num12 = (float)Math.PI * 10f * (1f - swingTime * 1.5f) * (-proj.spriteDirection) / segments;
            float num13 = swingTime * 1.5f;
            float num14 = 0f;

            if (num13 > 1f)
            {
                num14 = (num13 - 1f) / 0.5f;
                num13 = MathHelper.Lerp(1f, 0f, num14);
            }

            Player player = Main.player[proj.owner];
            Projectile ownerProjectile = Main.projectile[(int)proj.ai[2]];
            Item heldItem = player.HeldItem;

            float num15 = (ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * swingTime * player.whipRangeMultiplier;
            float num16 = 8 * num15 * num13 * rangeMultiplier / segments;

            Vector2 playerArmPosition = ownerProjectile.type == ModContent.ProjectileType<FlagellatorHandle>() 
                ? ownerProjectile.Center + proj.velocity * 16
                : ownerProjectile.Center + new Vector2(ownerProjectile.direction == -1 ? -10 : 8, 10);

            Vector2 vector = playerArmPosition;
            float num2 = -(float)Math.PI / 2f;
            Vector2 vector2 = vector;
            float num3 = (float)Math.PI / 2f + (float)Math.PI / 2f * proj.spriteDirection;
            Vector2 vector3 = vector;
            float num4 = (float)Math.PI / 2f;
            controlPoints.Add(playerArmPosition);

            for (int i = 0; i < segments; i++)
            {
                float num5 = (float)i / segments;
                float num6 = num12 * num5;
                Vector2 vector4 = vector + num2.ToRotationVector2() * num16;
                Vector2 vector5 = vector3 + num4.ToRotationVector2() * (num16 * 2f);
                Vector2 val = vector2 + num3.ToRotationVector2() * (num16 * 2f);
                float num7 = 1f - num13;
                float num8 = 1f - num7 * num7;
                Vector2 value = Vector2.Lerp(vector5, vector4, num8 * 0.9f + 0.1f);
                Vector2 vector6 = Vector2.Lerp(val, value, num8 * 0.7f + 0.3f);
                Vector2 spinPoint = playerArmPosition + (vector6 - playerArmPosition) * new Vector2(1f, num11);
                float num9 = num14;
                num9 *= num9;
                Vector2 item = spinPoint.RotatedBy(proj.rotation + 4.712389f * num9 * proj.spriteDirection, playerArmPosition);
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
            Projectile.WhipSettings.Segments = 16;
            Projectile.WhipSettings.RangeMultiplier = 1.2f;
        }

        public override bool PreAI()
        {
            Projectile.position = Main.projectile[(int)ProjectileOwner].Center;

            if (!Main.projectile[(int)ProjectileOwner].active)
            {
                Projectile.Kill();
                return false;
            }

            return base.PreAI();
        }

        public override void PostAI()
        {
            Projectile.position = Main.projectile[(int)ProjectileOwner].Center;
            base.PostAI();
        }

        public override bool PreDraw(ref Color l) => WhipCommon.Draw(Projectile, Timer, new(0, 0, 10, 26), new(74, 18), new(58, 16), new(42, 16), new(26, 16), Projectile.Opacity);
    }
}
