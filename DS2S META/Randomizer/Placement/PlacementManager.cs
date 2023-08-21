using DS2S_META.Utils;
using DS2S_META.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using static DS2S_META.Randomizer.Diset;
using static DS2S_META.Randomizer.RandomizerManager;

namespace DS2S_META.Randomizer.Placement
{
    /// <summary>
    /// Couples with PlacementLogic to decide on how to distribute
    /// the items for the randomizer
    /// </summary>
    internal class PlacementManager
    {
        // Fields
        internal IEnumerable<Diset> Disets { get; set; }
        internal Presanitizer Scope { get; set; }
        internal Steiner Steiner { get; set; }
        internal List<ItemRestriction> Restrictions;

        private List<DropInfo> LTR => Scope.LTR_flatlist; // shorthand
        private List<Randomization> PTF => Scope.AllPtf;
        internal List<int> KeysPlacedSoFar = new();

        // Internal and Volatile!
        private static int MinElligDist;
        private static int MaxElligDist;
        private static Randomization? MinElligRdz;
        private static Randomization? MaxElligRdz;
        private static Randomization? RecentBestRdz;

        // Enums/definitions
        internal static List<PICKUPTYPE> FullySafeFlags = new()
        {
           PICKUPTYPE.NONVOLATILE,
           PICKUPTYPE.BOSS,
           PICKUPTYPE.METALCHEST,
         };
        internal static List<PICKUPTYPE> HalfSafe = new()
        {
           PICKUPTYPE.NONVOLATILE,
           PICKUPTYPE.BOSS,
           PICKUPTYPE.GUARANTEEDENEMYDROP,
           PICKUPTYPE.WOODCHEST,
           PICKUPTYPE.METALCHEST,
           PICKUPTYPE.SHOP, // not evshop though
         };
        internal static List<PICKUPTYPE> AllowedGenTypes = new()
        {
            PICKUPTYPE.ENEMYDROP,
            PICKUPTYPE.GUARANTEEDENEMYDROP,
            PICKUPTYPE.COVENANTEASY,
            PICKUPTYPE.NPC,
            PICKUPTYPE.WOODCHEST,
            PICKUPTYPE.METALCHEST,
            PICKUPTYPE.VOLATILE, // misc volatile
            PICKUPTYPE.NONVOLATILE, // this is basically corpse pickups now
            PICKUPTYPE.BOSS,
            PICKUPTYPE.NGPLUS,
            PICKUPTYPE.SHOP,
            PICKUPTYPE.EVSHOP,
        };
        internal static Dictionary<int, List<PICKUPTYPE>> ManuallyRequiredItemsTypeRules = new()
        {
            // Add here / refactor as required.
            //{ 60155000, FullySafeFlags },    // Estus Flask
            { 0x039B89C8, FullySafeFlags },  // Estus Flask Shard
            { 0x039B8DB0, FullySafeFlags },  // Sublime Bone Dust
            //{ 05400000, FullySafeFlags },    // Pyromancy Flame
            //{ 05410000, FullySafeFlags },    // Dark Pyromancy Flame 
            //{ 60355000, FullySafeFlags },    // Aged Feather
            //{ 40420000, FullySafeFlags },    // Silvercat Ring
        };
        private static readonly Dictionary<ITEMID, IEnumerable<int>> MultiKeyTriggers = new()
        {
            { ITEMID.TORCH,              IntArray(3) },
            { ITEMID.SOULOFAGIANT,       IntArray(3) },
            { ITEMID.NADALIAFRAGMENT,    IntArray(11) },
            { ITEMID.PHARROSLOCKSTONE,   IntArray(2, 10) },
            { ITEMID.FRAGRANTBRANCH,     IntArray(3, 10) },
            { ITEMID.SMELTERWEDGE,       IntArray(11) },
        };
        private static IEnumerable<int> IntArray(params int[] values) { return  values; }
        internal List<Randomization> RdzMajors;

