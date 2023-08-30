using DS2S_META.Utils;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class CharCreation
    {
        // Fields:
        internal static List<ItemRow> allItems;
        internal static List<ItemRow> consumables;
        internal static List<ItemRow> rings;
        internal static List<ItemRow> weapons;
        internal static List<ItemRow> heads;
        internal static List<ItemRow> bodies;
        internal static List<ItemRow> arms;
        internal static List<ItemRow> legs;
        internal static List<ItemRow> arrows;
        internal static List<ItemRow> bolts;
        internal static List<PlayerStatusClassRow> classrows;
        internal static List<PlayerStatusClassRow> giftsrows;

        internal static List<ITEMID> bannedItems = new() { ITEMID.ESTUS, ITEMID.ESTUSEMPTY,
                                             ITEMID.DARKSIGN, ITEMID.BONEOFORDER,
                                             ITEMID.BLACKSEPCRYSTAL };
        internal static List<int> classids = new() { 20, 30, 50, 70, 80, 90, 100, 110 }; // Warrior --> Deprived

        static CharCreation()
        {
            // Define lists that can be used for CharCreation random draws
            allItems = ParamMan.ItemRows.Values.Where(it => it.HasName).ToList().FilterOutId(bannedItems).ToList();
            //
            consumables = allItems.FilterByType(eItemType.CONSUMABLE)
                                    .FilterOutUsage(ITEMUSAGE.ITEMUSAGEKEY, ITEMUSAGE.BOSSSOULUSAGE).ToList();
            //
            rings = allItems.FilterByType(eItemType.RING).ToList();
            weapons = allItems.FilterByType(eItemType.WEAPON1, eItemType.WEAPON2).ToList();
            heads = allItems.FilterByType(eItemType.HEADARMOUR).ToList();
            bodies = allItems.FilterByType(eItemType.CHESTARMOUR).ToList();
            arms = allItems.FilterByType(eItemType.GAUNTLETS).ToList();
            legs = allItems.FilterByType(eItemType.LEGARMOUR).ToList();
            //
            var ammo = allItems.FilterByType(eItemType.AMMO);
            arrows = ammo.Where(it => it.ArrowRow?.AmmunitionType == (int)ArrowRow.AmmoType.ARROW).ToList();
            bolts = ammo.Where(it => it.ArrowRow?.AmmunitionType == (int)ArrowRow.AmmoType.BOLT).ToList();
            //
            classrows = ParamMan.PlayerStatusClassRows.Where(row => classids.Contains(row.ID)).ToList();
            giftsrows = ParamMan.PlayerStatusClassRows.Where(row => row.ID > 400 & row.ID < 1000).ToList();
        }

        internal static void Randomize()
        {
            RandomizeCharClasses();
            RandomizeStartingGifts();
        }
        internal static void RandomizeCharClasses()
        {
            // Randomize DS2 classes
            foreach (var classrow in classrows)
                RandomizeCharClass(classrow);
        }
        internal static void RandomizeCharClass(PlayerStatusClassRow classrow)
        {
            // Draw from sets:
            var randRings = DrawUntilFailure(rings, 15, 4, ir => ir.IconID); // Rings 15% (exp), note +1 rings wouldn't display any icon
            var randRHweps = DrawIdsUntilFailure(weapons, 40, 3);            // RHand Weapons 40% (exp)
            var randLHweps = DrawIdsUntilFailure(weapons, 40, 3);            // LHand Weapons 40% (exp)
            var randConsumItems = DrawItemsUntilLimit(consumables, 25, 4);   // Class Items 25% (flat)
            var randArrows = DrawIdsUntilLimit(arrows, 20, 2);               // Arrows 20% (flat)
            var randBolts = DrawIdsUntilLimit(bolts, 20, 2);                 // Bolts 20% (flat)
            var randHead = DrawSingleParamId(heads, 50);                     // Head armour 50% (single)
            var randBody = DrawSingleParamId(bodies, 50);                    // Body armour 50% (single)
            var randArms = DrawSingleParamId(arms, 50);                      // Arms armour 50% (single)
            var randLegs = DrawSingleParamId(legs, 50);                      // Legs armour 50% (single)
             
            // Draw supplementary info: 
            var randRHreinf = DrawWepReinforces(randRHweps.Count());
            var randLHreinf = DrawWepReinforces(randLHweps.Count());
            var arrowQuants = DrawAmmoQuants(randArrows.Count());
            var boltQuants = DrawAmmoQuants(randBolts.Count());
            var consumQuants = DrawItemQuants(randConsumItems);
            var consumIds = randConsumItems.Select(ir => ir.ItemID);

            // Update param classrow
            classrow.Wipe(); // clear defaults
            classrow.WriteRings(randRings);
            classrow.WriteRHWeps(randRHweps, randRHreinf);
            classrow.WriteLHWeps(randLHweps, randLHreinf);
            classrow.WriteItems(consumIds, consumQuants);
            classrow.WriteArrows(randArrows, arrowQuants);
            classrow.WriteBolts(randBolts, boltQuants);
            if (randHead != null) classrow.HeadArmor = (int)randHead;
            if (randBody != null) classrow.BodyArmor = (int)randBody;
            if (randArms != null) classrow.ArmsArmor = (int)randArms;
            if (randLegs != null) classrow.LegsArmor = (int)randLegs;
            //
            RandomizeLevels(classrow);
            classrow.WriteRow(); // Commit all changes to game memory
        }
        internal static void RandomizeLevels(PlayerStatusClassRow classrow)
        {
            // Update Class Levels:
            classrow.Vigor = DrawRandomLevel();
            classrow.Endurance = DrawRandomLevel();
            classrow.Attunement = DrawRandomLevel();
            classrow.Vitality = DrawRandomLevel();
            classrow.Strength = DrawRandomLevel();
            classrow.Dexterity = DrawRandomLevel();
            classrow.Intelligence = DrawRandomLevel();
            classrow.Faith = DrawRandomLevel();
            classrow.Adaptability = DrawRandomLevel();
            classrow.SetSoulLevel();

            if (classrow.SoulLevel <= 0)
            {
                var diff_to_fix = Math.Abs(classrow.SoulLevel);
                // add it to vgr for now as testing:
                classrow.Vigor += (short)(diff_to_fix + 1);
                classrow.SetSoulLevel();
            }
        }
        internal static void RandomizeStartingGifts()
        {
            foreach (var giftrow in giftsrows)
                RandomizeStartingGift(giftrow);
        }
        internal static void RandomizeStartingGift(PlayerStatusClassRow giftrow)
        {
            // Draw items
            var randItems = DrawItemsUntilLimit(consumables, 50, 5); // Items 50% (flat)
            
            // Supplementary info:
            var itemIds = randItems.Select(ir => ir.ItemID).ToList();
            var itemQuants = DrawItemQuants(randItems);

            // Commit all changes to memory
            giftrow.Wipe(); // clear defaults
            giftrow.WriteItems(itemIds, itemQuants);
            giftrow.WriteRow();
        }

        // Draw ItemRow helpers:
        internal static List<T> DrawUntilFailure<T>(List<ItemRow> pool, int pcpass, int nmax, Func<ItemRow, T> outmap)
        {
            // Most generic version: applies outbound transform after draws
            return DrawItemsUntilFailure(pool, pcpass, nmax).Select(outmap).ToList();
        }
        internal static List<int> DrawIdsUntilFailure(List<ItemRow> pool, int percentpass, int maxdraws)
        {
            return DrawUntilFailure(pool, percentpass, maxdraws, ir => ir.ItemID).ToList();
        }
        internal static List<ItemRow> DrawItemsUntilFailure(List<ItemRow> pool, int percentpass, int maxdraws)
        {
            List<ItemRow> randdraws = new();

            // Continuing drawing objects if you pass the percentcheck until you reach maxdraws
            for (int i = 0; i < maxdraws; i++)
            {
                // failure:
                if (Rng.FailedPercentRoll(percentpass))
                    break;

                // success: draw and continue
                randdraws.Add(pool.RandomElement());
            };
            return randdraws;
        }
        internal static List<int> DrawIdsUntilLimit(List<ItemRow> pool, int percentpass, int maxdraws)
        {
            return DrawItemsUntilLimit(pool, percentpass, maxdraws).Select(ir => ir.ItemID).ToList();
        }
        internal static int? DrawSingleParamId(List<ItemRow> pool, int percentpass)
        {
            // used for armour memes (todo?)
            return DrawItemsUntilLimit(pool, percentpass, 1).Select(ir => ir.ID).FirstOrDefault();
        }
        internal static List<ItemRow> DrawItemsUntilLimit(List<ItemRow> pool, int percentpass, int maxdraws)
        {
            // more lenient form of DrawItemsUntilFailure. Flat x% chance on every draw, no early exit.
            List<ItemRow> randdraws = new();

            // Continuing drawing objects if you pass the percentcheck until you reach maxdraws
            for (int i = 0; i < maxdraws; i++)
            {
                // failure:
                if (Rng.FailedPercentRoll(percentpass))
                    continue;

                // success: draw and continue
                randdraws.Add(pool.RandomElement());
            };
            return randdraws;
        }

        // Rng helpers:
        internal static List<int> DrawWepReinforces(int n) => Util.CollateCalls(DrawWepReinforce, n).ToList();
        internal static List<short> DrawAmmoQuants(int n) => Util.CollateCalls(DrawAmmoQuant, n).ToList();
        internal static List<short> DrawItemQuants(List<ItemRow> cons) => cons.Select(ir => DrawItemQuant(ir)).ToList();
        //
        internal static int DrawWepReinforce()
        {
            var tmp = Rng.NextPercent();
            if (tmp < 60) return 0;
            if (tmp < 90) return 1;
            if (tmp < 95) return 2;
            if (tmp < 99) return 3;
            return 4;
        }
        internal static short DrawAmmoQuant() => (short)Rng.Next(30);
        internal static short DrawItemQuant(ItemRow item)
        {
            if (item.ItemUsageID == (int)ITEMUSAGE.SOULUSAGE)
                return 1;
            return (short)(1 + Rng.Next(4));
        }
        internal static short DrawRandomLevel()
        {
            int lvlmean = 7;
            //var randlvl = (short)RandomGammaInt(lvlmean, 1);
            var randlvl = (short)Rng.RandomGaussianInt(lvlmean, 3, 1);
            return (short)(randlvl <= 0 ? 1 : randlvl);
        }
    }
}
