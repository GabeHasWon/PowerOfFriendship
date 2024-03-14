using System.IO;
using Terraria.ModLoader.IO;

namespace PoF.Content.Items.Accessories;

public class Stargem : ModItem
{
    const int ManaStarChance = 4;

    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(44, 32);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(silver: 8);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<StargemPlayer>().equipped = true;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.FallenStar)
            .AddTile(TileID.Anvils)
            .Register();
    }

    class StargemPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (equipped && target.life < 0 && Main.rand.NextBool(ManaStarChance))
                Item.NewItem(Player.GetSource_OnHit(target), target.Hitbox, ItemID.Star);
        }
    }

    class StargemProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        bool addedStarChance = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            addedStarChance = false;

            if (source is EntitySource_Parent { Entity: Player plr } && plr.GetModPlayer<StargemPlayer>().equipped)
                addedStarChance = true;
            else if (source is EntitySource_Parent { Entity: Projectile proj } && proj.GetGlobalProjectile<StargemProjectile>().addedStarChance)
                addedStarChance = true;
            else if (projectile.IsMinionOrSentryRelated)
                addedStarChance = true;

            if (addedStarChance)
                projectile.netUpdate = true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (addedStarChance && target.life < 0 && Main.rand.NextBool(ManaStarChance))
                Item.NewItem(projectile.GetSource_OnHit(target), target.Hitbox, ItemID.Star);
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) => bitWriter.WriteBit(addedStarChance);
        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) => addedStarChance = bitReader.ReadBit();
    }
}