        // Constructor
        public PlacementManager(Presanitizer scope, List<ItemRestriction> uiRestrictions) 
        {
            Scope = scope;
            Restrictions = uiRestrictions;
            CreateDisets();
            CreateRdzMajors();
            Steiner = new Steiner(this, scope);
        }

        // Setup
        internal void CreateDisets()
        {
            // Take a copy of the FlatList so we don't need to regenerate everytime:
            var flcopy = LTR.Select(di => di.Clone()).ToList();

            // Partition into KeyTypes, ReqNonKeys and Generic Loot-To-Randomize:
            var keys = flcopy.Where(di => di.IsKeyType).ToList();                                  // Keys
            var flNoKeys = flcopy.Except(keys);
            var reqs = flNoKeys.Where(di => di.IsReqType || IsRestrictedItem(di.ItemID)).ToList(); // Reqs
            var gens = flNoKeys.Except(reqs).ToList();                                           // Generics

            // Create group objects for placement
            Disets = new List<Diset>()
            {
                Diset.FromKeys(keys),
                Diset.FromReqs(reqs),
                Diset.FromGens(gens),
            };
        }
        private bool IsRestrictedItem(int itemid) => Restrictions.Any(rs => rs.ItemID == itemid);
        private bool IsVanillaRestricted(int itemid) => Restrictions.Where(rs => rs.IsVanillaType).Any(rs => rs.ItemID != itemid);

        // Core
        internal void Randomize()
        {
            PlaceSets();
            FillLeftovers();

            // Sanity checks
            if (PTF.Where(rdz => !rdz.IsHandled).Any()) throw new Exception("Something not completed");
        }
        internal void PlaceSets()
        {
            // reset ongoing lists
            KeysPlacedSoFar = new();

            // Place everything
            foreach (var diset in Disets)
                PlaceSet(diset);
        }
        private void FillLeftovers()
        {
            // ld: list of DropInfos
            int Nfc = ItemSetBase.FillerItems.Count; // fill count
            foreach (var rdz in PTF)
            {
                while (!rdz.IsSaturated())
                {
                    int ind = Rng.Next(Nfc);
                    DropInfo item = ItemSetBase.FillerItems[ind]; // get filler item
                    rdz.AddShuffledItem(item);
                }
                rdz.MarkHandled();
            }
        }
        private void PlaceSet(Diset diset)
        {
            // Slightly extra logic when diset.IsKeys is true. See updatefornewkey

            // Get fresh copies
            var ld = new List<DropInfo>(diset.Data.ToList()); // ld: list of DropInfos
            var availrdzs = GetRemainingRdz();

            while (ld.Any())
            {
                // Draw next thing to place
                var di = ld.RandomElement();

                // Special case logic (removes from avail after completion)
                if (HandledAsVanilla(di, availrdzs, diset.IsKeys)) 
                    continue;

                // Normal key logic:
                var rdz = FindElligibleRdz(di, diset.Type, availrdzs, out var placeres);

                // Handle early placement failure to distance checks:
                bool distfail = placeres?.IsDistanceSoftFail == true;
                if (distfail && diset.IsKeys && ld.Any(di => !IsRestrictedItem(di.ItemID)))
                    continue; // try to delay until no choices left

                // Solution found (or compromise reached)
                PlaceIt(rdz, di, availrdzs, diset.IsKeys);
            }
        }
        //
        private bool HandledAsVanilla(DropInfo di, List<Randomization> availrdzs, bool iskey)
        {
            if (!IsVanillaRestricted(di.ItemID))
                return false;

            var rdzs = availrdzs.FilterByVanillaItem(di.ItemID);
            bool canplaceall = rdzs.All(r => CanPlaceVanillaKey(r, di));

            // This is the "DELAY_VANLOCKED" situation. Kept in list for future reattempt.
            if (!canplaceall)
                return true;

            // Can place key in (all its) Vanilla locations
            foreach (var r in rdzs)
                PlaceIt(r, di, availrdzs, iskey); // remove from availrdz
            return true;
        }
        private List<Randomization> GetRemainingRdz()
        {
            // see what's left and get a copy of that list
            return new List<Randomization>(PTF.Where(rdz => !rdz.IsHandled));
        }
        private void PlaceIt(Randomization rdz, DropInfo di, List<Randomization> availrdz, bool iskey)
        {
            if (iskey) PlaceKey(rdz, di, availrdz);
            else PlaceItem(rdz, di, availrdz);
        }
        private void PlaceKey(Randomization rdz, DropInfo di, List<Randomization> availrdz)
        {
            PlaceItem(rdz, di, availrdz);
            UpdateForNewKey(rdz, (ITEMID)di.ItemID); // update softlock stuff
        }
        private static void PlaceItem(Randomization rdz, DropInfo di, List<Randomization> availrdz)
        {
            // "Fix di, add di to rdz, remove rdz from availrdz if it saturates"
            FixInfusReinf(di);
            rdz.AddShuffledItem(di);
            if (!rdz.IsSaturated())
                return;

            // rdz complete:
            rdz.MarkHandled();
            availrdz.Remove(rdz);
        }
        //
        private static void FixInfusReinf(DropInfo di)
        {
            di.Reinforcement = GetFixedReinforcement(di);
            di.Infusion = GetFixedInfusion(di);
        }
        private static byte GetFixedInfusion(DropInfo di)
        {
            var item = di.ItemID.AsItemRow();

            // non weapons:
            if (!item.IsWeaponType)
                return 0; // uninfusable

            // weapons:
            var infusionOptions = item.GetInfusionList();
            if (!infusionOptions.Any(ds2I => ds2I.ID == di.Infusion))
                return 0; // Don't allow a "new" infusion
            return di.Infusion; // fine to keep original
        }
        private static byte GetFixedReinforcement(DropInfo di)
        {
            var item = di.AsItemRow();
            var maxupgrade = GetItemMaxUpgrade(item);
            return (byte)Math.Min(di.Reinforcement, maxupgrade); // limit to item max upgrade
        }

