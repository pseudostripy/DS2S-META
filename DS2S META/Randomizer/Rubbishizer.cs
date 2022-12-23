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
    /// Handle logic & data related to Rubbishizer
    /// </summary>
    internal class Rubbishizer
    {
        // Fields:
        private const int RUBBISH = 60510000;
        internal DropInfo DIOneRubbish = new()
        {
            ItemID = RUBBISH,
            Infusion = 0,
            Reinforcement = 0,
            Quantity = 1
        };
        internal DropInfo DIZeroRubbish = new()
        {
            ItemID = RUBBISH,
            Infusion = 0,
            Reinforcement = 0,
            Quantity = 0
        };

        private List<ItemLotRow> VanillaLots = new();
        private List<ItemLotRow> VanillaDrops = new();
        private List<ShopRow> VanillaShops = new();
        
        internal bool IsInitialized = false;
        internal bool IsRubbishized = false;
        
        

        // Constructors:
        internal Rubbishizer()
        {
        }

        // Main Methods:
        internal void Initalize()
        {
            GetVanillaDrops();
            GetVanillaLots();
            GetVanillaShops();
            IsInitialized = true;
        }
        
        internal void GetVanillaDrops()
        {
            var vanlotschr = ParamMan.ItemLotChrParam?.Rows.OfType<ItemLotRow>().ToList();
            if (vanlotschr == null) throw new Exception("Shouldn't get here");
            foreach (var droprow in vanlotschr) droprow.IsDropTable = true;
            VanillaDrops = vanlotschr;
        }
        internal void GetVanillaLots()
        {
            var vanlotsother = ParamMan.ItemLotOtherParam?.Rows.OfType<ItemLotRow>().ToList();
            if (vanlotsother == null) throw new NullReferenceException("Shouldn't get here");
            VanillaLots = vanlotsother;
        }
        internal void GetVanillaShops()
        {
            var vanshops = ParamMan.ShopLineupParam?.Rows.OfType<ShopRow>().ToList();
            if (vanshops == null) throw new NullReferenceException("Shouldn't get here");
            VanillaShops = vanshops;
        }


        internal void Rubbishize()
        {
            if (!IsInitialized)
                Initalize();

            // Place sets of items:
            DoLots();
            DoDrops();
            DoShops();
            DoCharCreation();
            DoStartingGear();

            // Rubbishize Game!
            WriteParams();
            IsRubbishized = true;
        }
        //internal async Task Rubbishize()
        //{
        //    // Place sets of items:
        //    DoLots();
        //    DoDrops();
        //    DoShops();

        //    // Rubbishize Game!
        //    await Task.Run(() => WriteAllLots());
        //    await Task.Run(() => WriteAllDrops());
        //    await Task.Run(() => WriteAllShops());
        //    IsRubbishized = true;
        //}
        internal void Unrubbishize()
        {
            if (ParamMan.ShopLineupParam == null || ParamMan.ItemLotOtherParam == null || ParamMan.ItemParam == null)
                throw new Exception("Param tables are null");

            // Restore all the param tables we changed:
            ParamMan.ShopLineupParam?.RestoreParam();
            ParamMan.ItemLotOtherParam?.RestoreParam();
            ParamMan.ItemLotChrParam?.RestoreParam();
            ParamMan.PlayerStatusClassParam?.RestoreParam();
            ParamMan.PlayerStatusItemParam?.RestoreParam();
            IsRubbishized = false;
        }

        private void DoLots()
        {
            // Make every item rubbish:
            foreach (var lotrow in VanillaLots)
            {
                if (lotrow.ID == 10256160) // Gulch Fragrant Branch
                    continue;
                if (lotrow.ID == 1787000) // Ancient Dragon Gift
                    continue;

                // Make everything  else rubbish!
                for (int i = 0; i < lotrow.NumDrops; i++)
                    lotrow.SetDrop(DIOneRubbish, i);

            }
        }
        private void DoDrops()
        {
            // Make every item rubbish:
            foreach (var droprow in VanillaDrops)
            {
                for (int i = 0; i < droprow.NumDrops; i++)
                    droprow.SetDrop(DIOneRubbish, i);
            }
        }
        private void DoShops()
        {
            // Make every item rubbish:
            foreach (var shop in VanillaShops)
                shop.ItemID = RUBBISH;
        }
        private void DoCharCreation()
        {
            // Tidy this up at some point
            var classids = new List<int>() { 20, 30, 50, 70, 80, 90, 100, 110 }; // Warrior --> Deprived
            var classrows = ParamMan.PlayerStatusClassParam?.Rows.Where(row => classids.Contains(row.ID))
                                                            .OfType<PlayerStatusClassRow>().ToList();
            if (classrows == null) throw new Exception("Failed to find classes in param");
            var classitemnums = new List<int>() { 1, 1, 1, 1, 1, 7, 1, 0 };
            
            // Main randomizing loop for each class
            for (var iclass = 0; iclass < classids.Count; iclass++)
            {
                // Delete the defaults:
                var classrow = classrows[iclass];
                classrow.Wipe();

                // Class Items:
                var numItems = classitemnums[iclass];
                
                for (int i = 0; i < numItems; i++)
                {
                    classrow.WriteAtItemArray(i, RUBBISH);
                    classrow.WriteAtItemQuantArray(i, 1);
                }

                // Commit all changes to memory
                classrow.StoreRow();
            }

            var itemids = new List<int>() { 20, 30, 50, 70, 80, 90, 100, 110 }; // Warrior --> Deprived
            var itemrows = ParamMan.PlayerStatusClassParam?.Rows
                                  .OfType<PlayerStatusClassRow>()
                                  .Where(row => row.ID > 400 & row.ID < 1000)
                                  .ToList();
            if (itemrows == null) throw new Exception("can't find items in param");

            // Main randomizing loop for each class
            foreach (var gift in itemrows) 
            { 
            //for (var iclass = 0; iclass < classids.Count; iclass++)
            //{
                // Delete the defaults:
                var numtoset = gift.CountItems();
                gift.Wipe();

                for (int i = 0; i < numtoset; i++)
                {
                    gift.WriteAtItemArray(i, RUBBISH);
                    gift.WriteAtItemQuantArray(i, 1);
                }

                // Commit all changes to memory
                gift.StoreRow();
            }


        }
        private void DoStartingGear()
        {
            var statusitemrows = ParamMan.PlayerStatusItemParam?.Rows
                .OfType<PlayerStatusItemRow>()  
                .ToList();
            if (statusitemrows == null) throw new Exception("Didn't load the param table correctly");

            foreach (var sirow in statusitemrows)
                sirow.Wipe();
        }



        // Memory modification:
        internal void WriteParams()
        {
            WriteAllLots();
            WriteAllDrops();
            WriteAllShops();
            WriteCharacters();
            WriteStartingGear();
        }
        internal void WriteAllLots()
        {
            VanillaLots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotOtherParam?.WriteModifiedParam();
        }
        internal void WriteAllDrops()
        {
            VanillaDrops.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotChrParam?.WriteModifiedParam();
        }
        internal void WriteAllShops()
        {
            VanillaShops.ForEach(lot => lot.StoreRow());
            ParamMan.ShopLineupParam?.WriteModifiedParam();
        }
        internal void WriteCharacters()
        {
            ParamMan.PlayerStatusClassParam?.WriteModifiedParam();
        }
        internal void WriteStartingGear()
        {
            ParamMan.PlayerStatusItemParam?.WriteModifiedParam();
        }
    }
}
