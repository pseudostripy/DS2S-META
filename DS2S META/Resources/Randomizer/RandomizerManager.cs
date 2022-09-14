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
        DS2SHook Hook;
        List<Randomization> Data = new List<Randomization>(); // Combined info list
        internal static Random RNG = new Random();
        private Dictionary<int, ItemLot> VanillaLots;
        private Dictionary<int, ShopInfo> VanillaShops;
        private Dictionary<int, ShopInfo> FixedVanillaShops;
        internal ItemSetBase Logic;
        internal List<DropInfo> LTR_flatlist;
        internal bool IsInitialized = false;
        internal bool IsRandomized = false;
        // 
        internal List<DropInfo> ldkeys;
        internal List<DropInfo> ldreqs;
        internal List<DropInfo> ldgens;
        //
        private List<int> Unfilled = new List<int>();
        private List<int> KeysPlacedSoFar = new List<int>(); // to tidy
        //
        internal static Dictionary<int, ItemParam> VanillaItemParams;
        internal static string GetItemName(int itemid) => VanillaItemParams[itemid].MetaItemName;
        internal static bool TryGetItemName(int itemid, out string name)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            name = found ? GetItemName(itemid) : "";
            return found;
        }
        internal static bool TryGetItem(int itemid, out ItemParam item)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            item = found ? VanillaItemParams[itemid] : null;
            return found;
        }
        internal int GetItemMaxUpgrade(ItemParam item)
        {
            // Wrapper similar to the DS2Item class call in Hook.
            switch (item.ItemType)
            {
                case eItemType.WEAPON: // & shields
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
        internal RandomizerManager() { }

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
        internal void Randomize(int seed)
        {
            // Setup for re-randomization:
            SetSeed(seed);      // reset Rng Twister
            GetLootToRandomize(); // set Data field
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
            WriteShuffledLots();
            WriteShuffledShops();
            Hook.WarpLast();    // Force an area reload. TODO add warning:
            IsRandomized = true;
        }
        internal void Unrandomize()
        {
            WriteVanillaShops();
            WriteVanillaLots();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();
            IsRandomized = false;
        }

        // Core Logic
        internal void GetLootToRandomize()
        {
            Data = new List<Randomization>(); // Reset

            // For each vanilla lot, make a new randomization object
            IEnumerable<Randomization> ltr = VanillaLots.Select(kvp => new LotRdz(kvp))
                            .Where(ldz => ldz.VanillaLot.NumDrops != 0)
                            .Where(ldz => Logic.AvoidsTypes(ldz, Logic.BanFromLoot));
            List<Randomization> listltr = ltr.ToList();


            // Get shop loot
            IEnumerable<Randomization> shoploot = FixedVanillaShops.Select(kvp => new ShopRdz(kvp));
            
            // List of Places to fill:
            Data.AddRange(listltr.Where(ldz => Logic.AvoidsTypes(ldz, Logic.BanGeneralTypes)));
            Data.AddRange(shoploot);            // Add shop loot to be filled

            // Define what loot can be distributed:
            listltr.AddRange(shoploot);         // Add shop loot for distribution
            LTR_flatlist = listltr.SelectMany(rz => rz.Flatlist).ToList();
            FixFlatList(); // ensure correct number of keys etc
        }
        internal Dictionary<int, ShopInfo> FixShopEvents()
        {
            // Code is probably written awfully, but whatever. I might refactor this one day.

            // Go through and clone the "normal" shops:
            var PTF = new Dictionary<int, ShopInfo>();
            var handled_cases = new List<int>();
            var skip_shops = new List<int>();
            foreach (var kvp in VanillaShops)
            {
                if (handled_cases.Contains(kvp.Key))
                    continue;

                // Remove the empty shop from LTR
                if (kvp.Value.ItemID == 0)
                    continue;

                // "normal case"
                if (kvp.Value.DisableFlag == -1)
                {
                    PTF.Add(kvp.Key, kvp.Value.Clone());
                    handled_cases.Add(kvp.Key);
                    continue;
                }

                // "disable flag set" - find and fix it's enable counterpart:
                int disableEvt = kvp.Value.DisableFlag;
                var ctrparts = VanillaShops.Where(kvp2 => kvp2.Value.EnableFlag == disableEvt).ToList();

                foreach (var KvpEnShop in ctrparts)
                {
                    handled_cases.Add(KvpEnShop.Key);
                    skip_shops.Add(KvpEnShop.Key); // this is for final pass checks
                }

                // Fix shop that has disablement
                ShopInfo SI = kvp.Value.Clone();
                SI.DisableFlag = -1;    // Don't remove it on merchant move
                SI.PriceRate = 1.00f;   // Just default all these to 1 for now to make prices consistent when they move around.
                PTF.Add(kvp.Key, SI);
            }

            // One more pass to catch params in unfortunate order:
            return PTF.Where(kvp => !skip_shops.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
            if (!TryGetItem(di.ItemID, out ItemParam item))
                return;

            switch (item.ItemType)
            {
                case eItemType.STAFFCHIME:
                case eItemType.WEAPON:
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
            if (!TryGetItem(di.ItemID, out ItemParam item))
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
            
            List<string> lines = new List<string>();
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
                    
                    string desc = Logic.D[rdz.ParamID].Description;
                    sb.Append($": {desc}");
                    lines.Add(sb.ToString());
                }
            }

            // Write file:
            File.WriteAllLines("./keytesting.txt", lines.ToArray());
        }
        internal void PrintAllRdz()
        {
            List<string> lines = new List<string>();

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
            foreach(var kvp in VanillaLots)
            {
                var IL = kvp.Value.Clone();
                IL.Zeroise();
                Hook.WriteItemLotTable(kvp.Key, IL);
            }

            foreach(var kvp in VanillaShops)
            {
                ShopInfo blankshop = kvp.Value.Clone();
                blankshop.RawQuantity = 0;
                blankshop.ItemID = 0; // don't even show in shop
                var blankshopkvp = new KeyValuePair<int, ShopInfo>(kvp.Key, blankshop);
                Hook.WriteShopInfo(blankshopkvp);
            }
        }
        internal void WriteShuffledLots()
        {
            var shuffledlots = Data.OfType<LotRdz>().ToDictionary(ldz => ldz.ParamID, ldz => ldz.ShuffledLot);
            Hook.WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledShops()
        {
            var shuffledshops = Data.OfType<ShopRdz>().ToDictionary(ldz => ldz.ParamID, ldz => ldz.ShuffledShop);
            Hook.WriteAllShops(shuffledshops, true);
        }
        internal void WriteVanillaShops()
        {
            Hook.WriteAllShops(VanillaShops, false);
        }
        internal void WriteVanillaLots()
        {
            Hook.WriteAllLots(VanillaLots);
        }

        // RNG related:
        private const double priceMeanGaussian = 3000;  // For Gaussian distribution
        private const double priceSDGaussian = 500;     // For Gaussian distribution
        internal const double priceShapeK = 3.0;        // For Gamma distribution
        internal const double priceScaleTh = 2.0;       // For Gamma distribution
        internal void SetSeed(int seed)
        {
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
