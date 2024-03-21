namespace PoF.Content.Items.Whips;

public class AncientSnare : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<AncientSnareProj>(), 120, 6, 4, 40);
        Item.rare = ItemRarityID.LightPurple;
        Item.value = Item.buyPrice(0, 3);
    }

    public override bool MeleePrefix() => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            velocity *= Main.rand.NextFloat(1.8f, 2.4f);
            velocity = velocity.RotatedByRandom(0.1f);
            int proj = Projectile.NewProjectile(source, position, velocity, ProjectileID.SpikyBallTrap, damage / 3 * 2, 0.3f, player.whoAmI);
            Main.projectile[proj].hostile = false;
            Main.projectile[proj].friendly = true;
            Main.projectile[proj].timeLeft = 180;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
        }
        
        return true;
    }

    public class AncientSnareProj : ModProjectile
    {
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults() => ProjectileID.Sets.IsAWhip[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 38;
            Projectile.WhipSettings.RangeMultiplier = 2.2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            Projectile.damage = (int)(Projectile.damage * 0.6f);
        }

        public override bool PreDraw(ref Color light) => WhipCommon.Draw(Projectile, Timer, new(0, 0, 14, 26), new(80, 18), new(62, 18), new(44, 18), new(26, 18));
    }
}