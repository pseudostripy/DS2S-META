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
        internal string GetItemName(int itemid) => VanillaItemParams[itemid].MetaItemName;
        internal bool TryGetItemName(int itemid, out string name)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            name = found ? GetItemName(itemid) : "";
            return found;
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
            AddShopLogic();

            // Fill vanilla data to work with:
            GetLootToRandomize(); // set Data
            Unfilled = Enumerable.Range(0, Data.Count).ToList();
            IsInitialized = true;
        }
        internal void Randomize(int seed)
        {
            // Setup for re-randomization:
            SetSeed(seed);              // reset Rng Twister

            // Clear Lots/Shops to avoid vanilla spillover bug
            // TODO

            // Split items into Keys, Required, Generics
            DefineKRG();

            // Place sets of items:
            PlaceSet(ldkeys, SetType.Keys);
            PlaceSet(ldreqs, SetType.Reqs);
            PlaceSet(ldgens, SetType.Gens);

            // Printout the current shuffled lots:
            PrintKeysNeat();
            // PrintAllItems(); // TODO

            // Randomize Game!
            WriteShuffledLots();
            WriteShuffledShops();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();
            IsRandomized = true;
        }
        internal void Unrandomize()
        {
            WriteVanillaShops();
            WriteVanillaShops();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();
            IsRandomized = false;
        }

        // Core Logic
        internal void GetLootToRandomize()
        {
            // For each vanilla lot, make a new randomization object
            Data.AddRange(VanillaLots.Select(kvp => new LotRdz(kvp))
                            .Where(ldz => ldz.VanillaLot.NumDrops != 0)
                            .Where(ldz => Logic.AvoidsTypes(ldz, Logic.BanFromLoot)));

            // Add shops
            foreach (var kvp in VanillaShops)
            {
                // Remove duplicates and missing content:
                if (ShopRules.Exclusions.Contains(kvp.Key))
                    continue;
                Data.Add(new ShopRdz(kvp));
            }

            LTR_flatlist = Data.SelectMany(rz => rz.Flatlist).ToList();
            FixFlatList(); // ensure correct number of keys etc
        }
        internal void AddShopLogic()
        {
            // Update PTF with shop places
            foreach (var kvp in VanillaShops)
            {
                //// Remove duplicates and missing content:
                //if (ShopRules.Exclusions.Contains(kvp.Key))
                //    continue;

                // Unpack:
                ShopInfo SI = kvp.Value;
                int shopparamid = kvp.Key;

                // Apopend:
                RandoInfo RI = new RandoInfo(SI.ParamDesc, PICKUPTYPE.SHOP);
                Logic.AppendKvp(new KeyValuePair<int, RandoInfo>(shopparamid, RI));
            }
        }
        internal void DefineKRG()
        {
            // Hard-code requireds for now:
            List<int> intreqs = DefineRequiredItems();

            // Partition into KeyTypes, ReqNonKeys and Generic:
            ldkeys = LTR_flatlist.Where(DI => DI.IsKeyType).ToList();                   // Keys
            ldreqs = LTR_flatlist.Where(DI => intreqs.Contains(DI.ItemID)).ToList();    // Declare reqs
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
                KEYID.NADALIA,
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
                int keyindex = RNG.Next(ld.Count);
                DropInfo item = ld[keyindex]; // get key to place
                PlaceItem(item, flag);
                ld.RemoveAt(keyindex);
            }
        }
        private void PlaceItem(DropInfo item, SetType stype)
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
                    KeysPlacedSoFar.Add(item.ItemID);

                rdz.AddShuffledItem(item);
                if (rdz.IsSaturated())
                {
                    var test = Unfilled.Remove(elnum); // now filled!
                    int debug = -1;
                }


                return;
            }
            throw new Exception("True Softlock, please investigate");
        }

        // To move somewhere else:
        private List<int> DefineRequiredItems()
        {
            // Add here / refactor as required.
            List<int> items = new List<int>()
            {
                40420000,   // Silvercat Ring
                5400000,    // Pyromancy Flame
                5410000,    // Dark Pyromancy Flame 
                60355000,   // Aged Feather
            };
            return items;
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
            // Order keys to find & output:
            var orderedkeys = Enum.GetValues(typeof(KEYID)).Cast<int>().OrderBy(i => i).ToArray();

            List<string> lines = new List<string>();
            foreach (var keyid in orderedkeys)
            {
                if (!TryGetItemName(keyid, out string itemname))
                    continue;

                var rdzsWithKey = Data.Where(rdz => rdz.HasShuffledItemID(keyid)).ToList();
                foreach (var rdz in rdzsWithKey)
                {
                    StringBuilder sb = new StringBuilder(itemname);
                    string desc = Logic.D[rdz.ParamID].Description;
                    sb.Append($": {desc}");
                    lines.Add(sb.ToString());
                }
            }

            // Write file:
            File.WriteAllLines("./keytesting.txt", lines.ToArray());
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
