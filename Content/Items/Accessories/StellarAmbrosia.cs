using PoF.Common.Globals.ProjectileGlobals;
using PoF.Common.Syncing;
using PoF.Content.Items.Talismans;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace PoF.Content.Items.Accessories;

public class StellarAmbrosia : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.Size = new(28, 40);
        Item.rare = ItemRarityID.Red;
        Item.value = Item.sellPrice(gold: 5);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<StellarPlayer>().equipped = true;
        player.GetCritChance(DamageClass.SummonMeleeSpeed) += 0.15f;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.FragmentStardust, 15)
        .AddTile(TileID.LunarCraftingStation)
        .Register();

    public class StellarPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects() => equipped = false;
    }

    public class StellarProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.DamageType.CountsAsClass<TalismanDamageClass>() && projectile.friendly && projectile.TryGetOwner(out var owner) &&
                owner.GetModPlayer<StellarPlayer>().equipped && !TalismanGlobal.IsMinorTalismanProjectile.Contains(projectile.type))
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    target.GetGlobalNPC<StellarNPC>().ApplyCell(target, projectile, damageDone);
                else
                    new ApplyAmbrosiaCellModule(target.whoAmI, projectile.identity, damageDone).Send(-1, -1, false);
            }
        }
    }

    public class StellarNPC : GlobalNPC
    {
        private class Cell(Vector2 off, int damage, int maxTime, NPC host)
        {
            public readonly Vector2 Offset = off;
            public readonly int Damage = damage;
            public readonly NPC Host = host;
            public readonly int MaxTime = maxTime;

            public float size = 0f;
            public int _time = maxTime;
            public float _accumulatedDamage = 0;
            public float _accDoT = 0;
            public int _fourthSecondTimer = 0;

            public void Update()
            {
                _time--;

                if (_time > 30f)
                    size = MathHelper.Lerp(size, 1f, 0.05f);
                else
                    size = _time / 30f;

                float rate = Damage / (float)MaxTime;
                _accumulatedDamage += rate;
                _accDoT += rate;

                int hitDamage = 0;

                while (_accumulatedDamage > 1)
                {
                    hitDamage++;
                    _accumulatedDamage--;
                }

                if (hitDamage > 0)
                {
                    Host.life -= hitDamage;
                    Host.checkDead();
                }

                if (_fourthSecondTimer++ > 30 && _accDoT >= 1)
                {
                    CombatText.NewText(Host.Hitbox, Colors.RarityBlue, (int)_accDoT);
                    _fourthSecondTimer = 0;
                    _accDoT = 0;
                }
            }
        }

        public override bool InstancePerEntity => true;

        private readonly List<Cell> _cells = [];

        internal void ApplyCell(NPC npc, Projectile proj, int dmg) 
        {
            Vector2 off = npc.DirectionTo(proj).RotatedBy(npc.rotation) * Math.Min(npc.width, npc.height) * Main.rand.NextFloat() * 0.5f;
            _cells.Add(new Cell(off, dmg / 2, 240, npc));
        }

        public override bool PreAI(NPC npc)
        {
            foreach (var item in _cells)
                item.Update();

            _cells.RemoveAll(x => x._time < 0);

            return true;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Main.instance.LoadProjectile(ProjectileID.StardustCellMinionShot);
            Texture2D tex = TextureAssets.Projectile[ProjectileID.StardustCellMinionShot].Value;
            int ind = 0;

            foreach (var item in _cells)
            {
                var frame = tex.Frame(1, 4, 0, (int)((Main.GameUpdateCount / 4f + ind) % 4), 0, 0);
                var pos = npc.Center - item.Offset.RotatedBy(npc.rotation) - screenPos;
                spriteBatch.Draw(tex, pos, frame, drawColor, npc.rotation, frame.Size() / 2f, item.size, SpriteEffects.None, 0);
            }
        }
    }
}