        // Elligibility results
        private Randomization FindElligibleRdz(DropInfo di, SetType stype, List<Randomization> availrdzs, out PlaceResult? bestPlaceRes)
        {
            // Find an Rdz (at random) satisfying all constraints.
            bestPlaceRes = null;
            ResetMinMaxElligible();

            // List of remaining spots to search through
            var rdzs = new List<Randomization>(availrdzs); // take a copy

            while (rdzs.Any())
            {
                // Choose random rdz for item:
                var rdz = rdzs.RandomElement();
                rdzs.Remove(rdz); // don't choose again

                // Is is a good rdz to use?
                var placeres = GetPlacementResult(rdz, di, stype);

                // Bad rdz: keep trying
                if (placeres.IsHardFail())
                    continue;

                // Broke the distance restrictions but otherwise fine.
                if (placeres.IsDistanceSoftFail)
                {
                    RecordSoftFail(placeres, rdz);
                    bestPlaceRes = placeres;
                    continue; // Try for better alternative.
                }

                // Must be a pass
                bestPlaceRes = placeres;
                return rdz;
            }

            if (bestPlaceRes?.IsDistanceSoftFail == true)
            {
                if (RecentBestRdz == null) throw new Exception("Cannot happen");
                return RecentBestRdz;
            }

            // rip
            throw new Exception("True softlock: nowhere to place without breaking conditions");
        }
        private static void ResetMinMaxElligible()
        {
            // reset after each FindEllgibleRdz completion
            MinElligDist = 10000;
            MaxElligDist = 0;
            MinElligRdz = null;
            MaxElligRdz = null;
            RecentBestRdz = null;
        }
        private static void RecordSoftFail(PlaceResult placeres, Randomization rdz)
        {
            var dist = placeres?.DistanceRes?.Distance ?? throw new NullReferenceException();

            // Prepare for unmeetable distance restrictions (these are ONLY 
            // used if NONE of the Rdz are elligible!
            if (placeres.FailTooNear && dist > MaxElligDist)
            {
                // Rdz dist is lower than minimum setting, record our "furthest" failing attempt
                MaxElligDist = dist;
                MaxElligRdz = rdz; // use as last resort
                RecentBestRdz = rdz; // todo one day
            }
            if (placeres.FailTooFar && dist < MinElligDist)
            {
                // All distances were higher than maximum allowed dist.
                // Find the lowest distance possible and use it:
                MinElligDist = dist;
                MinElligRdz = rdz;
                RecentBestRdz = rdz; // todo one day
            }
        }
        //
        private PlaceResult GetPlacementResult(Randomization rdz, DropInfo di, SetType st)
        {
            // allocate output obj
            PlaceResult placeRes = new();

            // Priority Filter (Reserved UI items):
            placeRes.AddReservedRes(GetReservedResult(rdz, di));
            if (!placeRes.PassedReservedCond) return placeRes; // exit on fail reserved checks

            // Remaining filters:
            if (placeRes.IsVanillaPlacement)
            {
                // special Vanilla placement checks (vanilla keys handled separately)
                placeRes.AddCategoryRes(CategoryRes.VanillaOverride); // autopass: n/a for Vanilla placements
                return placeRes;
            }

            // normal randomizer placement checks
            placeRes.AddCategoryRes(GetPickupTypeCond(rdz, di, st));    // check pickuptypes
            if (!placeRes.PassedCategoryCond) return placeRes;          // exit on fail pickuptypes
            placeRes.AddSoftlockRes(IsSoftlockPlacement(rdz, KeysPlacedSoFar));  // check softlock
            if (!placeRes.PassedSoftlockCond) return placeRes;          // exit on fail softlock
            placeRes.AddDistanceRes(GetDistanceResult(rdz, di));        // check distance logic
            return placeRes;                                            // final return
        }
        private ReservedRes GetReservedResult(Randomization rdz, DropInfo di)
        {
            // This condition passes if:
            // 1. Rdz has no reservation, itemID has no restriction
            // 2. Rdz reservation already fulfilled, itemID has no restriction
            // 3. Rdz reserved for provided input itemid
            //
            // vanplace is a special boolean flag to state that this
            // is the case 3) above, where we OVERRULE other restrictions
            // EXCEPT softlock conditions that must still be abided.
            // Vanilla keys are placed last in the list to ensure
            // that they should pass softlock.

            // Check if special item or rdz
            bool rdzReserved = ReservedRdzs.ContainsKey(rdz);
            bool itemReserved = ReservedRdzs.ContainsValue(di.ItemID);

            // 00 - always pass
            if (!rdzReserved && !itemReserved)
                return new ReservedRes(LOGICRES.SUCCESS); // Case 1 [no restrictions]

            // 01 - always fail
            if (!rdzReserved && itemReserved)
                return new ReservedRes(LOGICRES.FAIL_VAN_WRONGRDZ); // item is reserved for different rdz

            // 10 - conditional
            if (rdzReserved && !itemReserved)
            {
                var isRdzFulfilled = rdz.HasShuffledItemID(ReservedRdzs[rdz]);
                if (isRdzFulfilled)
                    return new ReservedRes(LOGICRES.SUCCESS); // Case 2 [already completed reservations]
                else
                    return new ReservedRes(LOGICRES.FAIL_RESERVED); // rdz is reserved for a different item
            }

            // 11 - conditional
            if (rdzReserved && itemReserved)
            {
                if (ReservedRdzs[rdz] == di.ItemID)
                    return new ReservedRes(LOGICRES.SUCCESS_VANPLACE); // Case 3 [matched reservations]
                else
                    return new ReservedRes(LOGICRES.FAIL_RESERVED); // each are reserved for other places
            }
            throw new Exception("Missed some logic apparently?");
        }
        private bool CanPlaceVanillaKey(Randomization rdz, DropInfo di)
        {
            if (IsRotundaDeadlock(di))
                return true; // Rotunda will just be placed in Heides without issue

            return !IsSoftlockPlacement(rdz, KeysPlacedSoFar);
        }
        internal static bool IsSoftlockPlacement(Randomization rdz, List<int> placedSoFar)
        {
            // Try each different option for key requirements
            var kso = rdz.RandoInfo.KSO;
            if (kso.Count == 0)
                return false; // can't cause a softlock if there's no restrictions

            foreach (var keyset in kso)
            {
                if (keyset.Keys.All(kid => placedSoFar.Contains((int)kid)))
                    return false; // NOT SOFT LOCKED all required keys are placed for at least one Keyset
            }
            return true; // No keyset has all keys placed yet, so this is dangerous; try somewhere else
        }
        private CategoryRes GetPickupTypeCond(Randomization rdz, DropInfo di, SetType st)
        {
            // This condition passes if:
            // 1. Rdz abides by pickuptype flags for item-type
            //
            // The list of allowed pickuptypes is set by the item that we are
            // placing, where the item is categorized as the first bullet
            // that applies starting from the top (highest priority):
            //    - Category: CustomManuallyAdjustable [UI-customisable item restrictions]
            //    - Category: KeyItems                  
            //    - Category: RequiredItems             
            //    - Category: GeneralItems

            // Racemode special:
            var isRaceMode = true;
            if (isRaceMode && st == SetType.Keys)
            {
                if (RdzMajors.Contains(rdz))
                    return CategoryRes.RaceKeyPass;
                else
                    return CategoryRes.RaceKeyFail;
            }

            // Normal checks:
            if (rdz.ContainsOnlyTypes(DefineAllowedPickupTypes(st, di)))
                return CategoryRes.ValidPickupTypes;
            else
                return CategoryRes.ForbiddenPickupTypes;
        }
        private List<PICKUPTYPE> DefineAllowedPickupTypes(SetType st, DropInfo di)
        {
            // todo
            if (ManuallyRequiredItemsTypeRules.ContainsKey(di.ItemID))
                return FullySafeFlags; // to generalize with front-end

            if (IsRestrictedItem(di.ItemID))
                return FullySafeFlags; // front-end specified items

            if (st == SetType.Keys)
                return FullySafeFlags;
            if (st == SetType.Reqs) 
                return HalfSafe;
            if (st == SetType.Gens)
                return AllowedGenTypes;
            throw new Exception("Unexpected fall through set type");
        }
        private DistanceRes GetDistanceResult(Randomization rdz, DropInfo di)
        {
            // This condition passes if:
            // a) itemID has no distance restriction
            // b) itemID has distance restriction that is satisfied

            if (!DistanceRestrictedIDs.TryGetValue(di.ItemID, out var minmax))
                return DistanceRes.NoRestriction; // no distance restriction on item

            if (rdz is DropRdz)
                throw new Exception("Reserved item should be forbidden from DropRdz in CategoryRes");

            if (rdz.RandoInfo.Area == MapArea.Undefined || rdz.RandoInfo.Area == MapArea.Quantum)
                return DistanceRes.Incalculable; // should be impossible to get here, left in as safety

            // get node we're trying to place into
            var node = Nodes[rdz.RandoInfo.NodeKey];
            if (node.IsLocked)
                throw new Exception("Should have been caught in SoftlockRes checks");

            // Calc "traversible distance" to this Rdz including required keys on passage.
            int dist = SteinerTreeDist(RandoGraph, node.SteinerNodes, out var steinsol);
            //var _ = HelperListID2Maps(steinsol); // debugging

            // Breaking soft constraints:
            if (dist < minmax.Min)
                return DistanceRes.TooNear(dist);
            if (dist > minmax.Max)
                return DistanceRes.TooFar(dist);

            // Perfect gauntlet
            return DistanceRes.DistancePass(dist);
        }
        private static bool IsRotundaDeadlock(DropInfo di)
        {
            // Catch a specific meme regarding the Rotunda Lockstone
            // in that it's second Vanilla location (crushed eye invasion)
            // is locked behind Drangleic (which is in turn locked
            // behind placing the Rotunda).
            // Given that Licia is always (CURRENTLY) 
            // accessible from the start, we can just hard-code 
            // fix this case.
            if (di.ItemID == (int)ITEMID.ROTUNDALOCKSTONE)
                return true;
            return false;
        }
        internal void CreateRdzMajors()
        {
            RdzMajors = PTF.Where(rdz => SatisfiesMajorCondition(rdz)).ToList();
        }
        internal static bool SatisfiesMajorCondition(Randomization rdz)
        {
            // todo define/read UI settings

            // do fastest queries first for efficiency
            bool isboss = rdz.HasPickupType(PICKUPTYPE.BOSS);
            bool issafe = rdz.HasPickupType(FullySafeFlags);
            if (!(isboss || issafe)) return false; // volatile or uninterested

            // add more logic here as you see fit
            var majoritems = new List<ITEMID>() { ITEMID.ESTUSSHARD, ITEMID.BONEDUST, ITEMID.FRAGRANTBRANCH };
            bool ismajorpickup = rdz.HasVanillaAnyItemID(majoritems);
            return isboss || (issafe && ismajorpickup);
        }

