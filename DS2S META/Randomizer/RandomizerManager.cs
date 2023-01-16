using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using DS2S_META.Utils;
using System.CodeDom;
using System.Threading;
using SoulsFormats;
using static SoulsFormats.MSBD;
using System.Transactions;
using System.Windows.Controls;
using Octokit;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Handle logic & data related to Randomizer
    /// </summary>
    internal class RandomizerManager
    {
        // Fields:
        DS2SHook? Hook;
        List<Randomization> AllP = new();   // All Places (including those to fill_by_copy)
        List<Randomization> AllPTR = new(); // Places to Randomize
        internal static Random RNG = new();
        private List<ItemLotRow> VanillaLots = new();
        private List<ItemLotRow> VanillaDrops = new();
        private List<ShopRow> VanillaShops = new();
        internal List<ShopRow> ShopsToFillByCopying = new();
        internal ItemSetBase Logic = new CasualItemSet();
        internal List<DropInfo> LTR_flatlist = new();
        internal bool IsInitialized = false;
        internal bool IsRandomized = false;
        // 
        internal List<DropInfo> ldkeys = new();
        internal List<DropInfo> ldreqs = new();
        internal List<DropInfo> ldgens = new();
        //
        private List<int> Unfilled = new();
        private List<int> KeysPlacedSoFar = new(); // to tidy
        internal int CurrSeed;
        //
        internal static Dictionary<int, ItemRow> VanillaItemParams = new();
        internal static string GetItemName(int itemid) => VanillaItemParams[itemid].MetaItemName;
        internal static bool TryGetItemName(int itemid, out string name)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            name = found ? GetItemName(itemid) : "";
            return found;
        }
        internal static bool TryGetItem(int itemid, out ItemRow? item)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            item = found ? VanillaItemParams[itemid] : null;
            return found;
        }
        internal int GetItemMaxUpgrade(ItemRow item)
        {
            if (Hook == null)
                return 0;

            // Wrapper similar to the DS2Item class call in Hook.
            int? upgr;
            switch (item.ItemType)
            {
                case eItemType.WEAPON1: // & shields
                case eItemType.WEAPON2: // & staves
                    upgr = item.WeaponRow?.MaxUpgrade;
                    return upgr ?? 0;
                case eItemType.HEADARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                case eItemType.LEGARMOUR:
                    upgr = item.ArmorRow?.ArmorReinforceRow?.MaxReinforceLevel;
                    return upgr ?? 0;

                default:
                    return 0;
            }
        }
        //
        
        // Enums:
        internal enum SetType : byte { Keys, Reqs, Gens }


        // Constructors:
        internal RandomizerManager()
        {
            RNG = new Random(Environment.TickCount); // used for generate seed numbers
        }

        // Main Methods:
        internal void Initalize(DS2SHook hook)
        {
            Hook = hook; // Required for reading game params in memory

            // Param collecting:
            Logic = new CasualItemSet();
            GetVanillaLots();
            GetVanillaDrops();
            GetVanillaShops();
            VanillaItemParams = Hook.Items.ToDictionary(it => it.ID, it => it);
            
            SetupAllPTF();
            FixLogicDictionary();
            GetLootToRandomize(); // set LTR_Flatlist field
            IsInitialized = true;
        }
        internal List<NamedItemLot?> GetActiveDropTables()
        {
            // Distinct enemies with non-zero drops used in the maps
            List<NamedItemLot?> genplist = new();
            foreach (var genparam in ParamMan.GeneratorParams)
            {
                var prows = genparam.Rows.OfType<GeneratorParamRow>();
                var prows_zero = prows.Where(row => row.ItemLotID == 0).ToList();
                var indirect_lots = prows_zero.Select(prow => prow.GeneratorRegist?.Enemy?.ItemLot).ToList();

                var prows_nonzero = prows.Where(row => row.ItemLotID != 0).ToList();
                var direct_lots = prows_nonzero.Select(prow => prow.ItemLot).ToList();
                
                // Store info:
                var indirect_nil = prows_zero.Select(row => 
                                new NamedItemLot(row.Name, 
                                                 row.GeneratorRegist?.Enemy?.ItemLot,
                                                 row.GeneratorRegist?.Enemy?.ItemLotID));
                var direct_nil = prows_nonzero.Select(row => new NamedItemLot(row.Name, row.ItemLot, row.ItemLotID));
                genplist.AddRange(indirect_nil);
                genplist.AddRange(direct_nil);
            }

            // Add associated NG plus tables as they might not be linked directly:
            List<NamedItemLot?>ngpList = new();
            foreach(var nil in genplist)
            {
                int? ilotID = nil?.RawID;
                if (ilotID == null) throw new Exception("Shouldn't be possible");

                // Look for ng1,2 variants:
                for (int j = 1; j < 3; j++)
                {
                    int ngplusxID = (int)ilotID + j;
                    var tryng = ParamMan.ItemLotChrParam?.Rows.FirstOrDefault(row => row.ID == ngplusxID);
                    if (tryng == null)
                        break; // no associated ngplus tables
                    ItemLotRow? ngxItemLot = ParamMan.GetLink<ItemLotRow>(ParamMan.ItemLotChrParam, ngplusxID);
                    string ngname = $"{nil?.Name} in NG+{j}";

                    // add table to list of interest:
                    var ngnil = new NamedItemLot(ngname, ngxItemLot, ngplusxID);
                    ngpList.Add(ngnil);
                }
            }
            genplist.AddRange(ngpList);

            // Querying:
            //var test = genplist.Where(nil => nil?.Lot != null && nil.Lot.HasItem(0x001312D0)).ToList();

            // Get unique & interesting droptables
            var uniques = genplist.GroupBy(nil => nil?.Lot?.ID).Select(grp => grp.First()).ToList();
            return uniques.Where(nil => nil?.Lot?.IsEmpty != null && nil.Lot?.IsEmpty != true).ToList();
        }
        internal class NamedItemLot
        {
            internal int? RawID; // for awkward situations with ngplus drops but no NG drops
            internal int ID => Lot?.ID ?? -1;
            internal string Name;
            internal ItemLotRow? Lot;
            internal NamedItemLot(string name, ItemLotRow? lot, int? rawid)
            {
                Name = name;
                Lot = lot;
                RawID = rawid;
            }
        }

        internal void GetVanillaDrops()
        {
            var vanlotschr = ParamMan.ItemLotChrParam?.Rows.OfType<ItemLotRow>().ToList();
            if (vanlotschr == null) throw new Exception("Null table");
            foreach (var droprow in vanlotschr) droprow.IsDropTable = true;
            VanillaDrops = vanlotschr;

            AddDropsToLogic(); // gotta do it somewhere
            return;
        }
        internal void GetVanillaLots()
        {
            var vanlotsother = ParamMan.ItemLotOtherParam?.Rows.OfType<ItemLotRow>().ToList();
            if (vanlotsother == null) throw new NullReferenceException("Shouldn't get here");
            VanillaLots = vanlotsother;

            // Add descriptions
            foreach (var ilot in VanillaLots)
                ilot.ParamDesc = Logic.GetDesc(ilot.ID);
        }
        internal void GetVanillaShops()
        {
            var vanshops = ParamMan.ShopLineupParam?.Rows.OfType<ShopRow>().ToList();
            if (vanshops == null)
                throw new NullReferenceException("Shouldn't get here");
            VanillaShops = vanshops;

            AddShopsToLogic();
            return;
        }

        internal async Task Randomize(int seed)
        {
            if (Hook == null)
                return;

            // Setup for re-randomization:
            SetSeed(seed);      // reset Rng Twister
            ResetForRerandomization();

            //var test = AllPTR.OfType<DropRdz>().ToList();

            // Place sets of items:
            PlaceSet(ldkeys, SetType.Keys);
            PlaceSet(ldreqs, SetType.Reqs);
            PlaceSet(ldgens, SetType.Gens);
            FillLeftovers();

            // Miscellaneous things to handle:
            HandleTrivialities();   // Simply mark done
            FixShopEvents();        // All additional shop processing & edge cases.
            FixLotCopies();         // Aka Pursuer
            RandomizeStartingClasses();

            // Ensure all handled:
            var unhandled = AllP.Where(rdz => rdz.IsHandled == false);
            if (unhandled.Any())
                throw new Exception("Unhandled things remain.");

            // Printout the current shuffled lots:
            PrintKeysNeat();
            PrintAllRdz();

            // Randomize Game!
            await Task.Run(() => WriteShuffledLots());
            await Task.Run(() => WriteShuffledDrops());
            await Task.Run(() => WriteShuffledShops());
            ParamMan.ItemParam?.WriteModifiedParam(); // can speed this up if we have to

            Hook.WarpLast();    // Force an area reload. TODO add warning:
            IsRandomized = true;
        }
        internal void Unrandomize()
        {
            if (ParamMan.ShopLineupParam == null || ParamMan.ItemLotOtherParam == null || ParamMan.ItemParam == null)
                throw new Exception("Param tables are null");

            // Restore all the param tables we changed:
            ParamMan.ShopLineupParam?.RestoreParam();
            ParamMan.ItemLotOtherParam?.RestoreParam();
            ParamMan.ItemParam?.RestoreParam();
            ParamMan.PlayerStatusClassParam?.RestoreParam();
            ParamMan.ItemLotChrParam?.RestoreParam();

            // Force an area reload.
            Hook?.WarpLast();
            IsRandomized = false;
        }
        internal static int GetRandom()
        {
            return RNG.Next();
        }


        // Core Logic
        internal void RandomizeStartingClasses()
        {
            int BOSSSOULUSAGE = 2000;
            int ITEMUSAGEKEY = 2700;
            int SOULUSAGE = 1900;

            var classids = new List<int>() { 20, 30, 50, 70, 80, 90, 100, 110 }; // Warrior --> Deprived
            var classrows = ParamMan.PlayerStatusClassParam?.Rows.Where(row => classids.Contains(row.ID))
                                                            .OfType<PlayerStatusClassRow>();
            if (classrows == null) throw new Exception("Failed to find classes in param");
            
            // Setup lists to draw from randomly:
            var all_items = ParamMan.ItemParam?.Rows.OfType<ItemRow>();
            var all_consumables = all_items?.Where(it => it.ItemType == eItemType.CONSUMABLE)
                                            .Where(it => it.ItemUsageID != ITEMUSAGEKEY && it.ItemUsageID != BOSSSOULUSAGE)
                                            .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_consumables == null) throw new Exception("Items not loaded from Param table");
            //
            var all_rings = all_items?.Where(it => it.ItemType == eItemType.RING)
                                       .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_rings == null) throw new Exception("Rings not loaded correctly from param");
            //
            var all_weapons = all_items?.Where(it => it.ItemType == eItemType.WEAPON1 || it.ItemType == eItemType.WEAPON2)
                                        .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_weapons == null) throw new Exception("Weapons not loaded correctly from ItemParam");
            //
            var all_arrows = all_items?.Where(it => it.ItemType == eItemType.AMMO)
                                        .Where(it => it.ArrowRow?.AmmunitionType == (int)ArrowRow.AmmoType.ARROW).ToList();
            if (all_arrows == null) throw new Exception("Arrows not loaded correctly from ItemParam");
            //
            var all_bolts = all_items?.Where(it => it.ItemType == eItemType.AMMO)
                                        .Where(it => it.ArrowRow?.AmmunitionType == (int)ArrowRow.AmmoType.BOLT).ToList();
            if (all_bolts == null) throw new Exception("Bolts not loaded correctly from ItemParam");
            //
            var all_head = all_items?.Where(it => it.ItemType == eItemType.HEADARMOUR)
                                        .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_head == null) throw new Exception("Head armour not loaded correctly from ItemParam");
            var all_body = all_items?.Where(it => it.ItemType == eItemType.CHESTARMOUR)
                                        .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_body == null) throw new Exception("Body armour not loaded correctly from ItemParam");
            var all_arms = all_items?.Where(it => it.ItemType == eItemType.GAUNTLETS)
                                        .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_arms == null) throw new Exception("Arms armour not loaded correctly from ItemParam");
            var all_legs = all_items?.Where(it => it.ItemType == eItemType.LEGARMOUR)
                                        .Where(it => it.MetaItemName != String.Empty).ToList();
            if (all_legs == null) throw new Exception("Legs armour not loaded correctly from ItemParam");

            // Main randomizing loop for each class
            foreach (var classrow in classrows)
            {
                // Delete the defaults:
                classrow.Wipe();

                // Class Items:
                var numItems = RandomGammaInt(2, 1);
                numItems = Math.Min(numItems, 7);

                for (int i= 0; i < numItems; i++)
                {
                    var randitem = all_consumables[RNG.Next(all_consumables.Count)];
                    classrow.WriteAtItemArray(i, randitem.ItemID);
                    if (randitem.ItemUsageID == SOULUSAGE)
                        classrow.WriteAtItemQuantArray(i, 1);
                    else
                    {
                        var quant = RNG.Next(5);
                        if (quant == 0)
                            quant = 1;
                        classrow.WriteAtItemQuantArray(i, (short)quant);
                    }
                        
                }

                // Class Rings 15% chance:
                int ringnum = 0;
                while (RNG.Next(100) > 85 && ringnum < 4)
                {
                    var randring = all_rings[RNG.Next(all_rings.Count)];
                    classrow.WriteAtRingArray(ringnum, randring.IconID); // if you allow +1 rings etc you don't get an icon!
                    ringnum++;
                };

                // Class Right-hand weapons 40% chance:
                int rhwepnum = 0;
                while (RNG.Next(100) < 40 && rhwepnum < 3)
                {
                    var randwep = all_weapons[RNG.Next(all_weapons.Count)];
                    classrow.WriteAtRHWepArray(rhwepnum, randwep.ItemID);
                    classrow.WriteAtRHWepReinforceArray(rhwepnum, GetRandomReinforce());
                    rhwepnum++;
                };
                
                // Class Left-hand weapons 40% chance:
                int lhwepnum = 0;
                while (RNG.Next(100) < 40 && lhwepnum < 3)
                {
                    var randwep = all_weapons[RNG.Next(all_weapons.Count)];
                    classrow.WriteAtLHWepArray(lhwepnum, randwep.ItemID);
                    classrow.WriteAtLHWepReinforceArray(lhwepnum, GetRandomReinforce());
                    lhwepnum++;
                };

                // Class Arrows 20% chance:
                int arrownum = 0;
                while (RNG.Next(100) < 20 && lhwepnum < 2)
                {
                    var randarrow = all_arrows[RNG.Next(all_arrows.Count)];
                    classrow.WriteAtArrowArray(arrownum, randarrow.ItemID);
                    classrow.WriteAtArrowAmountArray(arrownum, (short)RNG.Next(30));
                    arrownum++;
                };

                // Class Bolts 20% chance:
                int boltnum = 0;
                while (RNG.Next(100) < 20 && boltnum < 2)
                {
                    var randbolt = all_bolts[RNG.Next(all_bolts.Count)];
                    classrow.WriteAtBoltArray(boltnum, randbolt.ItemID);
                    classrow.WriteAtBoltAmountArray(boltnum, (short)RNG.Next(30));
                    boltnum++;
                };

                // Class Armour: each piece 50% chance
                if (RNG.Next(100) < 50)
                    classrow.HeadArmour = all_head[RNG.Next(all_head.Count)].ID;
                if (RNG.Next(100) < 50)
                    classrow.BodyArmour = all_body[RNG.Next(all_body.Count)].ID;
                if (RNG.Next(100) < 50)
                    classrow.HandsArmour = all_arms[RNG.Next(all_arms.Count)].ID;
                if (RNG.Next(100) < 50)
                    classrow.LegsArmour = all_legs[RNG.Next(all_legs.Count)].ID;

                // Class Levels:
                classrow.Vigor = GetRandomLevel();
                classrow.Endurance = GetRandomLevel();
                classrow.Attunement = GetRandomLevel();
                classrow.Vitality = GetRandomLevel();
                classrow.Strength = GetRandomLevel();
                classrow.Dexterity = GetRandomLevel();
                classrow.Intelligence = GetRandomLevel();
                classrow.Faith = GetRandomLevel();
                classrow.Adaptability = GetRandomLevel();
                classrow.SetSoulLevel();

                if (classrow.SoulLevel <= 0)
                {
                    var diff_to_fix = Math.Abs(classrow.SoulLevel);
                    // add it to vgr for now as testing:
                    classrow.Vigor += (short)(diff_to_fix + 1);
                    classrow.SetSoulLevel();
                }
                
                // Commit all changes to memory
                classrow.WriteRow();
            }

        }
        internal void GetLootToRandomize()
        {
            // Start with AllP
            // Remove Shops that aren't NormalType --> add to loot
            // Remove Lots of specified types: Crammed/Crows/etc --> add to loot
            // Collapse all loot into flatlist for randomization

            List<List<DropInfo>> droplists = new();
            List<List<DropInfo>> shoplotlists = new();

            // Only keep ones of interest
            var stage1 = AllPTR.Where(rdz => !rdz.HasType(Logic.BanFromLoot));

            // Keep all drops that are added to AllPTR
            var okDrops = stage1.OfType<DropRdz>();
            foreach (var drop in okDrops)
                droplists.Add(drop.Flatlist);

            // Collapse droplists and sort out uniqueness balacing:
            var LTR_flatlist_dropsonly = droplists.SelectMany(di => di).ToList();
            List<DropInfo> drop_flatlist_balanced = new();
            foreach(var di in LTR_flatlist_dropsonly)
            {
                if (!TryGetItem(di.ItemID, out var item))
                    continue;
                if (item == null) continue;

                // Keep consumables/armour as fine:
                if (!item.NeedsMadeUnique)
                {
                    drop_flatlist_balanced.Add(di);
                    continue;
                }

                // Otherwise, only add weapon if it isn't in there already:
                bool isAlreadyDone = drop_flatlist_balanced.FirstOrDefault(di2 => di2.ItemID == di.ItemID) != null;
                if (!isAlreadyDone)
                    drop_flatlist_balanced.Add(di);
            }
            


            // testing
            //var LTR_flatlist_test0 = droplists.SelectMany(di => di).ToList();
            //var test1 = LTR_flatlist_test0.Where(di => di.ItemID == 0x001312D0).ToList();
            //var test = 1;

            // Only keep loot of shops that I'll be replacing (others are duplicates)
            var okShops = stage1.OfType<ShopRdz>()
                                .Where(srdz => srdz.Status == RDZ_STATUS.STANDARD 
                                        || srdz.Status == RDZ_STATUS.MAKEFREE
                                        || srdz.Status == RDZ_STATUS.UNLOCKTRADE).ToList();
            foreach (var shop in okShops)
                shoplotlists.Add(shop.Flatlist);

            // Normal Lots:
            var stage1_lots = stage1.OfType<LotRdz>();
            var normal_lots = stage1_lots.Where(lrdz => !lrdz.HasType(new List<PICKUPTYPE>() { PICKUPTYPE.NGPLUS }));
            foreach (var lot in normal_lots)
                shoplotlists.Add(lot.Flatlist);

            
            // Special Lots (NGplus things):
            var ngplus_lots = stage1_lots.Where(lrdz => lrdz.HasType(new List<PICKUPTYPE>() { PICKUPTYPE.NGPLUS }));
            List<int>? manualNGplusIDs = Logic.LinkedNGs.Select(lng => lng.ngplusID).ToList();
            int linkedorigID;
            
            foreach (var lrdz in ngplus_lots)
            {
                if (!manualNGplusIDs.Contains(lrdz.UniqueParamID))
                {
                    // Type 1 (99% of cases)
                    linkedorigID = lrdz.UniqueParamID / 10 * 10; // Round down to nearest 10
                } else
                {
                    // Type 2 (currently only applies to Fume Knight)
                    var link = Logic.LinkedNGs.FirstOrDefault(lng => lng.ngplusID == lrdz.UniqueParamID);
                    if (link == null)
                        throw new Exception("Shouldn't get here");
                    linkedorigID = link.origID;
                }

                // Get items of "non-ngplus":
                var linkedLRDZ = stage1_lots.FirstOrDefault(lrdz => lrdz.UniqueParamID == linkedorigID);
                if (linkedLRDZ == null)
                    throw new Exception("I don't think we can have NGPlus without an associated default to find.");

                // Add unique items:
                var ufl = lrdz.GetUniqueFlatlist(linkedLRDZ.Flatlist);
                shoplotlists.Add(ufl);
            }

            
            // Collapse all droplists into one droplist:
            var LTR_flatlist_lotshops = shoplotlists.SelectMany(di => di).ToList();
            LTR_flatlist = LTR_flatlist_lotshops.Concat(drop_flatlist_balanced).ToList();

            // query testing
            var test = LTR_flatlist.Where(di => di.ItemID == 40310000).ToList();
            var test2 = AllP.Where(rdz => rdz.HasVannilaItemID(40310000)).ToList();

            // Final Manual/Miscellaneous fixes
            FixFlatList(); // ensure correct number of keys etc
        }
        
        private void ResetForRerandomization()
        {
            // Reset required arrays for the randomizer to work:
            
            // Empty the shuffled places in preparation:
            foreach (var rdz in AllP)
                rdz.ResetShuffled();
            
            KeysPlacedSoFar = new List<int>();
            Unfilled = Enumerable.Range(0, AllPTR.Count).ToList();
            
            // Remake (copies of) list of Keys, Required, Generics for placement
            DefineKRG();
        }
        private void HandleTrivialities()
        {
            foreach (var rdz in AllP.Where(rdz => rdz.Status == RDZ_STATUS.EXCLUDED))
                rdz.MarkHandled();

            // TODO!
            foreach (var rdz in AllP.Where(rdz => rdz.Status == RDZ_STATUS.CROWS))
                rdz.MarkHandled();
        }
        private void FixShopEvents()
        {
            FixShopCopies();
            FixNormalTrade();
            FixShopTradeCopies();
            FixFreeTrade(); // needs to be after FixShopTradeCopies()
            FixShopsToRemove();
        }

        internal Randomization GetRdzWithID(IEnumerable<Randomization> rdzlist, int id)
        {
            var res = rdzlist.FirstOrDefault(rdz => rdz.UniqueParamID == id);
            if (res == null)
                throw new Exception($"Cannot find Randomization object with id {id}");
            return res;
        }
        internal void FixLogicDictionary()
        {
            AddDropsToLogic(); // gotta do it somewhere
        }
        internal void SetupAllPTF()
        {
            // "Places To Fill"
            var lotptfs = SetLotPTFTypes();
            var shopptfs = SetShopPTFTypes();
            var dropptfs = SetDropPTFTypes();

            // All items to handle:
            AllP = lotptfs.ToList()
                    .Concat(shopptfs).ToList()
                    .Concat(dropptfs).ToList();

            // Places to fill with "Ordinary Randomization"
            AllPTR = AllP.Where(rdz => rdz.Status == RDZ_STATUS.STANDARD ||
                                       rdz.Status == RDZ_STATUS.MAKEFREE ||
                                       rdz.Status == RDZ_STATUS.UNLOCKTRADE).ToList();
        }
        internal IEnumerable<Randomization> SetLotPTFTypes()
        {
            // Get copy of all VanillaLots
            IEnumerable<GLotRdz> all_lots = VanillaLots.Select(lot => new LotRdz(lot)).ToList(); // LotsToFill
            Logic.TransformToUID(all_lots.Cast<Randomization>().ToList());     // FixLogic: PT1, Transform lots

            // Define exclusions (not placed)
            //var excl = all_lots.Where(ldz => ldz.IsEmpty ||
            //                            Logic.HasTypes(ldz, Logic.BanFromBeingRandomized) ||
            //                            Logic.CrowDuplicates.Contains(ldz.UniqueParamID));
            var excl = all_lots.Where(ldz => ldz.IsEmpty ||
                                        ldz.HasType(Logic.BanFromBeingRandomized) ||
                                        Logic.CrowDuplicates.Contains(ldz.UniqueParamID));

            foreach (var ldz in excl)
                ldz.Status = RDZ_STATUS.EXCLUDED;

            // Special Cases: Crows
            var crows = all_lots.Where(ldz => ldz.HasType(new List<PICKUPTYPE>() { PICKUPTYPE.CROWS })).ToList();
            foreach (var ldz in crows)
                ldz.Status = RDZ_STATUS.CROWS;


            // Special Cases: LinkedLots
            var slavelots = all_lots.Where(ldz => ldz.HasType(new List<PICKUPTYPE>() { PICKUPTYPE.LINKEDSLAVE })).ToList();
            foreach (var ldz in slavelots)
                ldz.Status = RDZ_STATUS.FILL_BY_COPY;
            
            //foreach (var kvp in Logic.WhereHasType(PICKUPTYPE.LINKEDSLAVE))
            //{
            //    var slavelot = all_lots.FirstOrDefault(ldz => ldz.UniqueParamID == kvp.Key);
            //    if (slavelot == null) throw new Exception("LinkedSlave Lot not found in Vanilla table definition");
            //    slavelot.Status = RDZ_STATUS.FILL_BY_COPY;
            //}

            // All the other shops should be good for "Ordinary Randomization"
            var normal_lots = all_lots.Where(rdz => rdz.Status == RDZ_STATUS.INITIALIZING);
            foreach (var rdz in normal_lots)
                rdz.Status = RDZ_STATUS.STANDARD;

            // Output
            return all_lots.Cast<Randomization>();
        }
        internal IEnumerable<Randomization> SetDropPTFTypes()
        {
            // Get copy of all VanillaLots
            IEnumerable<GLotRdz> all_drops = VanillaDrops.Select(lot => new DropRdz(lot)).ToList(); // DropsToFill

            // Get only interesting drops:
            var ADT = GetActiveDropTables();
            var ADTIDs = ADT.Select(nil => nil?.Lot?.ID).ToList();

            // Define exclusions (not placed)
            var excl = all_drops.Where(ldz => !ADTIDs.Contains(ldz.ParamID)).ToList();
            foreach (var ldz in excl)
                ldz.Status = RDZ_STATUS.EXCLUDED;

            // For everything else, add a string description and set as ready:
            var normal_lots = all_drops.Where(rdz => rdz.Status == RDZ_STATUS.INITIALIZING).ToList();
            foreach (var ldz in normal_lots)
            {
                // get associated NIL:
                var assoc_nil = ADT.FirstOrDefault(nil => nil?.ID == ldz.ParamID);
                if (assoc_nil == null) throw new Exception("Pretty sure we shouldn't get here");
                ldz.RandoDesc = assoc_nil.Name;
                ldz.Status = RDZ_STATUS.STANDARD;
            }
            
            // Output
            return all_drops.Cast<Randomization>();
        }
        internal IEnumerable<Randomization> SetShopPTFTypes()
        {
            // Function to assign how to handle each of the defined
            // shop params later into the randomizer process

            // Setup all shops as randomization:
            var shoprdzs = VanillaShops.Select(SR => new ShopRdz(SR)).ToList(); // ToList IS required
            Logic.TransformToUID(shoprdzs.Cast<Randomization>().ToList());      // FixLogic PT2: Transform shops

            // Remove exclusions from list
            foreach (var exclid in ShopRules.Exclusions)
                GetRdzWithID(shoprdzs, exclid).Status = RDZ_STATUS.EXCLUDED;

            // Define shops that need handling:
            var LEvents = ShopRules.GetLinkedEvents();

            // -------------------------------------------- //
            // "Find shops with IDs that match (the ID of) non-trade,copy LEvents; return the shops"
            var LE_normal_copies = LEvents.Where(lev => lev.IsCopy && !lev.IsTrade);
            var shopnormcopies = shoprdzs.Join(inner: LE_normal_copies,
                                               outerKeySelector: srdz => srdz.UniqueParamID,
                                               innerKeySelector: LEc => LEc.FillByCopy,
                                               resultSelector: (srdz,lec) => srdz);
            foreach (var srdz in shopnormcopies)
                srdz.Status = RDZ_STATUS.FILL_BY_COPY;

            // -------------------------------------------- //
            // Deal with things that need removing:
            var remIDs = LEvents.SelectMany(selector: LE => LE.RemoveIDs ?? new List<int>());
            var shopsrem = shoprdzs.Join(inner: remIDs,
                                    outerKeySelector: srdz => srdz.UniqueParamID,
                                    innerKeySelector: remid => remid,
                                    resultSelector: (srdz, remid) => srdz);
            foreach (var srdz in shopsrem)
                srdz.Status = RDZ_STATUS.SHOPREMOVE;

            // -------------------------------------------- //
            // Trade shops:

            // "Ordinary TradeShop" has nothing special to handle (aka Straid)
            var normtrades = LEvents.Where(lev => lev.IsTrade && !lev.IsCopy);
            var shopsnt = shoprdzs.Join(inner: normtrades,
                                         outerKeySelector: srdz => srdz.UniqueParamID,
                                         innerKeySelector: lev => lev.KeepID,
                                         resultSelector: (srdz, lev) => srdz);
            foreach (var srdz in shopsnt)
                srdz.Status = RDZ_STATUS.UNLOCKTRADE;

            // Ornifex FreeTrades need a flag setting to zeroise pricerate after creation
            // Note, these IDs still go through the "usual" randomization process first!
            var ft = LEvents.Where(lev => lev.FreeTrade);
            var shopsft = shoprdzs.Join(inner: ft,
                                         outerKeySelector: srdz => srdz.UniqueParamID,
                                         innerKeySelector: lev => lev.KeepID,
                                         resultSelector: (srdz, lev) => srdz);
            foreach (var srdz in shopsft)
                srdz.Status = RDZ_STATUS.MAKEFREE;

            // The TradeShopCopy ones DO NOT go through "ordinary randomization"
            // they instead, just copy the above ones, and then change price
            var tsc = LEvents.Where(lev => lev.IsTrade && lev.IsCopy);
            var shopstsc = shoprdzs.Join(inner: tsc,
                                         outerKeySelector: srdz => srdz.UniqueParamID,
                                         innerKeySelector: lev => lev.FillByCopy,
                                         resultSelector: (srdz, lev) => srdz);

            foreach (var srdz in shopstsc)
                srdz.Status = RDZ_STATUS.TRADE_SHOP_COPY;

            // All the other shops should be good for "Ordinary Randomization"
            var normal_shops = shoprdzs.Where(rdz => rdz.Status == RDZ_STATUS.INITIALIZING);
            foreach (var rdz in normal_shops)
                rdz.Status = RDZ_STATUS.STANDARD;

            // Output
            return shoprdzs.Cast<Randomization>();
        }

        internal void AddShopsToLogic()
        {
            foreach (var sr in VanillaShops)
            {
                // Append:
                RandoInfo RI = new(sr.ParamDesc, PICKUPTYPE.SHOP);
                Logic.AppendKvp(new KeyValuePair<int, RandoInfo>(sr.ID, RI));
            }
        }
        internal void AddDropsToLogic()
        {
            var active_drops = AllP.OfType<DropRdz>();
            foreach (var droprdz in active_drops)
            {
                // Append:
                RandoInfo RI;
                if (droprdz.IsGuaranteedDrop)
                    RI = new(droprdz.RandoDesc, PICKUPTYPE.GUARANTEEDENEMYDROP);
                else
                    RI = new(droprdz.RandoDesc, PICKUPTYPE.ENEMYDROP);

                Logic.D[droprdz.GUID] = RI; // Add to dictionary
                droprdz.RandoInfo = RI;     // save to randomization
            }
        }
        internal void DefineKRG()
        {
            // Take a copy of the FlatList so we don't need to regenerate everytime:
            var flatlist_copy = LTR_flatlist.Select(di => di.Clone()).ToList();

            // Partition into KeyTypes, ReqNonKeys and Generic Loot-To-Randomize:
            var too_many_torches = flatlist_copy.Where(DI => DI.IsKeyType).ToList();                  // Keys
            ldkeys = RemoveExtraTorches(too_many_torches);

            var flatlist_nokeys = flatlist_copy.Where(DI => !DI.IsKeyType).ToList();    // (keys handled above)
            ldreqs = flatlist_nokeys.Where(DI => DI.IsReqType).ToList();                // Reqs

            var flatlist_noreqs = flatlist_nokeys.Where(DI => !DI.IsReqType).ToList();  // (reqs handled above)
            ldgens = flatlist_noreqs.Except(ldkeys).Except(ldreqs).ToList();            // Generics

            // Ensure no meme double placements:
            //if (ldkeys.Any(di => ldreqs.Contains(di)))
            //    throw new Exception("Add a query to remove duplicates here!");
        }
        internal void FixFlatList()
        {
            // Ensure 5 SoaGs (game defines these weirdly)
            var soag = LTR_flatlist.Where(di => di.ItemID == (int)KEYID.SOULOFAGIANT).First();
            LTR_flatlist.Add(soag);
            LTR_flatlist.Add(soag);

            // Duplicate Fixes:
            RemoveFirstIfPresent(0x03088510); // Rotunda Lockstone
            RemoveFirstIfPresent(0x0398F1B8); // Feather
            RemoveFirstIfPresent(0x03096F70); // Ladder Miniature
            RemoveFirstIfPresent(0x0308D330); // Ashen Mist
            RemoveFirstIfPresent(0x03B39220); // Token of Fidelity
            RemoveFirstIfPresent(0x03B3B930); // Token of Spite
            LimitNumberOfItem(0x0399EFA0, 15); // 15xX Torch pickups in game
            LimitNumberOfItem(0x0395D4D8, 30); // 30 Effigy pickups in game
        }
        private List<DropInfo> RemoveExtraTorches(List<DropInfo> too_many_torches)
        {
            int min_torches = 6;
            int torch_keys_placed = 0;
            List<DropInfo> ldkeys_out = new();
            for (int i = 0; i < too_many_torches.Count; i++)
            {
                var di = too_many_torches[i];
                if (di.ItemID == 0x0399EFA0)
                {
                    if (torch_keys_placed >= min_torches)
                        continue;
                    torch_keys_placed += di.Quantity;
                }
                ldkeys_out.Add(di); // ok to keep as key
            }
            return ldkeys_out;
        }
        private void RemoveFirstIfPresent(int itemid)
        {
            var di = LTR_flatlist.Where(di => di.ItemID == itemid).FirstOrDefault();
            if (di == null)
                return;
            LTR_flatlist.Remove(di); 
        }
        private void LimitNumberOfItem(int itemid, int maxlim)
        {
            List<DropInfo> torem = new();
            int remain_cnt = LTR_flatlist.Where(DI => DI.HasItem(itemid)).Count();
            for (int i = 0; i < LTR_flatlist.Count; i++)
            {
                var di = LTR_flatlist[i];
                if (di.HasItem(itemid))
                {
                    torem.Add(di);
                    remain_cnt--;
                }

                if (remain_cnt == maxlim)
                    break;
            }
            LTR_flatlist.RemoveAll(torem.Contains);
        }

        // Placement code:
        internal static Dictionary<SetType, List<PICKUPTYPE>> BannedTypeList = new()
        {
            {SetType.Keys, ItemSetBase.BanKeyTypes},
            {SetType.Gens, ItemSetBase.BanGeneralTypes}
        };
        internal void PlaceSet(List<DropInfo> ld, SetType flag)
        {
            // ld: list of DropInfos
            while (ld.Count > 0)
            {
                if (Unfilled.Count == 0)
                    break;

                int keyindex = RNG.Next(ld.Count);
                DropInfo di = ld[keyindex]; // get item to place
                PlaceItem(di, flag);
                ld.RemoveAt(keyindex);
            }

            // Must have ran out of space to place things:
            if (ld.Count > 0 && flag != SetType.Gens) 
                throw new Exception("Ran out of space to place keys/reqs. Likely querying issue.");
        }
        private void PlaceItem(DropInfo di, SetType stype)
        {
            var localunfilled = new List<int>(Unfilled); // local clone of spots available for placement
            while (localunfilled.Count > 0)
            {
                // Choose random rdz for item:
                int pindex = RNG.Next(localunfilled.Count);
                int elnum = localunfilled[pindex];
                var rdz = AllPTR.ElementAt(elnum);

                // Check pickup type conditions:
                switch (stype)
                {
                    case SetType.Keys:
                    case SetType.Gens:
                        if (rdz.HasType(BannedTypeList[stype]))
                        {
                            localunfilled.RemoveAt(pindex);
                            continue;
                        }
                        break;
                    case SetType.Reqs:
                        // Now extra rules for specific stuff:

                        // (handled separately, below)
                        if (ItemSetBase.ManuallyRequiredItemsTypeRules.ContainsKey(di.ItemID))
                            break; 


                        // Get allowable placements by item type:
                        var item = ParamMan.GetItemFromID(di.ItemID);
                        if (item == null)
                        {
                            localunfilled.RemoveAt(pindex);
                            continue;
                        }
                        if (!rdz.ContainsOnlyTypes(ItemSetBase.ItemAllowTypes[item.ItemType]))
                        {
                            localunfilled.RemoveAt(pindex);
                            continue;
                        }
                        break;
                }

                // Now extra rules for specific stuff:
                if (ItemSetBase.ManuallyRequiredItemsTypeRules.TryGetValue(di.ItemID, out var mantypes))
                {
                    if (!rdz.ContainsOnlyTypes(mantypes))
                    {
                        localunfilled.RemoveAt(pindex);
                        continue;
                    }
                }
                

                // Check key-softlock conditions:
                if (stype == SetType.Keys && rdz.IsSoftlockPlacement(KeysPlacedSoFar))
                {
                    localunfilled.RemoveAt(pindex);
                    continue;
                }


                // Accept solution:
                if (stype == SetType.Keys)
                    KeysPlacedSoFar.Add(di.ItemID);

                // This is preliminary code if you want to randomize reinforcement/infusion
                FixReinforcement(di);
                FixInfusion(di);

                rdz.AddShuffledItem(di);
                if (rdz.IsSaturated())
                {
                    rdz.MarkHandled();
                    Unfilled.Remove(elnum); // now filled!
                }
                return;
            }
            throw new Exception("True Softlock, please investigate");
        }
        private void FillLeftovers()
        {
            // ld: list of DropInfos
            int Nfc = ItemSetBase.FillerItems.Count; // fill count
            foreach (var rdz in AllPTR)
            {
                while (!rdz.IsSaturated())
                {
                    int ind = RNG.Next(Nfc);
                    DropInfo item = ItemSetBase.FillerItems[ind]; // get filler item
                    rdz.AddShuffledItem(item);
                }
                rdz.MarkHandled();
            }
        }
        
        private void FixInfusion(DropInfo di)
        {
            var item = ParamMan.GetItemFromID(di.ItemID);
            if (item == null) return;

            switch (item.ItemType)
            {
                case eItemType.WEAPON1:
                case eItemType.WEAPON2:
                    var infusionOptions = item.GetInfusionList();
                    if (!infusionOptions.Any(ds2I => ds2I.ID == di.Infusion))
                        di.Infusion = 0; // Don't allow a "new" infusion
                    return;

                default:
                    di.Infusion = 0; // uninfusable
                    return;
            }
        }
        private void FixReinforcement(DropInfo di)
        {
            if (!TryGetItem(di.ItemID, out ItemRow? item))
                return;
            if (item == null)
                return;
            var maxupgrade = GetItemMaxUpgrade(item);
            di.Reinforcement = (byte)Math.Min(di.Reinforcement, maxupgrade); // limit to item max upgrade
        }

        // Memory modification:
        internal static void WriteAllLots(List<ItemLotRow> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotOtherParam?.WriteModifiedParam();
        }
        internal static void WriteAllDrops(List<ItemLotRow> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotChrParam?.WriteModifiedParam();
        }
        internal static void WriteSomeLots(List<ItemLotRow> somelots)
        {
            // Method used for just writing a few rows out of the Param
            somelots.ForEach(lot => lot.WriteRow());
        }
        internal static void WriteSomeShops(List<ShopRow> shops)
        {
            // Method used for just writing a few rows out of the Param
            shops.ForEach(SR => SR.WriteRow());
        }
        internal static void WriteAllShops(List<ShopRow> all_shops)
        {
            all_shops.ForEach(SR => SR.StoreRow());
            ParamMan.ShopLineupParam?.WriteModifiedParam();
        }
        internal void WriteShuffledLots()
        {
            if (Hook == null)
                return;

            var shuffledlots = AllP.OfType<LotRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledDrops()
        {
            if (Hook == null)
                return;

            var shuffleddrops = AllP.OfType<DropRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllDrops(shuffleddrops);
        }
        internal void WriteShuffledShops()
        {
            if (Hook == null)
                return;

            var shuffledshops = AllP.OfType<ShopRdz>().Select(sdz => sdz.ShuffledShop).ToList();
            WriteAllShops(shuffledshops);
        }
        
        // Utility:
        internal static Dictionary<int, string> ReadShopNames()
        {
            Dictionary<int, string> shopnames = new();

            // Read all:
            var lines = File.ReadAllLines("./Resources/Paramdex_DS2S_09272022/ShopLineupParam.txt");

            // Setup parser:
            Regex re = new(@"(?<paramid>\d+) (?<desc>.*)");
            foreach (var line in lines)
            {
                var match = re.Match(line);
                int paramid = int.Parse(match.Groups["paramid"].Value);
                string desc = match.Groups["desc"].Value;
                shopnames.Add(paramid, desc);
            }
            return shopnames;

        }
        internal void PrintKeysNeat()
        {
            // Prep:
            List<string> lines = new();

            // Intro line
            lines.Add($"Printing key locations for seed {CurrSeed}");
            lines.Add("---------------------------------------------");

            // Main print loop
            foreach (int keyid in ItemSetBase.KeyOutputOrder.Cast<int>())
            {
                if (!TryGetItemName(keyid, out string itemname))
                    continue;

                var rdzsWithKey = AllPTR.Where(rdz => rdz.HasShuffledItemID(keyid)).ToList();
                foreach (var rdz in rdzsWithKey)
                {
                    StringBuilder sb = new(itemname);
                    int quant = rdz.GetShuffledItemQuant(keyid);
                    if (quant != 1)
                        sb.Append($" x{quant}");

                    string? desc = rdz.RandoInfo?.Description;
                    sb.Append($": {desc}");
                    lines.Add(sb.ToString());
                }
            }

            // Write file:
            File.WriteAllLines("./key_placements.txt", lines.ToArray());
        }
        internal void PrintAllRdz()
        {
            // Prep:
            List<string> lines = new List<string>();

            // Intro line
            lines.Add($"Printing items at all locations for seed {CurrSeed}");
            lines.Add("---------------------------------------------");

            // World placements:
            lines.Add("World placement:");
            foreach (var ldz in AllPTR.OfType<LotRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Shops:
            lines.Add("");
            lines.Add("Shops:");
            foreach (var rdz in AllPTR.OfType<ShopRdz>())
                lines.Add(rdz.GetNeatDescription());

            // Enemy drops:
            lines.Add("");
            lines.Add("Enemy Drops:");
            foreach (var ldz in AllPTR.OfType<DropRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Write file:
            File.WriteAllLines("./all_answers.txt", lines.ToArray());
        }
        
        // Miscellaneous post-processing
        internal void FixShopCopies()
        {
            // Maughlin / Gilligan / Gavlan
            var fillbycopy = AllP.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Status == RDZ_STATUS.FILL_BY_COPY).ToList();
            var filled_shops = AllPTR.OfType<ShopRdz>();

            // Define shops that need handling:
            var LEvents = ShopRules.GetLinkedEvents();
            var shopcopies = LEvents.Where(lev => lev.IsCopy && !lev.IsTrade);

            foreach (var shp in fillbycopy)
            {
                var LE = shopcopies.FirstOrDefault(lev => lev.FillByCopy == shp.UniqueParamID);
                if (LE == null)
                    throw new Exception("Cannot find linked event");

                var shop_to_copy = filled_shops.Where(srdz => srdz.UniqueParamID == LE.CopyID).FirstOrDefault();
                if (shop_to_copy == null)
                    throw new Exception("Cannot find shop to copy from");

                // Fill by copy:
                shp.ShuffledShop.CopyCoreValuesFrom(shop_to_copy.ShuffledShop);
                shp.MarkHandled();
            }
        }
        internal void FixNormalTrade()
        {
            // Ornifex First Trade (ensure free)
            var normal_trades = AllPTR.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Status == RDZ_STATUS.UNLOCKTRADE).ToList();
            foreach (var shp in normal_trades)
            {
                shp.ShuffledShop.EnableFlag = -1;  // enable (show) immediately (except Ornifex "1" trades that are locked behind event)
                shp.ShuffledShop.DisableFlag = -1;
                shp.MarkHandled();
            }
        }
        internal void FixShopTradeCopies()
        {
            // Ornifex (non-free)
            var updateshops = new List<ShopRow>();

            var fillbycopy = AllP.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Status == RDZ_STATUS.TRADE_SHOP_COPY).ToList();
            var filled_shops = AllPTR.OfType<ShopRdz>();

            // Define shops that need handling:
            var LEvents = ShopRules.GetLinkedEvents();
            var tradecopies = LEvents.Where(lev => lev.IsCopy && lev.IsTrade);

            foreach (var shp in fillbycopy)
            {
                var LE = tradecopies.FirstOrDefault(lev => lev.FillByCopy == shp.UniqueParamID);
                if (LE == null)
                    throw new Exception("Cannot find linked event");

                var shop_to_copy = filled_shops.Where(srdz => srdz.UniqueParamID == LE.CopyID).FirstOrDefault();
                if (shop_to_copy == null)
                    throw new Exception("Cannot find shop to copy from");

                // Fill by copy:
                shp.ShuffledShop.CopyCoreValuesFrom(shop_to_copy.ShuffledShop);

                // They still won't show till after the event so this should work
                shp.ShuffledShop.EnableFlag = -1;
                shp.ShuffledShop.DisableFlag = -1;
                shp.MarkHandled();
            }
        }
        internal void FixFreeTrade()
        {
            // This is just a Normal Trade Fix but where we additionally 0 the price
            // Ornifex First Trade (ensure free)
            var shops_makefree = AllPTR.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Status == RDZ_STATUS.MAKEFREE);
            foreach (var shp in shops_makefree)
            {
                shp.ShuffledShop.EnableFlag = -1;  // enable (show) immediately (except Ornifex "1" trades that are locked behind event)
                shp.ShuffledShop.DisableFlag = -1;
                shp.ShuffledShop.PriceRate = 0;
                shp.MarkHandled();
            }
        }
        internal void FixShopsToRemove()
        {
            // Ornifex First Trade (ensure free)
            var shops_toremove = AllP.OfType<ShopRdz>()
                                       .Where(rdz => rdz.Status == RDZ_STATUS.SHOPREMOVE);
            foreach (var shp in shops_toremove)
            {
                shp.ZeroiseShuffledShop();
                shp.MarkHandled();
            }
        }
        internal void FixLotCopies()
        {
            // Maughlin / Gilligan / Gavlan
            var fillbycopy = AllP.OfType<LotRdz>()
                                 .Where(rdz => rdz.Status == RDZ_STATUS.FILL_BY_COPY);
            foreach (var lot in fillbycopy)
            {
                var LD = Logic.LinkedDrops.FirstOrDefault(ld => ld.SlaveIDs.Contains(lot.UniqueParamID));
                if (LD == null) 
                    throw new Exception("Cannot find LinkedDrop as expected");

                // Get Randomized ItemLot to copy from:
                var lot_to_copy = AllPTR.OfType<LotRdz>().Where(ldz => ldz.UniqueParamID == LD.MasterID).First();

                // Clone/Update:
                lot.ShuffledLot = lot.VanillaLot.CloneBlank();              // keep param reference for this ID
                lot.ShuffledLot.CloneValuesFrom(lot_to_copy.ShuffledLot);   // set to new values
                lot.MarkHandled();
            }
        }

        // RNG related:
        //private const double priceMeanGaussian = 3000;  // For Gaussian distribution
        //private const double priceSDGaussian = 500;     // For Gaussian distribution
        internal const double priceShapeK = 3.0;        // For Gamma distribution
        internal const double priceScaleTh = 2.0;       // For Gamma distribution
        internal void SetSeed(int seed)
        {
            CurrSeed = seed;
            RNG = new Random(seed);
        }
        internal int RandomGaussianInt(double mean, double stdDev, int roundfac = 50)
        {
            // Steal code from online :)
            double u1 = 1.0 - RNG.NextDouble(); // uniform(0,1] random doubles
            double u2 = 1.0 - RNG.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; // random normal(mean,stdDev^2)

            return RoundToFactorN(randNormal, roundfac);
        }
        internal static int RoundToFactorN(double val, int fac)
        {
            var nearestMultiple = Math.Round((val / fac), MidpointRounding.AwayFromZero) * fac;
            return (int)nearestMultiple;
        }
        internal static int RandomGammaInt(int wantedMean, int roundfac = 50, double scaleA = priceShapeK, double shapeTh = priceScaleTh)
        {
            // Wrapper to handle pre/post int manipulation for Gamma distribution
            double rvg = RandomGammaVariable(scaleA, shapeTh);

            double rvgmode = (scaleA - 1) * shapeTh; // gamma distribution property
            double rvgScaled = (rvg / rvgmode) * wantedMean;
            return RoundToFactorN(rvgScaled, roundfac);
        }
        internal static double RandomGammaVariable(double shapeA, double scaleTh)
        {
            // https://www.cs.toronto.edu/~radford/csc2541.F04/gamma.html
            // Can code up a more efficient version if you want to go through the maths

            double scaleB = 1 / scaleTh; // Align notation
            int Na = (int)Math.Floor(shapeA);
            List<double> RVu = new(); // RandomVariables Uniform(0,1] distribution
            List<double> RVe = new(); // RandomVariables Exponential(1) distribution
            for (int i = 0; i < Na; i++)
            {
                double ui = RNG.NextDouble();
                double Li = -Math.Log(1 - ui);

                // Store results:
                RVu.Add(ui);
                RVe.Add(Li);
            }

            double S = RVe.Sum();
            double RVgamma = S / scaleB;
            return RVgamma;
        }
        internal int GetRandomReinforce()
        {
            var tmp = RNG.Next(100);
            if (tmp < 60) return 0;
            if (tmp < 90) return 1;
            if (tmp < 95) return 2;
            if (tmp < 99) return 3;
            return 4;
        }
        internal short GetRandomLevel()
        {
            int lvlmean = 7;
            //var randlvl = (short)RandomGammaInt(lvlmean, 1);
            var randlvl = (short)RandomGaussianInt(lvlmean, 3, 1);
            return (short)(randlvl <= 0 ? 1 : randlvl);
        }
    }
}
