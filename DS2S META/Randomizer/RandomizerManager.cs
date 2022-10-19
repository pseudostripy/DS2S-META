using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using DS2S_META.Utils;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Handle logic & data related to Randomizer
    /// </summary>
    internal class RandomizerManager
    {
        // Fields:
        DS2SHook? Hook;
        List<Randomization> Data = new List<Randomization>(); // Combined info list
        internal static Random RNG = new Random();
        private List<ItemLot> VanillaLots = new();
        private List<ShopRow> VanillaShops = new();
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
            GetVanillaShops();
            GetVanillaLots();
            VanillaItemParams = Hook.Items.ToDictionary(it => it.ID, it => it);
            Logic = new CasualItemSet();
            FixShopEvents1(); // Update PTF with shop places
            AddShopLogic();

            // Add descriptions
            foreach (var ilot in VanillaLots)
                ilot.ParamDesc = Logic.GetDesc(ilot.ID);
            IsInitialized = true;
        }
        internal void GetVanillaLots()
        {
            if (Hook?.ItemLotOtherParam == null)
                throw new NullReferenceException("Shouldn't get here");

            VanillaLots = Hook.ItemLotOtherParam.Rows.Select(row => new ItemLot(row)).ToList();
            return;
        }
        internal void GetVanillaShops()
        {
            var vanshops = ParamMan.ShopLineupParam?.Rows.OfType<ShopRow>().ToList();
            if (vanshops == null)
                throw new NullReferenceException("Shouldn't get here");
            VanillaShops = vanshops;
            return;
        }

        internal async Task Randomize(int seed)
        {
            if (Hook == null)
                return;

            // Setup for re-randomization:
            SetSeed(seed);      // reset Rng Twister
            FixShopEvents1(); // Update PTF with shop places
            RandomizeStartingClasses();
            
            GetLootToRandomize(); // set Data field
            KeysPlacedSoFar = new List<int>(); // nice bug :)
            Unfilled = Enumerable.Range(0, Data.Count).ToList();
            DefineKRG();        // Split items into Keys, Required, Generics


            // Place sets of items:
            PlaceSet(ldkeys, SetType.Keys);
            PlaceSet(ldreqs, SetType.Reqs);
            PlaceSet(ldgens, SetType.Gens);
            FillLeftovers();

            // Printout the current shuffled lots:
            PrintKeysNeat();
            PrintAllRdz();

            // Randomize Game!
            await Task.Run(() => WriteShuffledLots());
            await Task.Run(() => WriteShuffledShops());
            FixMaughlinEvent();
            FixOrnifexEvent();
            Hook.WarpLast();    // Force an area reload. TODO add warning:
            IsRandomized = true;

            // maybe enable all the 1s too and see if they get removed on event

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
                while (RNG.Next(100) < 20 && lhwepnum < 2)
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
            Data = new List<Randomization>(); // Reset

            // For each vanilla lot, make a new randomization object
            IEnumerable<Randomization> ltr = VanillaLots.Select(lot => new LotRdz(lot))
                            .Where(ldz => ldz.VanillaLot?.NumDrops != 0)
                            .Where(ldz => Logic.AvoidsTypes(ldz, Logic.BanFromLoot))
                            .Where(ldz => !Logic.CrowDuplicates.Contains(ldz.ParamID));
            List<Randomization> listltr = ltr.ToList();


            // Get shop loot
            IEnumerable<Randomization> shoploot = FixedVanillaShops.Select(kvp => new ShopRdz(kvp));

            
            // List of Places to fill:
            var lotptf = listltr.Where(ldz => Logic.AvoidsTypes(ldz, Logic.BanGeneralTypes));
            Data.AddRange(lotptf);
            Data.AddRange(shoploot);  // Add shop loot to be filled

            // Define what loot can be distributed:
            listltr.AddRange(shoploot);         // Add shop loot for distribution
            LTR_flatlist = listltr.SelectMany(selector: rz => rz.Flatlist).ToList();
            FixFlatList(); // ensure correct number of keys etc
        }
        internal void FixShopEvents1()
        {
            // This function stops shops from re-randomizing
            // when npcs move or update their shops

            // Go through and clone the "normal" shops:
            var PTF = new List<ShopRow>();
            var LEvents = ShopRules.GetLinkedEvents();

            // Get list of all undisabled:
            var tokeep = LEvents.Select(le => le.KeepID);
            var tolose = LEvents.SelectMany(le => le.RemoveIDs);

            // Clone vanilla shops, edit and then remove bad rows:
            foreach(var SR in VanillaShops)
                PTF.Add(SR.Clone()); 

            // Remove exclusions from list
            foreach(var excl in ShopRules.Exclusions)
            {
                var torem = PTF.FirstOrDefault(SR => SR.ID == excl);
                if (torem == null) continue;
                PTF.Remove(torem);
            }

            // Sort out linked events:
            foreach (var LE in LEvents)
            {
                // Sort out tokeep
                var shopkeep = PTF.FirstOrDefault(SR => SR.ID == LE.KeepID);
                if (shopkeep == null) throw new Exception("Error in finding linked shop ID");
                
                // Different situations to handle for trades/normal npc move events:
                if (LE.IsTrade)
                {
                    // All Ornifex trades (the ones with a "1" seem to be foricbly enabled after the free trade)
                    var shopft = PTF.FirstOrDefault(SR => SR.ID == LE.KeepID);
                    if (shopft == null) throw new Exception("Error finding trade ID");
                    shopft.EnableFlag = -1;  // enable (show) immediately (except Ornifex "1" trades that are locked behind event)
                    shopft.DisableFlag = -1;
                    shopft.WriteRow(); // save to memory

                    // Remove these from list of what is to be populated
                    if (LE.CopyID != 0)
                        PTF.Remove(shopft); // copy still happens in final function
                }
                
                // Sort out to remove:
                foreach(var torem in LE.RemoveIDs)
                {
                    var shoprem = PTF.FirstOrDefault(SR => SR.ID == torem);
                    if (shoprem == null) continue;
                    shoprem.ClearShop(); // Memory write!
                    PTF.Remove(shoprem);
                }
            }
            FixedVanillaShops = PTF;
        }
        
        internal void AddShopLogic()
        {
            foreach (var si in FixedVanillaShops)
            {
                // Append:
                RandoInfo RI = new RandoInfo(si.ParamDesc, PICKUPTYPE.SHOP);
                Logic.AppendKvp(new KeyValuePair<int, RandoInfo>(si.ID, RI));
            }
        }
        internal void DefineKRG()
        {
            // Partition into KeyTypes, ReqNonKeys and Generic Loot-To-Randomize:
            ldkeys = LTR_flatlist.Where(DI => DI.IsKeyType).ToList();                   // Keys
            ldreqs = LTR_flatlist.Where(DI => ItemSetBase.RequiredItems.Contains(DI.ItemID)).ToList(); // Reqs
            ldgens = LTR_flatlist.Except(ldkeys).Except(ldreqs).ToList();               // Generics

            // Fixes:
            ldkeys = RemoveDuplicateKeys(ldkeys); // avoid double ashen mist etc.

            // Ensure no meme double placements:
            if (ldkeys.Any(di => ldreqs.Contains(di)))
                throw new Exception("Add a query to remove duplicates here!");
        }
        private List<DropInfo> RemoveDuplicateKeys(List<DropInfo> allkeys)
        {
            // First select things which are allowed to be dupes:
            var okdupes = new List<KEYID>()
            {   KEYID.TORCH, KEYID.PHARROSLOCKSTONE, KEYID.FRAGRANTBRANCH,
                KEYID.SOULOFAGIANT, KEYID.SMELTERWEDGE, KEYID.FLAMEBUTTERFLY,
                KEYID.NADALIAFRAGMENT,
            };
            var okdupesint = okdupes.Cast<int>();

            var dupekeys = allkeys.Where(di => okdupesint.Contains(di.ItemID)).ToList();
            var alluniquekeys = allkeys.Where(di => !okdupesint.Contains(di.ItemID));

            // Probably a better way of doing this by overloading isEqual but has other considerations
            List<DropInfo> uniquekeys = new List<DropInfo>();
            for (int i = 0; i < alluniquekeys.Count(); i++)
            {
                var currdrop = alluniquekeys.ElementAt(i);
                if (uniquekeys.Any(di => di.ItemID == currdrop.ItemID))
                    continue;
                uniquekeys.Add(currdrop);
            }
            return dupekeys.Concat(uniquekeys).ToList();
        }
        internal void FixFlatList()
        {
            // Ensure 5 SoaGs (game defines these weirdly)
            var soag = LTR_flatlist.Where(di => di.ItemID == (int)KEYID.SOULOFAGIANT).First();
            LTR_flatlist.Add(soag);
            LTR_flatlist.Add(soag);

            // Remove Lord soul duplicates:
            RemoveFirstIfPresent(64140000); // Rotten
            RemoveFirstIfPresent(64060000); // Sinner
            RemoveFirstIfPresent(64170000); // Freja
            RemoveFirstIfPresent(64120000); // Old Iron King
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
                PlaceItem(di.Clone(), flag);
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
                var rdz = Data.ElementAt(elnum);

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
                    Unfilled.Remove(elnum); // now filled!
                
                return;
            }
            throw new Exception("True Softlock, please investigate");
        }
        private void FillLeftovers()
        {
            // ld: list of DropInfos
            int Nfc = ItemSetBase.FillerItems.Count; // fill count
            foreach (var rdz in Data)
            {
                while (!rdz.IsSaturated())
                {
                    int ind = RNG.Next(Nfc);
                    DropInfo item = ItemSetBase.FillerItems[ind]; // get filler item
                    rdz.AddShuffledItem(item);
                }
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

            var shuffledlots = Data.OfType<LotRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledShops()
        {
            if (Hook == null)
                return;

            var shuffledshops = Data.OfType<ShopRdz>().Select(sdz => sdz.ShuffledShop).ToList();
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
            List<string> lines = new List<string>();

            // Intro line
            lines.Add($"Printing key locations for seed {CurrSeed}");
            lines.Add("---------------------------------------------");

            // Main print loop
            foreach (int keyid in ItemSetBase.KeyOutputOrder.Cast<int>())
            {
                if (!TryGetItemName(keyid, out string itemname))
                    continue;

                var rdzsWithKey = Data.Where(rdz => rdz.HasShuffledItemID(keyid)).ToList();
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
            foreach (var ldz in Data.OfType<LotRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Shops:
            lines.Add("");
            lines.Add("Shops:");
            foreach (var rdz in Data.OfType<ShopRdz>())
                lines.Add(rdz.GetNeatDescription());

            // Write file:
            File.WriteAllLines("./all_answers.txt", lines.ToArray());
        }
        internal void FixMaughlinEvent()
        {
            // His update event seems to be unique in that it clears previous stuff?

            var maughlin_events = new List<LinkedShopEvent>()
            {
                new LinkedShopEvent(76100211, 76100219), // Maughlin royal sodlier helm
                new LinkedShopEvent(76100212, 76100220), // Maughlin royal sodlier armour
                new LinkedShopEvent(76100213, 76100221), // Maughlin royal sodlier gauntlets
                new LinkedShopEvent(76100214, 76100222), // Maughlin royal sodlier leggings
                new LinkedShopEvent(76100215, 76100223), // Maughlin elite knight helm
                new LinkedShopEvent(76100216, 76100224), // Maughlin elite knight armour
                new LinkedShopEvent(76100217, 76100225), // Maughlin elite knight gauntlets
                new LinkedShopEvent(76100218, 76100226), // Maughlin elite knight leggings
            };

            var cloneshops = new List<ShopRow>();
            foreach (LinkedShopEvent LE in maughlin_events)
            {
                var goodshop = Data.OfType<ShopRdz>().Where(rdz => rdz.ParamID == LE.KeepID).First();
                if (goodshop.ShuffledShop == null)
                    throw new NullReferenceException("Shouldn't get here");

                // this still isn't a perfect solution because of quantities
                var vanshop = VanillaShops.Where(si => si.ID == LE.RemoveIDs.First()).First();
                vanshop.ItemID = goodshop.ShuffledShop.ItemID;
                vanshop.Quantity = goodshop.ShuffledShop.Quantity;
                vanshop.PriceRate = goodshop.ShuffledShop.PriceRate;
                cloneshops.Add(vanshop);
            }

            if (Hook == null)
                return;
            WriteSomeShops(cloneshops, true);
        }
        internal void FixOrnifexEvent()
        {
            // Need to make her stuff copies of the freetrades for continuity
            var ornifex_copies = ShopRules.GetLinkedEvents()
                                          .Where(LE => LE.IsTrade && LE.CopyID != 0);

            var updateshops = new List<ShopRow>();
            foreach (LinkedShopEvent LE in ornifex_copies)
            {
                var shop_to_copy = Data.OfType<ShopRdz>().Where(rdz => rdz.ParamID == LE.CopyID).First();
                if (shop_to_copy.ShuffledShop == null)
                    throw new NullReferenceException("Shouldn't get here");

                var shop_to_edit = VanillaShops.FirstOrDefault(shp => shp.ID == LE.KeepID);
                if (shop_to_edit == null) throw new Exception("Cannot find Ornifex trade shop to edit with copy");
                //var shop_to_edit = Data.OfType<ShopRdz>().FirstOrDefault(rdz => rdz.ParamID == LE.KeepID);

                // Note the event enable/disable are already handled way earlier.
                shop_to_edit.ItemID = shop_to_copy.ShuffledShop.ItemID;
                shop_to_edit.MaterialID = shop_to_copy.ShuffledShop.MaterialID;
                shop_to_edit.Quantity = shop_to_copy.ShuffledShop.Quantity;
                shop_to_edit.PriceRate = shop_to_copy.ShuffledShop.PriceRate;

                // Finally, fix the original shops to be free:
                shop_to_copy.ShuffledShop.PriceRate = 0;
                updateshops.Add(shop_to_copy.ShuffledShop);
                updateshops.Add(shop_to_edit);
            }

            if (Hook == null) return;
            WriteSomeShops(updateshops, true);
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
