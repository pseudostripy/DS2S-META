using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using DS2S_META.Resources.Randomizer;
using System.IO;

namespace DS2S_META
{
    /// <summary>
    /// Randomizer Code & Front End for RandomizerControl.xaml
    /// </summary>
    public partial class RandomizerControl : METAControl
    {
        // Fields:
        private Color PURPLE = Color.FromArgb(0xFF, 0xB1, 0x59, 0xCC);
        private Color LIGHTRED = Color.FromArgb(0xFF, 0xDA, 0x4D, 0x4D);
        private Color LIGHTGREEN = Color.FromArgb(0xFF, 0x87, 0xCC, 0x59);
        private Random RNG = new Random();
        private Dictionary<int, ItemLot> VanillaLots;
        private Dictionary<int, ShopInfo> VanillaShops;
        private Dictionary<int, int> VanillaItemPrices;
        internal RandoDicts RD = new RandoDicts();
        internal bool isRandomized = false;

        // For Gaussians:
        private const double priceMean = 3000;
        private const double priceSD = 500;
        // For Gamma distribution
        internal const double priceShapeK = 3.0;
        internal const double priceScaleTh = 2.0;

        // FrontEnd:
        public RandomizerControl()
        {
            InitializeComponent();
        }
        private void FixSeedVisibility()
        {
            //Handles the "Seeed..." label on the text box
            if (txtSeed.Text == "")
                lblSeed.Visibility = Visibility.Visible;
            else
                lblSeed.Visibility = Visibility.Hidden;

        }
        private void txtSeed_TextChanged(object sender, TextChangedEventArgs e)
        {
            FixSeedVisibility();
        }
        private void btnRandomize_Click(object sender, RoutedEventArgs e)
        {
            // Want to try to Hook in DS2 to change the wooden chest above cardinal tower to metal chest items:
            if (!Hook.Hooked)
            {
                MsgMissingDS2();
                return;
            }

            // Read normal game params:
            if (VanillaLots == null)
            {
                VanillaLots = Hook.GetVanillaLots();
                VanillaShops = Hook.GetVanillaShops();
                VanillaItemPrices = Hook.GetVanillaItemPrices();
            }

            
            if (isRandomized)
                Unrandomize();
            else
                Randomize();

            // Force an area reload. TODO add warning:
            Hook.WarpLast();

            // Update UI:
            btnRandomize.Content = isRandomized ? "Unrandomize!" : "Randomize!";
            Color bkg = isRandomized ? PURPLE : LIGHTGREEN;
            lblGameRandomization.Background = new SolidColorBrush(bkg);
            string gamestate = isRandomized ? "Randomized" : "Normal";
            lblGameRandomization.Content = $"Game is {gamestate}!";
        }
        private void MsgMissingDS2()
        {
            MessageBox.Show("Please open Dark Souls 2 first.");
            return;
        }

