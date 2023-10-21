using DS2S_META.Randomizer.Placement;
using DS2S_META.Utils;
using DS2S_META.Utils.ParamRows;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal enum RDZ_TASKTYPE
    {
        // Enum used to denote required further post-processing situations
        UNDEFINED,
        EXCLUDE,
        SHOPREMOVE,
        SHOPSUSTAIN,
        STANDARD,
        CROWS,
        FILL_BY_COPY,
        UNLOCKTRADE, // enable immediately
        FREETRADE,
        TRADE_SHOP_COPY,
        LINKEDSLAVE, // LotRdz version of fill_by_copy
    }

    /// <summary>
    /// This class defines the boundary of the Randomizer,
    /// removing bad Rdzs (item placements) and setting up appropriate 
    /// lists of valid items (DropInfos) to be included in Randomizer.
    /// 
    /// Likely need only be called once, but could change in future
    /// if adjustable UI settings affect reinitialization.
    /// </summary>
    internal class Presanitizer
    {
        // Sanitizer logic: inclusions/eclusions etc.
        internal static List<PICKUPTYPE> BanFromBeingRandomized = new()
        {
            // List of places where loot cannot come from:
            PICKUPTYPE.CRAMMED,
            PICKUPTYPE.UNRESOLVED,
            PICKUPTYPE.REMOVED,
        };
        internal List<PICKUPTYPE> BanFromLoot = new()
        {
            // List of places where loot cannot come from:
            PICKUPTYPE.CRAMMED,
            PICKUPTYPE.UNRESOLVED,
            PICKUPTYPE.REMOVED,
        };
        internal static List<int> CrowDuplicates = new()
        {
            // Prism: keep C loot:
            50000300, // B loot from prism
            50000301, // A loot from prism
            50000302, // S loot from prism

            // Small silky: keep B loot:
            50000001, // A loot from small silky
            50000002, // S loot from small silky
            50000003, // C loot from small silky

            // Silky: keep A loot
            50000100, // B loot from silky
            50000102, // S loot from silky
            50000103, // C loot from silky

            // Petrified: keep S loot
            50000200, // B loot from petrified
            50000201, // A loot from petrified
            50000203, // C loot from petrified
        };
        internal static List<eItemType> WepSpellsArmour = new() // toimprove
        { eItemType.WEAPON1,
          eItemType.WEAPON2,
          eItemType.HEADARMOUR,
          eItemType.CHESTARMOUR,
          eItemType.GAUNTLETS,
          eItemType.LEGARMOUR,
          eItemType.SPELLS,
        };
        internal static List<RDZ_TASKTYPE> ShopLootInclTasks = new()
        {
            RDZ_TASKTYPE.STANDARD,
            RDZ_TASKTYPE.SHOPSUSTAIN,
            RDZ_TASKTYPE.FREETRADE,
            RDZ_TASKTYPE.UNLOCKTRADE,
        };
        internal static Dictionary<int, int> AbnormalNgLinks = new() {
            // Key = NG plus ID, Value = assoc NG ID 
            { 675010, 675000 },         // Fume Knight
            { 0x393A6AE, 0x393A6A4 }    // Betwixt Pursuer
        }; 

        // Output fields:
        public List<Randomization> FullListRdz = new();
        internal List<Randomization> AllPtf = new();
        internal List<DropInfo> LTR_flatlist = new();

        // Constructor
        internal Presanitizer()
        {
            SetupAllPtf();          // AllPtf
            GetLootToRandomize();   // LTR_Flatlist
        }
        internal void Reinitialize()
        {
            foreach (var rdz in AllPtf)
                rdz.ResetShuffled();
        }

        // Core:
        internal void SetupAllPtf()
        {
            // "Places To Fill"
            var lotptfs = DefineLotRdzs();
            var dropptfs = DefineDropRdzs();
            var shopptfs = DefineShopRdzs();
            //
            FullListRdz.AddRange(lotptfs);
            FullListRdz.AddRange(dropptfs);
            FullListRdz.AddRange(shopptfs);
            //
            // All places relevant to Randomizer:
            AllPtf = FullListRdz.FilterByTaskType(RDZ_TASKTYPE.STANDARD, RDZ_TASKTYPE.UNLOCKTRADE,
                                                RDZ_TASKTYPE.FREETRADE, RDZ_TASKTYPE.SHOPSUSTAIN).ToList();
        }
        internal void GetLootToRandomize()
        {
            // Apply rules to choose loot for placement
            // Remove globally banned stuff:
            var potentialRdzs = AllPtf.FilterOutPickupType(BanFromLoot);

            // LOTS
            // Normal Lots:
            var ldzs = potentialRdzs.OfType<LotRdz>().ToList();
            var ldzsNgp = ldzs.FilterByPickupType(PICKUPTYPE.NGPLUS);
            var ldzsNg = ldzs.Except(ldzsNgp).ToList();

            // 
            var flNgLots = ldzsNg.SelectMany(ldz => ldz.Flatlist).ToList(); // keep all NG loot
            var flNgpLots = ldzsNgp.SelectMany(ldz => ldz.GetUniqueFlatlist(GetAssocNgItemlot(ldz, ldzsNg).Flatlist)).ToList(); // keep only unique ng+ loot

            // DROPS
            // Add single copy of armour/weapons arising from drop
            var rawflDrops = potentialRdzs.OfType<DropRdz>()                    // all droprdzs
                                    .SelectMany(ddz => ddz.Flatlist).ToList();  // all dropinfos from droprdzs
            //
            var test = ParamMan.ItemRows.ToList();
            //var test = rawflDrops.FilterByItemType(WepSpellsArmour).ToList();
            var flBalDrops = rawflDrops.FilterByItemType(WepSpellsArmour)       // only those with matching item type
                                    .DistinctBy(di => di.ItemID);               // keep only 1 of each item 
            var flOtherDrops = rawflDrops.FilterOutItemType(WepSpellsArmour);   // keep everything else as is

            // SHOPS
            // Only keep loot of shops that I'll be replacing (others are duplicates)
            var flShops = potentialRdzs.OfType<ShopRdz>().ToList().FilterByTaskType(ShopLootInclTasks).SelectMany(sdz => sdz.Flatlist).ToList();
            

            // Combine everything
            LTR_flatlist.AddRange(flNgLots);
            LTR_flatlist.AddRange(flNgpLots);
            LTR_flatlist.AddRange(flBalDrops);
            LTR_flatlist.AddRange(flOtherDrops);
            LTR_flatlist.AddRange(flShops);

            // Final Manual/Miscellaneous fixes
            FixFlatListBalance(); // ensure correct number of keys etc
        }

        // AllPtf categorized logic:
        internal static List<Randomization> DefineLotRdzs()
        {
            // Get copy of all VanillaLots
            List<LotRdz> all_lots = new(); // preallocate empty
            foreach (var lotrow in ParamMan.ItemLotOtherRows)
            {
                var ri = lotrow.ID.GetGlotRandoInfo(); // todo if ever extend CasualItemSet
                RDZ_TASKTYPE status = CalcLotRdzStatus(lotrow, ri);
                var ldz = new LotRdz(lotrow, ri, status); // create combined information
                all_lots.Add(ldz);
            }
            return all_lots.Cast<Randomization>().ToList();
        }
        internal static List<Randomization> DefineDropRdzs()
        {
            List<DropRdz> droprdzs = new(); // preallocate empty
            foreach (var droprow in  ParamMan.ItemLotChrRows)
            {
                RandoInfo ri = droprow.ID.GetDropRandoInfo(); // no lookup info available for drops
                if (droprow.IsGuaranteedDrops())
                {
                    var pulist = ri.PickupTypes.ToList();
                    pulist.Add(PICKUPTYPE.GUARANTEEDENEMYDROP);
                    ri.PickupTypes = pulist.ToArray();
                }
                    

                var status = CalcDropRdzStatus(droprow, ri); // all OK until further notice
                var sdz = new DropRdz(droprow, ri, status);  // create combined information
                droprdzs.Add(sdz);
            }
            return droprdzs.Cast<Randomization>().ToList();
        }
        internal static List<Randomization> DefineShopRdzs()
        {
            List<ShopRdz> shoprdzs = new(); // preallocate empty
            foreach (var shoprow in ParamMan.ShopLineupRows)
            {
                var ri = shoprow.ID.GetShopRandoInfo();
                var status = CalcShopRdzStatus(shoprow, ri); // inherit-only for now
                var sdz = new ShopRdz(shoprow, ri, status);  // create combined information
                shoprdzs.Add(sdz);
            }
            return shoprdzs.Cast<Randomization>().ToList();
        }
        private static RDZ_TASKTYPE CalcLotRdzStatus(ItemLotRow lotrow, RandoInfo ri)
        {
            // **Change logic in here as required**

            // Not interested in randomizing:
            if (lotrow.IsEmpty) 
                return RDZ_TASKTYPE.EXCLUDE;
            if (ri.HasType(BanFromBeingRandomized))
                return RDZ_TASKTYPE.EXCLUDE;
            if (CrowDuplicates.Contains(lotrow.ID))
                return RDZ_TASKTYPE.EXCLUDE;
           
            // keep underlying status
            return ri.RandoHandleType;
        }
        private static RDZ_TASKTYPE CalcDropRdzStatus(ItemDropRow droprow, RandoInfo ri)
        {
            if (droprow.IsEmpty)
                return RDZ_TASKTYPE.EXCLUDE;
            
            var excltypes = new List<PICKUPTYPE>() { PICKUPTYPE.BADENEMYDROP, PICKUPTYPE.BADREGISTDROP };
            if (ri.HasType(excltypes))
                return RDZ_TASKTYPE.EXCLUDE;

            return RDZ_TASKTYPE.STANDARD;
        }
        private static RDZ_TASKTYPE CalcShopRdzStatus(ShopRow shoprow, RandoInfo ri)
        {
            return ri.RandoHandleType; // until told otherwise
        }
        
        // Misc. helpers
        private static ItemLotRow GetAssocNgItemlot(LotRdz ngpldz, List<LotRdz> ldzsNg)
        {
            // Find the NG itemlot associated with the input NGplus itemlot.
            // This is used to remove duplicated items from loot table.
            int assocNGid;
            int ngplotid = ngpldz.VanillaLot.ID;
            if (AbnormalNgLinks.ContainsKey(ngplotid))
                assocNGid = AbnormalNgLinks[ngplotid];
            else
                 assocNGid = ngplotid / 10 * 10; // Round down to nearest 10

            return ldzsNg.Where(ldz => ldz.ParamID == assocNGid).First().VanillaLot;
        }
        internal void FixFlatListBalance()
        {
            // Ensure 5 SoaGs (game defines these weirdly)
            var soag = LTR_flatlist.FilterByItem(ITEMID.SOULOFAGIANT).First();
            LTR_flatlist.Add(soag);
            LTR_flatlist.Add(soag);

            // Duplication fixes and balance:
            LimitItems(ITEMID.ROTUNDALOCKSTONE, 1); // Single Rotunda Lockstone
            LimitItems(ITEMID.AGEDFEATHER, 1);      // Single Feather
            LimitItems(ITEMID.LADDERMINIATURE, 1);  // Single Ladder Miniature
            LimitItems(ITEMID.ASHENMIST, 1);        // Single Ashen Mist
            LimitItems(ITEMID.TOKENOFFIDELITY, 1);  // Single Token of Fidelity
            LimitItems(ITEMID.TOKENOFSPITE, 1);     // Single Token of Spite
            LimitItems(ITEMID.TORCH, 15);           // 15x Torch pickups in game
            LimitItems(ITEMID.HUMANEFFIGY, 40);     // 40x Effigy pickups in game
        }
        private void LimitItems(ITEMID itemid, int lim)
        {
            // Limits number of pickup locations (not their specific item count).
            // It does this at random from the avail pickups.
            var reldis = LTR_flatlist.FilterByItem(itemid); // relevant DropInfos
            var tokeep = reldis.RandomElements(lim);
            var toremove = reldis.Except(tokeep);
            LTR_flatlist.RemoveAll(toremove.Contains);
        }
    }
}
