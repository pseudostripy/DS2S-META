using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DS2S_META.Utils;
using DS2S_META.ViewModels;
using System.Security.Cryptography;
using DS2S_META.Randomizer.Placement;
using DS2S_META.Utils.ParamRows;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Handle logic & data related to Randomizer
    /// </summary>
    internal class RandomizerManager
    {
        // Fields:
        DS2SHook? Hook;
        
        internal bool IsInitialized = false;
        internal bool IsRandomized = false;

        
        internal int CurrSeed;
        internal List<ItemRestriction> UIRestrictions;
        internal List<Restriction> Restrictions;
        internal Presanitizer Scope;
        internal PlacementManager Placer;
        internal bool IsRaceMode { get; set; }
        
        
        // Constructors:
        internal RandomizerManager()
        {
            Rng.SetSeed(Environment.TickCount); // used for generate seed numbers
        }

        // Main Methods:
        internal void Initalize(DS2SHook hook)
        {
            Hook = hook; // Required for reading game params in memory
            Rng.SetSeed(0x10); // *always same seed for setup*. Seed is updated before selecting things for randomization
            Scope = new Presanitizer();
            IsInitialized = true;
        }
        

        
        // Core:
        internal async Task Randomize(int seed)
        {
            if (Hook == null)
                return;

            
            // Setup for re-randomization:
            if (!EnsureSeedCompatibility(seed)) return;
            SetSeed(seed);          // reset Rng Twister
            Scope.Reinitialize();
            SetupRestrictions();    // reload from UI

            // DoRandomize:
            Placer = new PlacementManager(Scope, Restrictions, IsRaceMode);
            Placer.Randomize();
            CharCreation.Randomize();

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
            ParamMan.PlayerStatusItemParam?.RestoreParam();
            ParamMan.ItemLotChrParam?.RestoreParam();

            // Force an area reload.
            Hook?.WarpLast();
            IsRandomized = false;
        }
        
        internal void SetupRestrictions()
        {
            // - Split restrictions and assign to associated arrays
            // - Choose one from a group of items for this seed
            var restrictions = new List<Restriction>(); // preallocate empty
                        
            // volatile;
            int itemid;
            MinMax minmax = new(0, 0) ;

            foreach( var irest in UIRestrictions)
            {
                itemid = irest.ItemIDs.RandomElement(); // choose each from available set
                
                switch (irest.RestrType)
                {
                    case RestrType.Anywhere:
                    case RestrType.Vanilla:
                        minmax = new MinMax(0, 0); // irrelevant
                        break; // no restriction

                    case RestrType.Distance:
                        minmax = new MinMax(irest.DistMin, irest.DistMax);
                        break;
                }
                var restr = new Restriction(irest.RestrType, itemid, minmax);
                restrictions.Add(restr);
            }

            // Store to class
            Restrictions = restrictions;
        }
        
        
        // Memory modification:
        internal static void WriteAllLots(List<ItemLotRow> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotOtherParam?.WriteModifiedParam();
        }
        internal static void WriteAllDrops(List<ItemDropRow> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotChrParam?.WriteModifiedParam();
        }
        internal static void WriteAllShops(List<ShopRow> all_shops)
        {
            all_shops.ForEach(SR => SR.StoreRow());
            ParamMan.ShopLineupParam?.WriteModifiedParam();
        }
        internal void WriteShuffledLots()
        {
            var shuffledlots = Scope.AllPtf.OfType<LotRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledDrops()
        {
            var shuffleddrops = Scope.AllPtf.OfType<DropRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllDrops(shuffleddrops);
        }
        internal void WriteShuffledShops()
        {
            var shuffledshops = Scope.AllPtf.OfType<ShopRdz>().Select(sdz => sdz.ShuffledShop).ToList();
            WriteAllShops(shuffledshops);
        }

        // Utility:
        internal void PrintKeysNeat()
        {
            // Prep:
            List<string> lines = new()
            {
                // Intro line
                $"Printing key locations for seed {CurrSeed}",
                "---------------------------------------------"
            };

            // Main print loop
            foreach (int keyid in ItemSetBase.KeyOutputOrder.Cast<int>())
            {
                var rdzsWithKey = Scope.AllPtf.Where(rdz => rdz.HasShuffledItemId(keyid)).ToList();
                foreach (var rdz in rdzsWithKey)
                {
                    var itemname = keyid.AsMetaName();
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
            List<string> lines = new()
            {
                // Intro line
                $"Printing items at all locations for seed {CurrSeed}",
                "---------------------------------------------",

                // World placements:
                "World placement:"
            };
            foreach (var ldz in Scope.AllPtf.OfType<LotRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Shops:
            lines.Add("");
            lines.Add("Shops:");
            foreach (var rdz in Scope.AllPtf.OfType<ShopRdz>())
                lines.Add(rdz.GetNeatDescription());

            // Enemy drops:
            lines.Add("");
            lines.Add("Enemy Drops:");
            foreach (var ldz in Scope.AllPtf.OfType<DropRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Write file:
            File.WriteAllLines("./all_answers.txt", lines.ToArray());
        }

        // Seed / CRC related        
        internal static bool GenCRCSeed(out int seed)
        {
            seed = 0;
            if (!GetRandoSettingsStr(out var strsett))
                return false;

            // Look for one that matches current settings hash checks
            var c = 0; // attempt count
            while (c < 100000)
            {
                seed = Rng.Next();
                string fullpayload = strsett + seed.ToString();
                var sha = ComputeSHA256(fullpayload);
                string shaend = sha[^CRC.Length..];
                if (shaend == CRC)
                    return true;
                c++;
            }
            throw new Exception("Either you're exceptionally unlucky, or theres a bug in the SHA256 CRC code");
        }
        internal void SetSeed(int seed)
        {
            CurrSeed = seed;
            Rng.SetSeed(seed); // reinitialize
        }
        internal static bool GetRandoSettingsStr(out string xmlstr)
        {
            xmlstr = string.Empty;
            var path = RandoSettingsViewModel.SettingsFilePath;
            if (!File.Exists(path))
                return false;

            xmlstr = File.ReadAllText(path);
            return true;
        }
        internal static string CRC = "AA";
        internal static bool EnsureSeedCompatibility(int seed)
        {
            // do a CRC on the settings to make sure that it aligns.
            if (!GetRandoSettingsStr(out var strsett))
                return CRCOverrideQuestion();

            // Check SHA combo
            string fullpayload = strsett += seed.ToString();
            var sha = ComputeSHA256(fullpayload);
            string shaend = sha[^CRC.Length..];
            bool crccheck = shaend == CRC;
            if (crccheck)
                return true; // no issues
            return CRCOverrideQuestion();
        }
        private static bool CRCOverrideQuestion()
        {
            var x = System.Windows.Application.Current.Dispatcher.Invoke(WaitForAnswer);
            return x;
        }
        internal static bool WaitForAnswer()
        {
            var seedwarn = new RandoSeedWarning()
            {
                Title = "Seed/Settings Mismatch",
                Width = 375,
                Height = 200,
            };
            seedwarn.ShowDialog();
            return seedwarn.IsOk;
        }
        public static string ComputeSHA256(string s)
        {
            // thanks internet
            string hash = string.Empty;

            // Initialize a SHA256 hash object
            using (SHA256 sha256 = SHA256.Create())
            {
                // Compute the hash of the given string
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

                // Convert the byte array to string format
                foreach (byte b in hashValue)
                {
                    hash += $"{b:X2}";
                }
            }
            return hash;
        }
    }
}