        // Entry Point Randomizer Code:
        private void Randomize()
        {
            // Setup for re-randomization:
            int seed = Convert.ToInt32(txtSeed.Text);
            RNG = new Random(seed); // reset Rng Twister
            RD = new RandoDicts();  // reset dictionaries

            // Need to get a list of the vanilla item lots C#.8 pleeeease :(
            ItemSetBase PTF = new CasualItemSet(); // Places To Fill
            var shopplaces = ConvertShopToSlots(); // Update with shop options to fill
            PTF.AppendMorePlaces(shopplaces); 

            // Get Loot to randomize:
            FixVanillaLots(PTF);
            var flatlist = VanillaLots.SelectMany(kvp => kvp.Value.Lot).ToList(); // flatlist of all drop options
            var flatshops = ConvertShopToDrops(); // flatlist of "admissible" shop params
            var LTR = flatlist.Concat(flatshops); // Loot To Randomize
            

            // Partition into KeyTypes, ReqNonKeys and Generic:
            var allkeys = LTR.Where(DI => Enum.IsDefined(typeof(KEYID), DI.ItemID)).ToList();   // Keys
            var LTR1 = LTR.Where(DI => !Enum.IsDefined(typeof(KEYID), DI.ItemID));              // All but keys
            var reqlist = DefineRequiredItems();                                                // Declare reqs
            var allreq = LTR1.Where(DI => reqlist.Contains(DI.ItemID)).ToList();                // Prepare reqs
            var LTR2 = LTR1.Where(DI => !reqlist.Contains(DI.ItemID));                          // All but keys, reqs
            var allgen = LTR2.ToList();                                                         // Generics

            
            // Get lists of places where items can go (removing options based on settings)
            var BanKeyTypes = new List<PICKUPTYPE>()
            {
                PICKUPTYPE.NPC,
                PICKUPTYPE.VOLATILE,
                PICKUPTYPE.EXOTIC,
                PICKUPTYPE.COVENANTEASY,
                PICKUPTYPE.COVENANTHARD,
                PICKUPTYPE.UNRESOLVED,
                PICKUPTYPE.REMOVED,
                PICKUPTYPE.NGPLUS,
                PICKUPTYPE.CRAMMED,
                PICKUPTYPE.SHOP, // For now
            };
            var BanGeneralTypes = new List<PICKUPTYPE>()
            {
                PICKUPTYPE.EXOTIC,
                PICKUPTYPE.COVENANTHARD, // To split into cheap/annoying
                PICKUPTYPE.UNRESOLVED,
                PICKUPTYPE.REMOVED,
                PICKUPTYPE.NGPLUS,
                PICKUPTYPE.CRAMMED,
            };
            RD.ValidKeyPlaces = PTF.RemoveBannedTypes(BanKeyTypes);
            RD.ValidGenPlaces = PTF.RemoveBannedTypes(BanGeneralTypes);


            // testing
            var test = LTR.ToArray().Where(di => di.ItemID == 52400000).ToArray();
            var testsoldier = LTR.ToArray().Where(di => di.ItemID == (int)KEYID.SOLDIER).ToArray();
            var testsoldier2 = allkeys.ToArray().Where(di => di.ItemID == (int)KEYID.SOLDIER).ToArray();

            // Place all keys:
            allkeys = RemoveDuplicateKeys(allkeys); // avoid double ashen mist etc.
            var keysrem = new List<DropInfo>(allkeys);                              // clone
            RD.RemKeyPlaces = new Dictionary<int, RandoInfo>(RD.ValidKeyPlaces);    // clone


            var testsoldier3 = keysrem.ToArray().Where(di => di.ItemID == (int)KEYID.SOLDIER).ToArray();

            while (keysrem.Count > 0)
            {
                int keyindex = RNG.Next(keysrem.Count);
                DropInfo item = keysrem[keyindex]; // get key to place
                PlaceKeyItem(item, RD);
                keysrem.RemoveAt(keyindex);
            }

            // Printout the current shuffled lots:
            List<string> lines = new List<string>();
            foreach (var kvp in RD.ShuffledLots)
            {
                int paramid = kvp.Key;
                ItemLot IL = kvp.Value;
                var ri = PTF.D[kvp.Key];
                lines.Add($"{paramid} : {ri.Description} : {IL}");
            }
            File.WriteAllLines("./keytesting.txt", lines.ToArray());

            // Place all non-key, required items:
            RD.RemGenPlaces = new Dictionary<int, RandoInfo>(RD.ValidGenPlaces);    // clone
            RD.RemGenPlaces = RD.RemGenPlaces.Where(kvp => !RD.ShuffledLots.ContainsKey(kvp.Key)) // remove key-filled places
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            while (allreq.Count > 0)
            {
                int index = RNG.Next(allreq.Count);
                DropInfo item = allreq[index]; // get item to place
                PlaceGenericItem(item, RD);
                allreq.RemoveAt(index);
            }

            

            // Place everything else:
            while (RD.RemGenPlaces.Count > 0)
            {
                if (allgen.Count == 0)
                    throw new Exception("Ran out of items, please code empties");

                int index = RNG.Next(RD.RemGenPlaces.Count);
                DropInfo item = allgen[index]; // get item to place
                PlaceGenericItem(item, RD);
                allgen.RemoveAt(index);
            }

            // Randomize Game!
            Hook.WriteAllLots(RD.ShuffledLots);
            Hook.WriteAllShops(RD.ShuffledShops, RD.ShuffledPrices);
            isRandomized = true;
        }
        private void Unrandomize()
        {
            //var timer = new Stopwatch();
            //timer.Start();
            Hook.WriteAllLots(VanillaLots);
            Hook.WriteAllShops(VanillaShops, VanillaItemPrices);
            isRandomized = false;

            //timer.Stop();
            //Console.WriteLine($"Execution time: {timer.Elapsed.TotalSeconds} ms");
        }

