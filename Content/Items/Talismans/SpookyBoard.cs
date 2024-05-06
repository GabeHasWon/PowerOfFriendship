using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Localization;

using static PoF.Content.Items.Talismans.Flagellator;

namespace PoF.Content.Items.Talismans;

internal class SpookyBoard : Talisman
{
    protected override float TileRange => 70;

    protected override void Defaults()
    {
        Item.Size = new(54, 22);
        Item.rare = ItemRarityID.Yellow;
        Item.damage = 92;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.mana = 7;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<SpookySummoner>();
        Item.shootSpeed = 5;
        Item.value = Item.buyPrice(0, 0, 30);
        Item.noMelee = true;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.SpookyWood, 100)
        .AddIngredient(ItemID.Smolstar)
        .AddIngredient<Flagellator>()
        .AddTile(TileID.WorkBenches)
        .Register();

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

        private bool _spawnedDaggers = false;
        private int _casualTime = 0;

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

            _casualTime++;

            if (_casualTime > 600)
            {
                AdvancedPopupRequest request = default;
                request.Text = Language.GetTextValue("Mods.PoF.Items.SpookyBoard.Dialogue." + Main.rand.Next(4));
                request.DurationInFrames = 240;
                request.Color = Color.Lerp(new Color(119, 92, 138), Color.White, 0.3f);
                PopupText.NewText(request, Projectile.Center);
                
                _casualTime = 0;
            }