        // Key updates
        private void UpdateForNewKey(Randomization rdz, ITEMID keyid)
        {
            // Check if interesting
            if (!ReachedNumKeysTrigger(keyid, out var keyenact))
                return;

            // Update softlock logic
            KeysPlacedSoFar.Add((int)keyenact);
            Steiner.UpdateSteinerNodesOnKey(rdz.RandoInfo.NodeKey, keyenact);
        }
        


        private bool ReachedNumKeysTrigger(ITEMID keyid, out KEYID triggerkey)
        {
            // Conceptually: Is the number of placed keys enough to unlock
            // new things. For most keys this is simply as soon as they are placed
            // i.e. Trigger is 1. This function also handles "MultiKeys" events
            // e.g. TENBRANCHLOCK etc.
            triggerkey = default;

            // how many are placed in total
            var count = Scope.AllPtf.Where(rdz => rdz.HasShuffledItemId(keyid)).Count();

            // Handle ordinary keys first:
            var ismultikey = MultiKeyTriggers.ContainsKey(keyid);
            if (!ismultikey && count == 1)
            {
                triggerkey = (KEYID)(int)keyid; // convert enum
                return true; // add it
            }
            
            // Multikeys:
            var triggers = MultiKeyTriggers[keyid];
            var exacthits = triggers.Where(i => i == count).ToList();
            if (exacthits.Count == 0) 
                return false; // not on trigger

            var hit = exacthits.First(); // will only be one
            triggerkey = GetMultiKey(keyid, hit);
            return true;
        }
        private static KEYID GetMultiKey(ITEMID keyid, int count)
        {
            return keyid switch
            {
                ITEMID.TORCH => KEYID.TORCH,
                ITEMID.SOULOFAGIANT => KEYID.SOULOFAGIANT,
                ITEMID.NADALIAFRAGMENT => KEYID.ALLNADSOULS,
                ITEMID.PHARROSLOCKSTONE => count == 2 ? KEYID.BIGPHARROS : KEYID.MEMEPHARROS,
                ITEMID.FRAGRANTBRANCH => count == 3 ? KEYID.BRANCH : KEYID.TENBRANCHLOCK,
                ITEMID.SMELTERWEDGE => KEYID.ALLWEDGES,
                _ => throw new Exception("Unexpected MultiKey condition")
            };
        }
        



        

        

        
        




        

        //private void FixLinkedRdz()
        //{
            // TODO
            // Get linked Rdzs:
            //List<Randomization> linkedrdz = new();
            //if (!ReservedRdzs.ContainsKey(rdz))
            //    linkedrdz.Add(rdz);
            //else
            //    linkedrdz = ReservedRdzs.Where(kvp => kvp.Value == di.ItemID)
            //                            .Select(kvp => kvp.Key).ToList();

            //foreach (var lrdz in linkedrdz)
            //    lrdz.AddShuffledItem(di);
        //}

        
    }
}
