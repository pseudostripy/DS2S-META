using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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
        private Dictionary<int, ItemLot> VanillaLots = new();
        private Dictionary<int, ShopInfo> VanillaShops = new();
        private Dictionary<int, ShopInfo> FixedVanillaShops = new();
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
        internal static Dictionary<int, ItemParam> VanillaItemParams = new();
        internal static string GetItemName(int itemid) => VanillaItemParams[itemid].MetaItemName;
        internal static bool TryGetItemName(int itemid, out string name)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            name = found ? GetItemName(itemid) : "";
            return found;
        }
        internal static bool TryGetItem(int itemid, out ItemParam? item)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            item = found ? VanillaItemParams[itemid] : null;
            return found;
        }
        internal int GetItemMaxUpgrade(ItemParam item)
        {
            if (Hook == null)
                return 0;

            // Wrapper similar to the DS2Item class call in Hook.
            switch (item.ItemType)
            {
                case eItemType.WEAPON1: // & shields
                case eItemType.WEAPON2: // & staves
                    return Hook.GetWeaponMaxUpgrade(item.ItemID);
                case eItemType.HEADARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                case eItemType.LEGARMOUR:
                    return Hook.GetArmorMaxUpgrade(item.ItemID);

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

            // Get Vanilla Params:
            var parShopDesc = ReadShopNames();
            VanillaShops = Hook.GetVanillaShops(parShopDesc);
            VanillaLots = Hook.GetVanillaLots();
            VanillaItemParams = Hook.GetVanillaItemParams();
            Logic = new CasualItemSet();
            FixedVanillaShops = FixShopEvents(); // Update PTF with shop places
            AddShopLogic();

            // Add descriptions
            foreach (var kvp in VanillaLots)
                kvp.Value.ParamDesc = Logic.GetDesc(kvp.Key);
            IsInitialized = true;
        }
        internal async Task Randomize(int seed)
        {
            if (Hook == null)
                return;

            // Setup for re-randomization:
            SetSeed(seed);      // reset Rng Twister
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
            ClearLotsShops();   // Zeroise everything first (to avoid vanilla item leak)
            await Task.Run( () => WriteShuffledLots());
            await Task.Run( () => WriteShuffledShops());
            //DisableUnusedShops();
            FixMaughlinEvent();
            Hook.WarpLast();    // Force an area reload. TODO add warning:
            IsRandomized = true;
        }
        internal void Unrandomize()
        {
            if (Hook == null)
                return;

            WriteVanillaShops();
            WriteVanillaLots();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();
            IsRandomized = false;
        }
        internal int GetRandom()
        {
            return RNG.Next();
        }


        // Core Logic
        internal void GetLootToRandomize()
        {
            Data = new List<Randomization>(); // Reset

            // For each vanilla lot, make a new randomization object
            IEnumerable<Randomization> ltr = VanillaLots.Select(kvp => new LotRdz(kvp))
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
        internal Dictionary<int, ShopInfo> FixShopEvents()
        {
            // Remove shop & trade menu resets on certain events so they stay randomised
            // Go through and clone the "normal" shops:
            var PTF = new Dictionary<int, ShopInfo>();
            
            var LEvents = ShopRules.GetLinkedEvents();

            // Get list of all undisabled:
            var tokeep = LEvents.Select(le => le.KeepID);
            var tolose = LEvents.SelectMany(le => le.RemoveIDs);


            foreach (var kvp in VanillaShops)
            {
                if (ShopRules.Exclusions.Contains(kvp.Key))
                    continue; // empty shops etc

                // Remove events:
                if (tolose.Contains(kvp.Key))
                    continue;

                // Keep and don't disable events:
                if (tokeep.Contains(kvp.Key))
                {
                    var shopid = LEvents.Where(le => le.KeepID == kvp.Key).First();
                    var normshop = VanillaShops[shopid.KeepID].Clone();
                    normshop.DisableFlag = -1;
                    PTF.Add(kvp.Key, normshop);
                    continue;
                }

                // Everything else:
                PTF.Add(kvp.Key, kvp.Value.Clone());
            }
            return PTF;
        }
        internal void AddShopLogic()
        {
            foreach (var kvp in FixedVanillaShops)
            {
                // Unpack:
                ShopInfo SI = kvp.Value;
                int shopparamid = kvp.Key;

                // Append:
                RandoInfo RI = new RandoInfo(SI.ParamDesc, PICKUPTYPE.SHOP);
                Logic.AppendKvp(new KeyValuePair<int, RandoInfo>(shopparamid, RI));
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
            LTR_flatlist.Remove(LTR_flatlist.Where(di => di.ItemID == 64140000).First()); // Rotten
            LTR_flatlist.Remove(LTR_flatlist.Where(di => di.ItemID == 64060000).First()); // Sinner
            LTR_flatlist.Remove(LTR_flatlist.Where(di => di.ItemID == 64170000).First()); // Freja
            LTR_flatlist.Remove(LTR_flatlist.Where(di => di.ItemID == 64120000).First()); // Old Iron King
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
            if (Hook == null)
                return;
            if (!TryGetItem(di.ItemID, out ItemParam? item))
                return;
            if (item == null)
                return;

            switch (item.ItemType)
            {
                case eItemType.WEAPON1:
                case eItemType.WEAPON2:
                    var infusionOptions = Hook.GetWeaponInfusions(item.ItemID);
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
            if (!TryGetItem(di.ItemID, out ItemParam? item))
                return;
            if (item == null)
                return;
            var maxupgrade = GetItemMaxUpgrade(item);
            di.Reinforcement = (byte)Math.Min(di.Reinforcement, maxupgrade); // limit to item max upgrade
        }
        
        // Utility:
        internal Dictionary<int, string> ReadShopNames()
        {
            Dictionary<int, string> shopnames = new Dictionary<int, string>();

            // Read all:
            var lines = File.ReadAllLines("./Resources/Randomizer/ShopLineupParam.txt");

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
        internal void ClearLotsShops()
        {
            if (Hook == null)
                return;

            foreach(var kvp in VanillaLots)
            {
                var IL = kvp.Value.Clone();
                IL.Zeroise();
                Hook.WriteItemLotTable(kvp.Key, IL);
            }

            foreach(var kvp in VanillaShops)
            {
                ShopInfo blankshop = kvp.Value.Clone();
                blankshop.Quantity = 0;
                blankshop.ItemID = 0; // don't even show in shop
                var blankshopkvp = new KeyValuePair<int, ShopInfo>(kvp.Key, blankshop);
                Hook.WriteShopInfo(blankshopkvp);
            }
        }
        internal void WriteShuffledLots()
        {
            if (Hook == null)
                return;

            var shuffledlots = Data.OfType<LotRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .ToDictionary(ldz => ldz.ParamID, ldz => ldz.ShuffledLot);
            Hook.WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledShops()
        {
            if (Hook == null)
                return;

            var shuffledshops = Data.OfType<ShopRdz>().ToDictionary(ldz => ldz.ParamID, ldz => ldz.ShuffledShop);
            Hook.WriteAllShops(shuffledshops, true);
        }
        internal void WriteVanillaShops()
        {
            if (Hook == null)
                return;
            Hook.WriteAllShops(VanillaShops, false);
        }
        internal void WriteVanillaLots()
        {
            if (Hook == null)
                return;
            Hook.WriteAllLots(VanillaLots);
        }
        internal void DisableUnusedShops()
        {
            var dicbadshops = new Dictionary<int, ShopInfo>();

            var tolose = ShopRules.GetLinkedEvents().SelectMany(le => le.RemoveIDs);
            var badshops = VanillaShops.Where(kvp => tolose.Contains(kvp.Key));
            foreach (var shop in badshops)
            {
                var badshop = shop.Value.Clone();
                badshop.EnableFlag = -1;
                badshop.Quantity = 0;
                badshop.ItemID = 0; // don't even show in shop
                dicbadshops[shop.Key] = badshop;
            }

            if (Hook == null)
                return;
            Hook.WriteAllShops(dicbadshops, true);
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

            var cloneshops = new Dictionary<int, ShopInfo>();
            foreach (LinkedShopEvent LE in maughlin_events)
            {
                var goodshop = Data.OfType<ShopRdz>().Where(rdz => rdz.ParamID == LE.KeepID).First();
                if (goodshop.ShuffledShop == null)
                    throw new NullReferenceException("Shouldn't get here");

                // this still isn't a perfect solution because of quantities
                var badshopkey = LE.RemoveIDs.First();
                var badshop = goodshop.ShuffledShop.Clone();
                var vanshop = VanillaShops[badshopkey];
                badshop.EnableFlag = vanshop.EnableFlag; // still enable when it wants to -> should "trade places"
                cloneshops[badshopkey] = badshop;
            }

            if (Hook == null)
                return;
            Hook.WriteAllShops(cloneshops, true);
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
    }
}
