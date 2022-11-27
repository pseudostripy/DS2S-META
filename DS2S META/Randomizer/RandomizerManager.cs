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
        private List<ItemLot> VanillaLots = new();
        private List<ShopRow> VanillaShops = new();
        internal List<ShopRow> ShopsToFillByCopying = new();
        private List<ShopRow> FixedVanillaShops = new(); // Can probably tidy up by removing from vanshops
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
        private const int FREETRADEEVENT = 100100;
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
            switch (item.ItemType)
            {
                case eItemType.WEAPON1: // & shields
                case eItemType.WEAPON2: // & staves
                    var upgr = item.WeaponRow?.MaxUpgrade;
                    return upgr ?? 0;
                case eItemType.HEADARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                case eItemType.LEGARMOUR:
                    return Hook.GetArmorMaxUpgrade(item.ItemID + 10000000);

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
            GetVanillaShops();
            VanillaItemParams = Hook.Items.ToDictionary(it => it.ID, it => it);
            
            SetupAllPTF();
            GetLootToRandomize(); // set LTR_Flatlist field

            // TODO After?
            //FixShopEvents1(); // Update PTF with shop places


            IsInitialized = true;
        }
        internal void GetVanillaLots()
        {
            if (Hook?.ItemLotOtherParam == null)
                throw new NullReferenceException("Shouldn't get here");

            VanillaLots = Hook.ItemLotOtherParam.Rows.Select(row => new ItemLot(row)).ToList();
            
            // Add descriptions
            foreach (var ilot in VanillaLots)
                ilot.ParamDesc = Logic.GetDesc(ilot.ID);

            return;
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
            await Task.Run(() => WriteShuffledShops());
            
            Hook.WarpLast();    // Force an area reload. TODO add warning:
            IsRandomized = true;
        }
        internal void Unrandomize()
        {
            if (ParamMan.ShopLineupParam == null || Hook?.ItemLotOtherParam == null || ParamMan.ItemParam == null)
                throw new Exception("Param tables are null");

            // Restore all the param tables we changed:
            ParamMan.ShopLineupParam?.RestoreParam();
            Hook.ItemLotOtherParam?.RestoreParam();
            ParamMan.ItemParam?.RestoreParam();
            ParamMan.PlayerStatusClassParam?.RestoreParam();

            // Force an area reload.
            Hook.WarpLast();
            IsRandomized = false;
        }
        internal int GetRandom()
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

            // Completely remove these from consideration
            var stage1 = AllPTR.Where(rdz => Logic.D[rdz.ParamID].AvoidsTypes(Logic.BanFromLoot)); 

            // Only keep loot of shops that I'll be replacing (others are duplicates)
            var okShops = stage1.OfType<ShopRdz>()
                                .Where(srdz => srdz.Status == RDZ_STATUS.STANDARD 
                                        || srdz.Status == RDZ_STATUS.MAKEFREE
                                        || srdz.Status == RDZ_STATUS.UNLOCKTRADE);
            foreach (var shop in okShops)
                droplists.Add(shop.Flatlist);

            // Normal Lots:
            var stage1_lots = stage1.OfType<LotRdz>();
            var normal_lots = stage1_lots.Where(lrdz => Logic.D[lrdz.ParamID].AvoidsType(PICKUPTYPE.NGPLUS));
            foreach (var lot in normal_lots)
                droplists.Add(lot.Flatlist);


            // Special Lots (NGplus things):
            var ngplus_lots = stage1_lots.Where(lrdz => Logic.D[lrdz.ParamID].HasType(PICKUPTYPE.NGPLUS));
            List<int>? manualNGplusIDs = Logic.LinkedNGs.Select(lng => lng.ngplusID).ToList();
            int linkedorigID;
            
            foreach (var lrdz in ngplus_lots)
            {
                if (!manualNGplusIDs.Contains(lrdz.ParamID))
                {
                    // Type 1 (99% of cases)
                    linkedorigID = lrdz.ParamID / 10 * 10; // Round down to nearest 10
                } else
                {
                    // Type 2 (currently only applies to Fume Knight)
                    var link = Logic.LinkedNGs.FirstOrDefault(lng => lng.ngplusID == lrdz.ParamID);
                    if (link == null)
                        throw new Exception("Shouldn't get here");
                    linkedorigID = link.origID;
                }

                // Get items of "non-ngplus":
                var linkedLRDZ = stage1_lots.FirstOrDefault(lrdz => lrdz.ParamID == linkedorigID);
                if (linkedLRDZ == null)
                    throw new Exception("I don't think we can have NGPlus without an associated default to find.");

                // Add unique items:
                var ufl = lrdz.GetUniqueFlatlist(linkedLRDZ.Flatlist);
                droplists.Add(ufl);
            }


            // Collapse all droplists into one droplist:
            LTR_flatlist = droplists.SelectMany(di => di).ToList();

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
            var res = rdzlist.FirstOrDefault(rdz => rdz.ParamID == id);
            if (res == null)
                throw new Exception($"Cannot find Randomization object with id {id}");
            return res;
        }
        internal void SetupAllPTF()
        {
            // "Places To Fill"
            var lotptfs = SetLotPTFTypes();
            var shopptfs = SetShopPTFTypes();

            // All items to handle:
            AllP = lotptfs.ToList().Concat(shopptfs).ToList();

            // Places to fill with "Ordinary Randomization"
            AllPTR = AllP.Where(rdz => rdz.Status == RDZ_STATUS.STANDARD ||
                                       rdz.Status == RDZ_STATUS.MAKEFREE ||
                                       rdz.Status == RDZ_STATUS.UNLOCKTRADE).ToList();
        }
        internal IEnumerable<Randomization> SetLotPTFTypes()
        {
            // Get copy of all VanillaLots
            IEnumerable<LotRdz> all_lots = VanillaLots.Select(lot => new LotRdz(lot)).ToList(); // LotsToFill

            // Define exclusions (not placed)
            var excl = all_lots.Where(ldz => ldz.IsEmpty ||
                                        Logic.HasTypes(ldz, Logic.BanFromBeingRandomized) ||
                                        Logic.CrowDuplicates.Contains(ldz.ParamID));
            foreach (var ldz in excl)
                ldz.Status = RDZ_STATUS.EXCLUDED;

            // Special Cases: Crows
            var crows = all_lots.Where(ldz => Logic.HasType(ldz, PICKUPTYPE.CROWS));
            foreach (var ldz in crows)
                ldz.Status = RDZ_STATUS.CROWS;


            // Special Cases: LinkedLots
            foreach (var kvp in Logic.WhereHasType(PICKUPTYPE.LINKEDSLAVE))
            {
                var slavelot = all_lots.FirstOrDefault(ldz => ldz.ParamID == kvp.Key);
                if (slavelot == null)
                    throw new Exception("LinkedSlave Lot not found in Vanilla table definition");
                slavelot.Status = RDZ_STATUS.FILL_BY_COPY;
            }

            // All the other shops should be good for "Ordinary Randomization"
            var normal_lots = all_lots.Where(rdz => rdz.Status == RDZ_STATUS.INITIALIZING);
            foreach (var rdz in normal_lots)
                rdz.Status = RDZ_STATUS.STANDARD;

            // Output
            return all_lots.Cast<Randomization>();
        }
        internal IEnumerable<Randomization> SetShopPTFTypes()
        {
            // Function to assign how to handle each of the defined
            // shop params later into the randomizer process

            // Setup all shops as randomization:
            var shoprdzs = VanillaShops.Select(SR => new ShopRdz(SR)).ToList(); // ToList IS required

            // Remove exclusions from list
            foreach (var exclid in ShopRules.Exclusions)
                GetRdzWithID(shoprdzs, exclid).Status = RDZ_STATUS.EXCLUDED;

            // Define shops that need handling:
            var LEvents = ShopRules.GetLinkedEvents();

            // -------------------------------------------- //
            // "Find shops with IDs that match (the ID of) non-trade,copy LEvents; return the shops"
            var LE_normal_copies = LEvents.Where(lev => lev.IsCopy && !lev.IsTrade);
            var shopnormcopies = shoprdzs.Join(inner: LE_normal_copies,
                                               outerKeySelector: srdz => srdz.ParamID,
                                               innerKeySelector: LEc => LEc.FillByCopy,
                                               resultSelector: (srdz,lec) => srdz);
            foreach (var srdz in shopnormcopies)
                srdz.Status = RDZ_STATUS.FILL_BY_COPY;

            // -------------------------------------------- //
            // Deal with things that need removing:
            var remIDs = LEvents.SelectMany(selector: LE => LE.RemoveIDs ?? new List<int>());
            var shopsrem = shoprdzs.Join(inner: remIDs,
                                    outerKeySelector: srdz => srdz.ParamID,
                                    innerKeySelector: remid => remid,
                                    resultSelector: (srdz, remid) => srdz);
            foreach (var srdz in shopsrem)
                srdz.Status = RDZ_STATUS.SHOPREMOVE;

            // -------------------------------------------- //
            // Trade shops:

            // "Ordinary TradeShop" has nothing special to handle (aka Straid)
            var normtrades = LEvents.Where(lev => lev.IsTrade && !lev.IsCopy);
            var shopsnt = shoprdzs.Join(inner: normtrades,
                                         outerKeySelector: srdz => srdz.ParamID,
                                         innerKeySelector: lev => lev.KeepID,
                                         resultSelector: (srdz, lev) => srdz);
            foreach (var srdz in shopsnt)
                srdz.Status = RDZ_STATUS.UNLOCKTRADE;

            // Ornifex FreeTrades need a flag setting to zeroise pricerate after creation
            // Note, these IDs still go through the "usual" randomization process first!
            var ft = LEvents.Where(lev => lev.FreeTrade);
            var shopsft = shoprdzs.Join(inner: ft,
                                         outerKeySelector: srdz => srdz.ParamID,
                                         innerKeySelector: lev => lev.KeepID,
                                         resultSelector: (srdz, lev) => srdz);
            foreach (var srdz in shopsft)
                srdz.Status = RDZ_STATUS.MAKEFREE;

            // The TradeShopCopy ones DO NOT go through "ordinary randomization"
            // they instead, just copy the above ones, and then change price
            var tsc = LEvents.Where(lev => lev.IsTrade && lev.IsCopy);
            var shopstsc = shoprdzs.Join(inner: tsc,
                                         outerKeySelector: srdz => srdz.ParamID,
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

        //internal void FixShopEvents1()
        //{
        //    // This function stops shops from re-randomizing
        //    // when npcs move or update their shops

        //    // Go through and clone the "normal" shops:
        //    var PTF = new List<ShopRow>();
        //    var LEvents = ShopRules.GetLinkedEvents();

        //    // Get list of all undisabled:
        //    var tokeep = LEvents.Select(le => le.KeepID);
        //    var tolose = LEvents.SelectMany(le => le.RemoveIDs);

        //    // Clone vanilla shops, edit and then remove bad rows:
        //    foreach(var SR in VanillaShops)
        //        PTF.Add(SR.Clone()); 

        //    // Remove exclusions from list
        //    foreach(var excl in ShopRules.Exclusions)
        //    {
        //        var torem = PTF.FirstOrDefault(SR => SR.ID == excl);
        //        if (torem == null) continue;
        //        PTF.Remove(torem);
        //    }

        //    // Sort out linked events:
        //    foreach (var LE in LEvents)
        //    {
        //        // Sort out tokeep
        //        var shopkeep = PTF.FirstOrDefault(SR => SR.ID == LE.KeepID);
        //        if (shopkeep == null) throw new Exception("Error in finding linked shop ID");
                
        //        // Different situations to handle for trades/normal npc move events:
        //        if (LE.IsTrade)
        //        {
        //            // All Ornifex trades (the ones with a "1" seem to be foricbly enabled after the free trade)
        //            var shopft = PTF.FirstOrDefault(SR => SR.ID == LE.KeepID);
        //            if (shopft == null) throw new Exception("Error finding trade ID");
        //            shopft.EnableFlag = -1;  // enable (show) immediately (except Ornifex "1" trades that are locked behind event)
        //            shopft.DisableFlag = -1;
        //            shopft.WriteRow(); // save to memory

        //            // Remove these from list of what is to be populated
        //            if (LE.CopyID != 0)
        //                PTF.Remove(shopft); // copy still happens in final function
        //        } 
                
        //        else if (LE.IsCopy)
        //        {
        //            foreach (var torem in LE.RemoveIDs)
        //            {
        //                var shoprem = PTF.FirstOrDefault(SR => SR.ID == torem);
        //                if (shoprem == null) continue;
        //                shoprem.ClearShop(); // Memory write!
        //                shoprem.CopyShopFromParamID = LE.CopyID;
        //                ShopsToFillByCopying.Add(shoprem); // add it to the "deal with later" list
        //                PTF.Remove(shoprem); // remove from "deal with now" list
        //            }
        //        }
                
        //        // Sort out to remove:
        //        foreach (var torem in LE.RemoveIDs)
        //        {
        //            var shoprem = PTF.FirstOrDefault(SR => SR.ID == torem);
        //            if (shoprem == null) continue;
        //            shoprem.ClearShop(); // Memory write!
        //            PTF.Remove(shoprem);
        //        }
        //    }
        //    FixedVanillaShops = PTF;
        //}
        
        internal void AddShopsToLogic()
        {
            foreach (var sr in VanillaShops)
            {
                // Append:
                RandoInfo RI = new(sr.ParamDesc, PICKUPTYPE.SHOP);
                Logic.AppendKvp(new KeyValuePair<int, RandoInfo>(sr.ID, RI));
            }
        }
        internal void DefineKRG()
        {
            // Take a copy of the FlatList so we don't need to regenerate everytime:
            var flatlist_copy = LTR_flatlist.Select(di => di.Clone()).ToList();

            // Partition into KeyTypes, ReqNonKeys and Generic Loot-To-Randomize:
            ldkeys = flatlist_copy.Where(DI => DI.IsKeyType).ToList();                   // Keys
            ldreqs = flatlist_copy.Where(DI => ItemSetBase.RequiredItems.Contains(DI.ItemID)).ToList(); // Reqs
            ldgens = flatlist_copy.Except(ldkeys).Except(ldreqs).ToList();               // Generics

            // Ensure no meme double placements:
            //if (ldkeys.Any(di => ldreqs.Contains(di)))
            //    throw new Exception("Add a query to remove duplicates here!");
        }

        // Still required??
        //private List<DropInfo> RemoveDuplicateKeys(List<DropInfo> allkeys)
        //{
        //    // First select things which are allowed to be dupes:
        //    var okdupes = new List<KEYID>()
        //    {   KEYID.TORCH, KEYID.PHARROSLOCKSTONE, KEYID.FRAGRANTBRANCH,
        //        KEYID.SOULOFAGIANT, KEYID.SMELTERWEDGE, KEYID.FLAMEBUTTERFLY,
        //        KEYID.NADALIAFRAGMENT,
        //    };
        //    var okdupesint = okdupes.Cast<int>();

        //    var dupekeys = allkeys.Where(di => okdupesint.Contains(di.ItemID)).ToList();
        //    var alluniquekeys = allkeys.Where(di => !okdupesint.Contains(di.ItemID));

        //    // Probably a better way of doing this by overloading isEqual but has other considerations
        //    List<DropInfo> uniquekeys = new List<DropInfo>();
        //    for (int i = 0; i < alluniquekeys.Count(); i++)
        //    {
        //        var currdrop = alluniquekeys.ElementAt(i);
        //        if (uniquekeys.Any(di => di.ItemID == currdrop.ItemID))
        //            continue;
        //        uniquekeys.Add(currdrop);
        //    }
        //    return dupekeys.Concat(uniquekeys).ToList();
        //}
        internal void FixFlatList()
        {
            // Ensure 5 SoaGs (game defines these weirdly)
            var soag = LTR_flatlist.Where(di => di.ItemID == (int)KEYID.SOULOFAGIANT).First();
            LTR_flatlist.Add(soag);
            LTR_flatlist.Add(soag);

            // Fixes:
            //ldkeys = RemoveDuplicateKeys(ldkeys); // avoid double ashen mist etc.
            RemoveFirstIfPresent(0x0308D330); // Ashen Mist duplicate

            //// Remove Lord soul duplicates:

            //RemoveFirstIfPresent(64060000); // Sinner
            //RemoveFirstIfPresent(64170000); // Freja
            //RemoveFirstIfPresent(64120000); // Old Iron King
        }
        private void RemoveFirstIfPresent(int itemid)
        {
            var di = LTR_flatlist.Where(di => di.ItemID == itemid).FirstOrDefault();
            if (di == null)
                return;
            LTR_flatlist.Remove(di); 
        }
        
        internal void PlaceSet(List<DropInfo> ld, SetType flag)
        {
            // ld: list of DropInfos
            while (ld.Count > 0)
            {
                if (Unfilled.Count == 0)
                    break;

                int keyindex = RNG.Next(ld.Count);
                DropInfo di = ld[keyindex]; // get item to place
                //PlaceItem(di.Clone(), flag);
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
                if (Logic.IsBannedType(rdz, stype))
                {
                    localunfilled.RemoveAt(pindex);
                    continue;
                }

                // Check key-softlock conditions:
                if (stype == SetType.Keys && Logic.IsSoftlockPlacement(rdz, KeysPlacedSoFar))
                {
                    localunfilled.RemoveAt(pindex);
                    continue;
                }

                //
                // Add a Logic.RulesCheck condition // TODO
                //


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
        internal void WriteAllLots(List<ItemLot> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            Hook?.ItemLotOtherParam?.WriteModifiedParam();
        }
        internal void WriteSomeLots(List<ItemLot> somelots)
        {
            // Method used for just writing a few rows out of the Param
            somelots.ForEach(lot => lot.ParamRow.WriteRow());
        }
        internal void WriteSomeShops(List<ShopRow> shops, bool isshuf)
        {
            // Method used for just writing a few rows out of the Param
            shops.ForEach(SR => SR.WriteRow());
        }
        internal void WriteAllShops(List<ShopRow> all_shops, bool isshuf)
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
        internal void WriteShuffledShops()
        {
            if (Hook == null)
                return;

            var shuffledshops = AllP.OfType<ShopRdz>().Select(sdz => sdz.ShuffledShop).ToList();
            WriteAllShops(shuffledshops, true);
        }
        
        // Utility:
        internal Dictionary<int, string> ReadShopNames()
        {
            Dictionary<int, string> shopnames = new Dictionary<int, string>();

            // Read all:
            var lines = File.ReadAllLines("./Resources/Paramdex_DS2S_09272022/ShopLineupParam.txt");

            // Setup parser:
            Regex re = new Regex(@"(?<paramid>\d+) (?<desc>.*)");
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
                    StringBuilder sb = new StringBuilder(itemname);
                    int quant = rdz.GetShuffledItemQuant(keyid);
                    if (quant != 1)
                        sb.Append($" x{quant}");
                    
                    string? desc = Logic.D[rdz.ParamID].Description;
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

            // Write file:
            File.WriteAllLines("./all_answers.txt", lines.ToArray());
        }
        //internal void FixMaughlinEvent()
        //{
        //    // His update event seems to be unique in that it clears previous stuff?

        //    var maughlin_events = new List<LinkedShopEvent>()
        //    {
        //        new LinkedShopEvent(76100211, 76100219), // Maughlin royal sodlier helm
        //        new LinkedShopEvent(76100212, 76100220), // Maughlin royal sodlier armour
        //        new LinkedShopEvent(76100213, 76100221), // Maughlin royal sodlier gauntlets
        //        new LinkedShopEvent(76100214, 76100222), // Maughlin royal sodlier leggings
        //        new LinkedShopEvent(76100215, 76100223), // Maughlin elite knight helm
        //        new LinkedShopEvent(76100216, 76100224), // Maughlin elite knight armour
        //        new LinkedShopEvent(76100217, 76100225), // Maughlin elite knight gauntlets
        //        new LinkedShopEvent(76100218, 76100226), // Maughlin elite knight leggings
        //    };

        //    var cloneshops = new List<ShopRow>();
        //    foreach (LinkedShopEvent LE in maughlin_events)
        //    {
        //        var goodshop = AllPTR.OfType<ShopRdz>().Where(rdz => rdz.ParamID == LE.KeepID).First();
        //        if (goodshop.ShuffledShop == null)
        //            throw new NullReferenceException("Shouldn't get here");

        //        // this still isn't a perfect solution because of quantities
        //        var vanshop = VanillaShops.Where(si => si.ID == LE.RemoveIDs.First()).First();
        //        vanshop.ItemID = goodshop.ShuffledShop.ItemID;
        //        vanshop.Quantity = goodshop.ShuffledShop.Quantity;
        //        vanshop.PriceRate = goodshop.ShuffledShop.PriceRate;
        //        cloneshops.Add(vanshop);
        //    }

        //    if (Hook == null)
        //        return;
        //    WriteSomeShops(cloneshops, true);
        //}
        //internal void FixOrnifexEvent()
        //{
        //    // Need to make her stuff copies of the freetrades for continuity
        //    var ornifex_copies = ShopRules.GetLinkedEvents()
        //                                  .Where(LE => LE.IsTrade && LE.CopyID != 0);

        //    var updateshops = new List<ShopRow>();
        //    foreach (LinkedShopEvent LE in ornifex_copies)
        //    {
        //        var shop_to_copy = AllPTR.OfType<ShopRdz>().Where(rdz => rdz.ParamID == LE.CopyID).First();
        //        if (shop_to_copy.ShuffledShop == null)
        //            throw new NullReferenceException("Shouldn't get here");

        //        var shop_to_edit = VanillaShops.FirstOrDefault(shp => shp.ID == LE.KeepID);
        //        if (shop_to_edit == null) throw new Exception("Cannot find Ornifex trade shop to edit with copy");
        //        //var shop_to_edit = Data.OfType<ShopRdz>().FirstOrDefault(rdz => rdz.ParamID == LE.KeepID);

        //        // Note the event enable/disable are already handled way earlier.
        //        shop_to_edit.ItemID = shop_to_copy.ShuffledShop.ItemID;
        //        shop_to_edit.MaterialID = shop_to_copy.ShuffledShop.MaterialID;
        //        shop_to_edit.Quantity = shop_to_copy.ShuffledShop.Quantity;
        //        shop_to_edit.PriceRate = shop_to_copy.ShuffledShop.PriceRate;

        //        // Finally, fix the original shops to be free:
        //        shop_to_copy.ShuffledShop.PriceRate = 0;
        //        updateshops.Add(shop_to_copy.ShuffledShop);
        //        updateshops.Add(shop_to_edit);
        //    }

        //    if (Hook == null) return;
        //    WriteSomeShops(updateshops, true);
        //}
        //internal void FixGilliganEvent()
        //{
        //    // And Gilligan Events
        //    var updateshops = new List<ShopRow>();

        //    foreach (var shp in ShopsToFillByCopying)
        //    {
        //        var shop_to_copy = AllPTR.OfType<ShopRdz>().Where(rdz => rdz.ParamID == shp.CopyShopFromParamID).First();
        //        if (shop_to_copy.ShuffledShop == null)
        //            throw new NullReferenceException("Shouldn't get here");

        //        // Note the event enable/disable are already handled way earlier.
        //        shp.ItemID = shop_to_copy.ShuffledShop.ItemID;
        //        shp.MaterialID = shop_to_copy.ShuffledShop.MaterialID;
        //        shp.Quantity = shop_to_copy.ShuffledShop.Quantity;
        //        shp.PriceRate = shop_to_copy.ShuffledShop.PriceRate;

        //        // Add to list to commit to memory
        //        updateshops.Add(shp);
        //    }

        //    if (Hook == null) return;
        //    WriteSomeShops(updateshops, true);
        //}

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
                var LE = shopcopies.FirstOrDefault(lev => lev.FillByCopy == shp.ParamID);
                if (LE == null)
                    throw new Exception("Cannot find linked event");

                var shop_to_copy = filled_shops.Where(srdz => srdz.ParamID == LE.CopyID).FirstOrDefault();
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
                var LE = tradecopies.FirstOrDefault(lev => lev.FillByCopy == shp.ParamID);
                if (LE == null)
                    throw new Exception("Cannot find linked event");

                var shop_to_copy = filled_shops.Where(srdz => srdz.ParamID == LE.CopyID).FirstOrDefault();
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
                var LD = Logic.LinkedDrops.FirstOrDefault(ld => ld.SlaveIDs.Contains(lot.ParamID));
                if (LD == null) 
                    throw new Exception("Cannot find LinkedDrop as expected");

                // Get Randomized ItemLot to copy from:
                var lot_to_copy = AllPTR.OfType<LotRdz>().Where(ldz => ldz.ParamID == LD.MasterID).First();

                // Clone/Update:
                lot.ShuffledLot = lot.VanillaLot.CloneBlank();              // keep param reference for this ID
                lot.ShuffledLot.CloneValuesFrom(lot_to_copy.ShuffledLot);   // set to new values
                lot.MarkHandled();
            }
        }

        // RNG related:
        private const double priceMeanGaussian = 3000;  // For Gaussian distribution
        private const double priceSDGaussian = 500;     // For Gaussian distribution
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
            List<double> RVu = new List<double>(); // RandomVariables Uniform(0,1] distribution
            List<double> RVe = new List<double>(); // RandomVariables Exponential(1) distribution
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