            if (!_spawnedDaggers)
            {
                _spawnedDaggers = true;

                int type = ModContent.ProjectileType<DaggerCopy>();
                var src = Projectile.GetSource_FromAI();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        var pos = Projectile.Center;
                        int proj = Projectile.NewProjectile(src, pos, Vector2.Zero, type, Projectile.damage / 10, 0, Projectile.owner, 0, 0, Projectile.whoAmI);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    }
                }

                _casualTime = 0;
            }

            if (!Despawning)
            {
                bool invalidWhip = !Whip.active || Whip.type != ModContent.ProjectileType<FlagellatorWhip>();

                if (invalidWhip && Projectile.GetNearestNPCTarget(out NPC npc))
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        var src = Projectile.GetSource_FromAI();
                        int type = ModContent.ProjectileType<FlagellatorWhip>();
                        var vel = Projectile.DirectionTo(npc.Center) * 1.2f;
                        var pos = Projectile.Center + vel;

                        WhipWhoAmI = Projectile.NewProjectile(src, pos, vel, type, Projectile.damage, 0, Projectile.owner, ai2: Projectile.whoAmI);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, (int)WhipWhoAmI);
                    }

                    if (!PayMana(Projectile))
                        Despawning = true;

                    _casualTime = 0;
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

                    Projectile.direction = Math.Sign(Whip.velocity.X);
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

                bool stop = HandleBasicFunctions<SpookyBoard>(Projectile, ref Time, 0.45f, false);

                if (!Despawning && stop)
                    Despawning = true;
            }
            else
            {
                Projectile.velocity *= 0.92f;

                if (Projectile.velocity.LengthSquared() < 0.2f * 0.2f)
                {
                    Projectile.Kill();

                    for (int i = 0; i < 20; ++i)
                    {
                        int dust = Main.rand.Next(4) switch
                        {
                            0 => DustID.Bone,
                            1 => DustID.Pumpkin,
                            _ => DustID.SpookyWood
                        };

                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dust);
                    }

                    SoundEngine.PlaySound(SoundID.NPCHit2, Projectile.Center);
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effect = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            var src = new Rectangle(0, 0, 26, 34);
            var drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(tex, drawPos, src, lightColor, Projectile.rotation, src.Size() / new Vector2(2, 1), 1f, effect, 0);

            src.X = Projectile.frame * 28;
            src.Y = 34;
            Main.EntitySpriteDraw(tex, drawPos, src, lightColor, Projectile.rotation, src.Size() * new Vector2(0.5f, 0), 1f, effect, 0);
            return false;
        }
    }

    public class DaggerCopy : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Smolstar}";

        private Projectile Parent => Main.projectile[(int)OwnerProjectileWhoAmI];

        private ref float State => ref Projectile.ai[0];
        private ref float TargetOrTimer => ref Projectile.ai[1];
        private ref float OwnerProjectileWhoAmI => ref Projectile.ai[2];

        private float swingTime = 0;

        public override void SetStaticDefaults() => Main.projFrames[Type] = 2;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Smolstar);
            Projectile.minionSlots = 0;
            AIType = ProjectileID.Smolstar;
        }

        public override bool PreAI()
        {
            Vector2 parentTop = Parent.Top + new Vector2(0f, -30f);

            if (!Parent.active || Parent.type != ModContent.ProjectileType<SpookySummoner>())
            {
                Projectile.Kill();
                return false;
            }

            Projectile.timeLeft = 2;

            if (State == 0f)
            {
                GetMinionSlots(out int index, out int totalIndexesInGroup);
                float rotationBase = (float)Math.PI * 2f / totalIndexesInGroup;
                float rotationOffset = totalIndexesInGroup * 0.66f;
                Vector2 vector2 = new Vector2(30f, 6f) / 5f * (totalIndexesInGroup - 1);
                Vector2 rotation = Vector2.UnitY.RotatedBy(rotationBase * index + Main.GlobalTimeWrappedHourly % rotationOffset / rotationOffset * ((float)Math.PI * 2f));
                parentTop += rotation * vector2;
                parentTop.Y += Parent.gfxOffY;
                parentTop = parentTop.Floor();
            }

            if (State == 0f)
            {
                Vector2 directionToParent = parentTop - Projectile.Center;
                float num4 = 10f;
                float lerpValue = Utils.GetLerpValue(200f, 600f, directionToParent.Length(), clamped: true);
                num4 += lerpValue * 30f;

                if (directionToParent.Length() >= 3000f)
                    Projectile.Center = parentTop;

                Projectile.velocity = directionToParent;

                if (Projectile.velocity.Length() > num4)
                    Projectile.velocity *= num4 / Projectile.velocity.Length();

                int startAttackRange = 800;
                int attackTarget = -1;

                Projectile.Minion_FindTargetInRange(startAttackRange, ref attackTarget, skipIfCannotHitWithOwnBody: false);

                if (attackTarget != -1)
                {
                    State = 60f;
                    TargetOrTimer = attackTarget;
                    Projectile.netUpdate = true;
                }

                float targetAngle = Projectile.velocity.SafeNormalize(Vector2.UnitY).ToRotation() + (float)Math.PI / 2f;
                if (directionToParent.Length() < 40f)
                    targetAngle = Vector2.UnitY.ToRotation() + (float)Math.PI / 2f;
                Projectile.rotation = Projectile.rotation.AngleLerp(targetAngle, 0.2f);
                return false;
            }

            if (State == -1f)
            {
                if (TargetOrTimer == 0f)
                {
                    SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                    for (int i = 0; i < 2; i++)
                    {
                        var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror,
                            Projectile.oldVelocity.X * 0.2f, Projectile.oldVelocity.Y * 0.2f, 0, default, 1.4f);

                        if (!Main.rand.NextBool(3))
                        {
                            dust.scale *= 1.3f;
                            dust.velocity *= 1.1f;
                        }

                        dust.noGravity = true;
                        dust.fadeIn = 0f;
                    }

                    Projectile.velocity += Main.rand.NextVector2CircularEdge(4f, 4f);
                }

                TargetOrTimer += 1f;
                Projectile.rotation += Projectile.velocity.X * 0.1f + Projectile.velocity.Y * 0.05f;
                Projectile.velocity *= 0.92f;

                if (TargetOrTimer >= 9f)
                {
                    Projectile.ai[0] = 0f;
                    TargetOrTimer = 0f;
                }

                return false;
            }

            NPC target = null;
            int targetIndex = (int)TargetOrTimer;

            if (Main.npc.IndexInRange(targetIndex) && Main.npc[targetIndex].CanBeChasedBy(this))
                target = Main.npc[targetIndex];

            if (target == null || Parent.Distance(target.Center) >= 900f)
            {
                State = target == null ? -1 : 0f;
                TargetOrTimer = 0f;
                Projectile.netUpdate = true;
            }
            else
            {
                if (swingTime > 0)
                {
                    swingTime--;

                    Projectile.rotation += Projectile.velocity.X * 0.1f + Projectile.velocity.Y * 0.05f;
                    Projectile.velocity *= 0.92f;
                }
                else
                {
                    const float MaxSpeed = 16f;

                    Vector2 direction = target.Center - Projectile.Center;

                    Projectile.velocity = direction;

                    if (Projectile.velocity.Length() > MaxSpeed)
                        Projectile.velocity *= MaxSpeed / Projectile.velocity.Length();

                    float angle = Projectile.velocity.SafeNormalize(Vector2.UnitY).ToRotation() + (float)Math.PI / 2f;
                    Projectile.rotation = Projectile.rotation.AngleLerp(angle, 0.4f);

                    if (direction.LengthSquared() < 200 * 200)
                        swingTime = 12;
                }
            }

            ProjectileInteraction();

            return false;
        }

        private void ProjectileInteraction()
        {
            const float VelOffset = 0.1f;
            float minDistance = Projectile.width * 5;

            for (int j = 0; j < Main.maxProjectiles; j++)
            {
                Projectile p = Main.projectile[j];

                if (j == Projectile.whoAmI || !p.active || p.owner != Projectile.owner)
                    continue;

                if (p.type == Projectile.type && Math.Abs(Projectile.position.X - p.position.X) + Math.Abs(Projectile.position.Y - p.position.Y) < minDistance)
                {
                    if (Projectile.position.X < p.position.X)
                        Projectile.velocity.X -= VelOffset;
                    else
                        Projectile.velocity.X += VelOffset;
                    if (Projectile.position.Y < p.position.Y)
                        Projectile.velocity.Y -= VelOffset;
                    else
                        Projectile.velocity.Y += VelOffset;
                }
            }
        }

        private void GetMinionSlots(out int index, out int totalIndexesInGroup)
        {
            index = 0;
            totalIndexesInGroup = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type)
                {
                    if (Projectile.whoAmI > i)
                        index++;
                    totalIndexesInGroup++;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;
            var pos = Projectile.Center - Main.screenPosition;
            var origin = tex.Size() / new Vector2(2, 4f);

            Color glowColor = Projectile.GetFloatingDaggerMinionGlowColor();
            glowColor.A = (byte)(glowColor.A / 4);

            for (int i = 0; i < 4; i++)
            {
                Vector2 basePos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 spinVector = Projectile.rotation.ToRotationVector2();
                double rotation = (float)Math.PI / 2f * i;
                var drawPos = basePos + spinVector.RotatedBy(rotation) * 2f;

                Main.EntitySpriteDraw(tex, drawPos, tex.Frame(1, 2, 0, 1), glowColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.spriteBatch.Draw(tex, pos, tex.Frame(1, 2, 0, Projectile.frame), Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);

            return false;
        }
    }
}