        // Utility methods:
        private List<DropInfo> ConvertShopToDrops()
        {
            List<DropInfo> shopdrops = new List<DropInfo>();
            var ShopRules = new ShopRules(); // Maybe static class?
            var shopexcl = ShopRules.DefineExclusions();

            foreach(var kvp in VanillaShops)
            {
                // Remove duplicates and missing content:
                if (shopexcl.Contains(kvp.Key))
                    continue;

                ShopInfo SI = new ShopInfo(kvp.Value); // clone new ref
                SI.Quantity = GetAdjustedQuantity(SI);
                //SI.BasePrice = VanillaItemPrices[SI.ItemID]; // Do we need this here?

                DropInfo DI = new DropInfo(SI.ItemID, SI.Quantity, 0, 0);
                shopdrops.Add(DI);
            }
            return shopdrops;
        }
        private Dictionary<int, RandoInfo> ConvertShopToSlots()
        {
            Dictionary<int, RandoInfo> shopslots = new Dictionary<int, RandoInfo>();
            var ShopRules = new ShopRules(); // Maybe static class?
            var shopexcl = ShopRules.DefineExclusions();

            foreach (var kvp in VanillaShops)
            {
                // Remove duplicates and missing content:
                if (shopexcl.Contains(kvp.Key))
                    continue;

                RandoInfo RI = new RandoInfo("TODO Link Shop Description", PICKUPTYPE.SHOP);
                shopslots.Add(kvp.Key, RI);
            }
            return shopslots;
        }
        internal int GetAdjustedQuantity(ShopInfo SI)
        {
            var itype = Hook.GetItemType(SI.ItemID);
            switch (SI.RawQuantity)
            {
                // Adjust these weights if required
                case 255:
                    return itype == DS2SHook.ItemType.CONSUMABLE ? 5 : 1;

                case 10:
                    return 3;

                default:
                    return SI.RawQuantity;
            }
        }
        private void FixVanillaLots(ItemSetBase pinfo)
        {
            // Remove lots which are all empty
            VanillaLots = VanillaLots.Where(kvp => kvp.Value.NumDrops != 0)
                             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Remove lots from params we don't like:
            var BanLootTypes = new List<PICKUPTYPE>() { PICKUPTYPE.CRAMMED, PICKUPTYPE.UNRESOLVED, PICKUPTYPE.REMOVED }; // Don't even include these in the randomization
            VanillaLots = VanillaLots.Where(kvp => !pinfo.D[kvp.Key].HasType(BanLootTypes))
                                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        private List<DropInfo> RemoveDuplicateKeys(List<DropInfo> allkeys)
        {
            // First select things which are allowed to be dupes:
            var okdupes = new List<KEYID>() { KEYID.TORCH, KEYID.PHARROSLOCKSTONE, KEYID.FRAGRANTBRANCH, KEYID.SOULOFAGIANT, KEYID.SMELTERWEDGE, KEYID.FLAMEBUTTERFLY };
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
        private void PlaceKeyItem(DropInfo item, RandoDicts RD)
        {
            bool keyPlaced = false;
            RD.SoftlockSpots = new Dictionary<int, RandoInfo>(); // resets each time a key is placed
            while (!keyPlaced)
            {
                int Nsr = RD.RemKeyPlaces.Count();
                if (Nsr == 0)
                    break; // hits exception below

                // Choose random place for key:
                int pindex = RNG.Next(Nsr);
                var place = RD.RemKeyPlaces.ElementAt(pindex);

                // Check viability:
                HandleSoftlockCheck(out bool isSoftLocked, place, RD);
                if (isSoftLocked)
                    continue; // handled inside function

                // Accept solution:
                RD.PlacedSoFar.Add(item.ItemID);
                AddLots_UpdateRD(RD, place, item, true);
                if (IsPlaceSaturated(RD.ShuffledLots, place.Key))
                    RD.RemKeyPlaces.Remove(place.Key);

                // Prepare for next item:
                keyPlaced = true;
                foreach (var kvp in RD.SoftlockSpots)
                    RD.RemKeyPlaces.Add(kvp.Key, kvp.Value);
            }

            if (!keyPlaced)
                throw new Exception("True Softlock");
        }
        private void PlaceGenericItem(DropInfo item, RandoDicts RD)
        {
            // Item should be able to go into the remGenPlaces spots without issue
            int Nsr = RD.RemGenPlaces.Count();
            if (Nsr == 0)
                throw new Exception("Shouldn't be reaching this situation");

            // Choose random place for item:
            int pindex = RNG.Next(Nsr);
            var place = RD.RemGenPlaces.ElementAt(pindex);

            if (place.Value.HasType(PICKUPTYPE.SHOP))
                AddShops_UpdateRD(RD, place.Key, item);
            else
                AddLots_UpdateRD(RD, place, item);
        }
        private void HandleSoftlockCheck(out bool isSoftLocked, KeyValuePair<int, RandoInfo> place, RandoDicts RD)
        {
            isSoftLocked = CheckIsSoftlockPlacement(place, RD.PlacedSoFar);
            if (!isSoftLocked)
                return;

            // This place is not valid for the current item.
            // Store it in temp array and restore later for next item checks.
            // This is for performance to avoid excessively drawing a bad place.
            RD.SoftlockSpots[place.Key] = place.Value;
            RD.RemKeyPlaces.Remove(place.Key);
        }
        private bool CheckIsSoftlockPlacement(KeyValuePair<int, RandoInfo> place, List<int> placedSoFar)
        {
            // Can only place item in slot if the keyconditions have been met
            var keysets = place.Value.KeySet;

            // Try each different option for key requirements
            foreach (var keyset in keysets)
            {
                if (keyset.Keys.All(kid => IsPlaced(kid, placedSoFar)))
                    return false; // NOT SOFT LOCKED all required keys are placed for at least one Keyset
            }
            return true;
        }
        private bool IsPlaced(KEYID kid, List<int> placedSoFar)
        {
            // Function to handle different checks depending on KeyTypes I guess:
            switch (kid)
            {
                case KEYID.NONE:
                    // no condition required:
                    return true;

                case KEYID.BELFRYLUNA:
                    // Branch && Pharros Lockstone x2
                    return condLuna();

                case KEYID.SINNERSRISE:
                    // Branch || Antiquated
                    return condSinner();

                case KEYID.DRANGLEIC:
                    // Branch && Rotunda && Sinner's Rise
                    return condDrangleic();

                case KEYID.AMANA:
                    // Drangleic && King's passage
                    return condAmana();

                case KEYID.ALDIASKEEP:
                    // Branch && King's Ring
                    return condAldias();

                case KEYID.MEMORYJEIGH:
                    // King's Ring && Ashen Mist
                    return condJeigh();

                case KEYID.GANKSQUAD:
                    // DLC1 && Eternal Sanctum
                    return condGankSquad();

                case KEYID.PUZZLINGSWORD:
                    // DLC1 (TODO Bow/Arrow as keys)
                    return condDLC1();

                case KEYID.ELANA:
                    // DLC1 && Dragon Stone
                    return condElana();

                case KEYID.FUME:
                    // DLC2 && Scorching Sceptor
                    return condFume();

                case KEYID.BLUESMELTER:
                    // DLC2 && Tower Key
                    return condBlueSmelter();

                case KEYID.ALONNE:
                    // DLC2 && Tower Key && Scorching Scepter && Ashen Mist
                    return condAlonne();

                case KEYID.DLC3:
                    // DLC3key && Drangleic
                    return condDLC3();

                case KEYID.FRIGIDOUTSKIRTS:
                    // DLC3 && Garrison Ward Key
                    return condFrigid();

                case KEYID.CREDITS:
                    // Drangleic & Memory of Jeigh
                    return condCredits();

                case KEYID.VENDRICK:
                    // Amana + SoaG x3
                    return condVendrick();

                case KEYID.BRANCH:
                    // Three branches available
                    return condBranch();

                case KEYID.TENBRANCHLOCK:
                    // Ten branches available
                    return condTenBranch();

                case KEYID.NADALIA:
                    // DLC2 && Scepter && Tower Key && 12x Smelter Wedge
                    return condNadalia();

                case KEYID.PHARROS:
                    // Eight Pharros lockstones available
                    return condPharros();

                case KEYID.BELFRYSOL:
                    // Rotunda Lockstone && Pharros Lockstone x2
                    return condSol();

                case KEYID.DARKLURKER:
                    // Drangleic && Forgotten key && Torch && Butterfly x3
                    return condDarklurker();

                default:
                    return condKey(kid); // Simple Key check
            }

            // Conditions wrappers:
            int countBranches() => placedSoFar.Where(i => i == (int)KEYID.FRAGRANTBRANCH).Count();
            int countPharros() => placedSoFar.Where(i => i == (int)KEYID.PHARROSLOCKSTONE).Count();
            bool condKey(KEYID keyid) => placedSoFar.Contains((int)keyid);
            bool condBranch() => countBranches() >= 3;
            bool condTenBranch() => countBranches() >= 10;
            bool condRotunda() => condKey(KEYID.ROTUNDA);
            bool condAshen() => condKey(KEYID.ASHENMIST);
            bool condKingsRing() => condKey(KEYID.KINGSRING);
            bool condDLC1() => condKey(KEYID.DLC1);
            bool condDLC2() => condKey(KEYID.DLC2);
            bool condSinner() => condBranch() || condKey(KEYID.ANTIQUATED);
            bool condDrangleic() => condBranch() && condRotunda() && condSinner();
            bool condAmana() => condDrangleic() && condKey(KEYID.KINGSPASSAGE);
            bool condAldias() => condBranch() && condKingsRing();
            bool condJeigh() => condAshen() && condKingsRing();
            bool condGankSquad() => condDLC1() && condKey(KEYID.ETERNALSANCTUM);
            bool condElana() => condDLC1() && condKey(KEYID.DRAGONSTONE);
            bool condFume() => condDLC2() && condKey(KEYID.SCEPTER);
            bool condBlueSmelter() => condDLC2() && condKey(KEYID.TOWER);
            bool condAlonne() => condDLC2() && condKey(KEYID.TOWER) && condKey(KEYID.SCEPTER) && condAshen();
            bool condDLC3() => condKey(KEYID.DLC3KEY) && condDrangleic();
            bool condFrigid() => condDLC3() && condKey(KEYID.GARRISONWARD);
            bool condCredits() => condDrangleic() && condJeigh();
            bool condWedges() => placedSoFar.Where(i => i == (int)KEYID.SMELTERWEDGE).Count() == 12;
            bool condNadalia() => condFume() && condBlueSmelter() && condWedges();
            bool condVendrick() => condAmana() && (placedSoFar.Where(i => i == (int)KEYID.SOULOFAGIANT).Count() >= 3);
            bool condBigPharros() => countPharros() >= 2;
            bool condPharros() => countPharros() >= 8; // surely enough
            bool condLuna() => condBranch() && condBigPharros();
            bool condSol() => condRotunda() && condBigPharros();
            bool condButterflies() => placedSoFar.Where(i => i == (int)KEYID.FLAMEBUTTERFLY).Count() >= 3;
            bool condDarklurker() => condDrangleic() && condKey(KEYID.FORGOTTEN) && condButterflies() && condKey(KEYID.TORCH);
        }
        private bool IsPlaceSaturated(Dictionary<int, ItemLot> shuflots, int placeid)
        {
            return shuflots[placeid].NumDrops == VanillaLots[placeid].NumDrops;
        }
        private void AddShops_UpdateRD(RandoDicts RD, int paramid, DropInfo item)
        {
            // Look up standard flags for this shop spot:
            ShopInfo SI = new ShopInfo(VanillaShops[paramid]); // clone object

            // Edit price / quantity:
            SI.ItemID = item.ItemID;
            SI.Quantity = item.Quantity;

            // Update dictionaries
            RD.ShuffledShops.Add(paramid, SI);
            RD.ShuffledPrices[SI.ItemID] = RandomGammaInt(3000);
            RD.RemGenPlaces.Remove(paramid); // Shops only have one slot which is now used
        }
        private void AddLots_UpdateRD(RandoDicts RD, KeyValuePair<int, RandoInfo> place, DropInfo item, bool keycall = false)
        {
            // Update Lots and remove availability of place once the lot is full wrt vanilla
            int pkey = place.Key;
            if (RD.ShuffledLots.ContainsKey(pkey))
                RD.ShuffledLots[pkey].AddDrop(item);
            else
                RD.ShuffledLots[pkey] = new ItemLot(item);

            if (keycall)
                return; // logic handled elsewhere

            if (IsPlaceSaturated(RD.ShuffledLots, pkey))
                RD.RemGenPlaces.Remove(place.Key);
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
        internal int RoundToFactorN(double val, int fac)
        {
            var nearestMultiple = Math.Round( (val / fac), MidpointRounding.AwayFromZero) * fac;
            return (int)nearestMultiple;
        }
        internal int RandomGammaInt(int wantedMean, int roundfac = 50, double scaleA = priceShapeK, double shapeTh = priceScaleTh)
        {
            // Wrapper to handle pre/post int manipulation for Gamma distribution
            double rvg = RandomGammaVariable(scaleA, shapeTh);

            double rvgmode = (scaleA - 1) * shapeTh; // gamma distribution property
            double rvgScaled = (rvg / rvgmode) * wantedMean;
            return RoundToFactorN(rvgScaled, roundfac);
        }
        internal double RandomGammaVariable(double shapeA, double scaleTh)
        {
            // https://www.cs.toronto.edu/~radford/csc2541.F04/gamma.html
            // Can code up a more efficient version if you want to go through the maths

            double scaleB = 1 / scaleTh; // Align notation
            int Na = (int)Math.Floor(shapeA);
            List<double> RVu = new List<double>(); // RandomVariables Uniform(0,1] distribution
            List<double> RVe = new List<double>(); // RandomVariables Exponential(1) distribution
            for(int i = 0; i < Na; i++)
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

        // Testing / Unused:
        private void TestPricesLottery()
        {
            // Produce a graph to look at our distribution
            List<int> testlist = new List<int>();
            int Nsamp = 1000;
            for(int i = 0; i <= Nsamp; i++)
            {
                testlist.Add(RandomGammaInt(3000));
            }

            // Too lazy to plot in C#, write to file instead:
            string[] lines = testlist.Select(i => $"{i}").ToArray();
            File.WriteAllLines("./prices_check.txt", lines);
        }
    }
}