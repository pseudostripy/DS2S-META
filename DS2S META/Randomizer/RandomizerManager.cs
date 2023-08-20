using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using DS2S_META.Utils;
using static DS2S_META.Randomizer.RandomizerManager;
using System.Runtime.Intrinsics.Arm;
using DS2S_META.ViewModels;
using System.Xml.Serialization;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Windows;
using Octokit;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Documents;
using DS2S_META.Randomizer.Placement;

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
        List<Randomization> AllPTF = new(); // Places to Randomize
        
        private List<ItemLotRow> VanillaLots = new();
        private List<ItemLotRow> VanillaDrops = new();
        private List<ShopRow> VanillaShops = new();

        internal List<ShopRow> ShopsToFillByCopying = new();
        internal ItemSetBase Logic = new CasualItemSet();
        internal List<DropInfo> LTR_flatlist = new();
        internal bool IsInitialized = false;
        internal bool IsRandomized = false;

        internal List<Randomization> RdzMajors = new(); // populated in ??
        internal List<ItemRestriction> Restrictions;
        internal List<int> RestrictedItems = new(); // shorthand
        
        // From front-end
        internal Dictionary<Randomization, int> ReservedRdzs = new();
        internal Dictionary<int,MinMax>  DistanceRestrictedIDs = new();
        // 
        internal List<DropInfo> ldkeys = new();         // all keys
        //internal List<DropInfo> ldkeys_res = new();     // vanilla keys
        //internal List<DropInfo> ldkeys_unres = new();   // other keys
        internal List<DropInfo> ldreqs = new();
        internal List<DropInfo> ldgens = new();
        //
        private List<Randomization> UnfilledRdzs = new();
        private List<int> KeysPlacedSoFar = new();
        private Dictionary<KEYID, HashSet<int>> KeySteinerNodes = new(); // NodeID lookup
        private Dictionary<MapArea, HashSet<int>> AreaPaths = new(); // NodeID lookup
        private List<int> ResVanPlacedSoFar = new();
        internal Dictionary<NodeKey,Node> Nodes = new();
        internal static Dictionary<MapArea, int> Map2Id = new();
        internal static Dictionary<int, MapArea> Id2Map = new();
        internal int CurrSeed;
        internal int[,] RandoGraph;
        List<KeySet> UniqueIncompleteKSs = new();
        internal IEnumerable<Diset> Disets; // Groups of itemtypes to be placed, eg. "Keys"

        //
        internal static Dictionary<int, ItemRow> VanillaItemParams = new();

        internal static bool TryGetItem(int itemid, out ItemRow? item)
        {
            bool found = VanillaItemParams.ContainsKey(itemid);
            item = found ? VanillaItemParams[itemid] : null;
            return found;
        }
        internal static int GetItemMaxUpgrade(ItemRow item)
        {
            // Wrapper similar to the DS2Item class call in Hook.
            switch (item.ItemType)
            {
                case eItemType.WEAPON1: // & shields
                case eItemType.WEAPON2: // & staves
                    return item.WeaponRow?.MaxUpgrade ?? 0;
                case eItemType.HEADARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                case eItemType.LEGARMOUR:
                    return item.ArmorRow?.ArmorReinforceRow?.MaxReinforceLevel ?? 0;

                default:
                    return 0;
            }
        }
        
        // Constructors:
        internal RandomizerManager()
        {
            Rng.SetSeed(Environment.TickCount); // used for generate seed numbers
        }

        // Main Methods:
        internal void Initalize(DS2SHook hook)
        {
            Hook = hook; // Required for reading game params in memory

            // One-time speedup
            CreateAllowedKeyTypes();
            CreateAllowedGenTypes();
            SetupAreasGraph();

            // Param collecting:
            Logic = new CasualItemSet();
            VanillaLots = ParamMan.ItemLotOtherRows.ToList(); // shorthand
            VanillaLots = ParamMan.ItemLotOtherRows.ToList(); // shorthand
            VanillaLots = ParamMan.ItemLotOtherRows.ToList(); // shorthand
            VanillaItemParams = ParamMan.ItemRows.ToDictionary(it => it.ItemID, it => it);

            SetupAllPTF();
            AddDropsToLogic();
            GetLootToRandomize(); // set LTR_Flatlist field
            IsInitialized = true;
        }
        

        
        // Core:
        internal async Task Randomize(int seed)
        {
            if (Hook == null)
                return;

            // Setup for re-randomization:
            if (!EnsureSeedCompatibility(seed)) return;
            SetSeed(seed);      // reset Rng Twister
            
            // todo moveto rerandomization setup
            SetupRestrictions();
            ResetForRerandomization();
            CreateRdzMajors();

            // Place sets of items:
            var placer = new PlacementManager(Disets);
            PlaceSet(ldkeys, SetType.Keys);
            PlaceSet(ldreqs, SetType.Reqs);
            PlaceSet(ldgens, SetType.Gens);
            FillLeftovers();

            // Miscellaneous things to handle:
            HandleTrivialities();   // Simply mark done
            FixShopEvents();        // All additional shop processing & edge cases.
            FixLotCopies();         // Aka Pursuer
            CharCreation.Randomize();

            // Ensure all handled:
            var unhandled = AllP.Where(rdz => rdz.IsHandled == false);
            if (unhandled.Any())
                throw new Exception("Unhandled things remain.");

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
        internal static int GetRandom() => Rng.Next();


        // Core Logic
        internal void SetupRestrictions()
        {
            // - Split restrictions and assign to associated arrays
            // - Choose one from a group of items for this seed
            ReservedRdzs = new();
            DistanceRestrictedIDs = new();
            ResVanPlacedSoFar = new();

            int itemid;

            // Choose each from the set:
            foreach (var restr in Restrictions)
                restr.ItemID = restr.ItemIDs[Rng.Next(restr.ItemIDs.Count)];
            
            foreach( var irest in Restrictions)
            {
                switch (irest.RestrType)
                {
                    case RestrType.Anywhere:
                        break; // no restriction

                    case RestrType.Vanilla:
                        // Draw random itemID from list of options:
                        itemid = irest.ItemID;

                        // Get vanillas:
                        var rdzvans = AllPTF.Where(rdz => rdz.HasVanillaItemID(itemid)).ToList();
                        if (rdzvans.Count == 0)
                            throw new Exception("Cannot find the Vanilla placement");
                        
                        // Store each vanilla:
                        foreach(var rdz in rdzvans)
                            ReservedRdzs[rdz] = itemid; // store
                        break;

                    case RestrType.Distance:
                        DistanceRestrictedIDs[irest.ItemID] = new MinMax(irest.DistMin, irest.DistMax);
                        break;
                }
            }

            // Shorthand
            RestrictedItems = Restrictions.Select(restr => restr.ItemID).ToList();
        }
        

        
        private void ResetForRerandomization()
        {
            // Reset required arrays for the randomizer to work:

            // Empty the shuffled places in preparation:
            foreach (var rdz in AllP)
                rdz.ResetShuffled();
            SetupTravNodes();
            SolveBasicAreas(); // solution if keys aren't a concept


            KeysPlacedSoFar = new List<int>();
            UnfilledRdzs = new List<Randomization>(AllPTF); // initialize with all spots
            UniqueIncompleteKSs = FindUniqueKS();
            KeySteinerNodes = new();

            // Remake (copies of) list of Keys, Required, Generics for placement
            DefineKRG();
        }
        private List<KeySet> FindUniqueKS()
        {
            List<KeySet> ksuniques = new();
            var shoplots = AllPTF.Where(rdz => rdz is not DropRdz).ToList();
            foreach(var rdz in shoplots)
            {
                if (rdz.RandoInfo.IsKeyless)
                    continue;

                foreach (var ks in rdz.RandoInfo.KSO)
                {
                    if (!ksuniques.Contains(ks))
                        ksuniques.Add(ks);
                }
            }
            return ksuniques;
        }
        private void HandleTrivialities()
        {
            foreach (var rdz in AllP.Where(rdz => rdz.Type == RDZ_TASKTYPE.EXCLUDE))
                rdz.MarkHandled();

            // TODO!
            foreach (var rdz in AllP.Where(rdz => rdz.Type == RDZ_TASKTYPE.CROWS))
                rdz.MarkHandled();
        }
        private void FixShopEvents()
        {
            FixShopCopies();
            FixNormalTrade();
            FixShopSustains();
            FixShopTradeCopies();
            FixFreeTrade(); // needs to be after FixShopTradeCopies()
            FixShopsToRemove();
        }

        
        
        internal void AddDropsToLogic()
        {
            var active_drops = AllP.OfType<DropRdz>();
            foreach (var droprdz in active_drops)
            {
                // Append:
                droprdz.RandoInfo.Description = droprdz.RandoDesc; // still empty for now
                PICKUPTYPE droptype = droprdz.IsGuaranteedDrop ? PICKUPTYPE.GUARANTEEDENEMYDROP : PICKUPTYPE.ENEMYDROP;
                droprdz.RandoInfo.PickupTypes = new PICKUPTYPE[] { droptype };

                if (DropKeySets.TryGetValue(droprdz.ParamID, out var kso))
                    droprdz.RandoInfo.KSO = kso;

                Logic.D[droprdz.GUID] = droprdz.RandoInfo; // Add to dictionary
            }
        }
        internal static Dictionary<int, List<KeySet>> DropKeySets = new()
        {
            // Awkward cases of DropRdzs dropping keys in vanilla:
            [(int)DROPPARAMID.LICIAHEIDES] = new List<KeySet>(),
            [(int)DROPPARAMID.DUKETSELDORA] = new List<KeySet>() {new KeySet(KEYID.BRANCH) },
        };
        internal void DefineKRG()
        {
            // Take a copy of the FlatList so we don't need to regenerate everytime:
            var flcopy = LTR_flatlist.Select(di => di.Clone()).ToList();

            // Partition into KeyTypes, ReqNonKeys and Generic Loot-To-Randomize:
            var keys = flcopy.Where(di => di.IsKeyType).ToList();                                  // Keys
            var flNoKeys = flcopy.Except(ldkeys);
            var reqs = flNoKeys.Where(di => di.IsReqType || IsRestrictedItem(di.ItemID)).ToList(); // Reqs
            var gens = flNoKeys.Except(ldreqs).ToList();                                           // Generics

            // Create group objects for placement
            Disets = new List<Diset>()
            {
                Diset.FromKeys(keys),
                Diset.FromReqs(reqs),
                Diset.FromGens(gens),
            };
        }
        internal void FixFlatList()
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

        // Placement code:
        internal static Dictionary<SetType, List<PICKUPTYPE>> BannedTypeList = new()
        {
            {SetType.Keys, ItemSetBase.BanKeyTypes},
            {SetType.Gens, ItemSetBase.BanGeneralTypes}
        };
        internal static List<PICKUPTYPE> AllowedKeyTypes = new();
        internal static List<PICKUPTYPE> AllowedGenTypes = new();

        internal static List<PICKUPTYPE> GetAllowedFromBanned(SetType settype)
        {
            // Simple wrapper so that we can define things in terms of banned types
            // rather than having to always specify all allowed ones:
            var bantypes = BannedTypeList[settype];
            var allowtypes = new List<PICKUPTYPE>();

            foreach (var putype in Enum.GetValues(typeof(PICKUPTYPE)).Cast<PICKUPTYPE>())
            {
                if (!bantypes.Contains(putype))
                    allowtypes.Add(putype);
            }
            return allowtypes;
        }
        
        // Create as static list since they're called so often
        internal static void CreateAllowedGenTypes()
        {
            AllowedGenTypes = GetAllowedFromBanned(SetType.Gens);
        }
        internal static void CreateAllowedKeyTypes()
        {
            AllowedKeyTypes = GetAllowedFromBanned(SetType.Keys);
        }
        internal static List<PICKUPTYPE> GetAllowedReqTypes(DropInfo di)
        {
            // Item of interest
            var item = di.ItemID.AsItemRow();
            return ItemSetBase.ItemAllowTypes[item.ItemType];
        }
        internal void CreateRdzMajors()
        {
            RdzMajors = AllPTF.Where(rdz => MajorCondition(rdz)).ToList();
        }
        internal static bool MajorCondition(Randomization rdz)
        {
            // todo define/read UI settings

            // do fastest queries first for efficiency
            bool isboss = rdz.HasPickupType(PICKUPTYPE.BOSS);
            bool issafe = rdz.HasPickupType(ItemSetBase.FullySafeFlags);
            if (!(isboss || issafe)) return false; // volatile or uninterested

            // add more logic here as you see fit
            var majoritems = new List<ITEMID>() { ITEMID.ESTUSSHARD, ITEMID.BONEDUST, ITEMID.FRAGRANTBRANCH };
            bool ismajorpickup = rdz.HasVanillaAnyItemID(majoritems);
            return isboss || (issafe && ismajorpickup);
        }

        

        // Steiner distance logic
        private static bool IsFailure(LOGICRES res)
        {
            return res switch
            {
                LOGICRES.FAIL_DIST_TOO_NEAR => true,
                LOGICRES.FAIL_DIST_TOO_FAR => true,
                LOGICRES.FAIL_DIST_NOTAPPLICABLE => true,
                LOGICRES.FAIL_SOFTLOCK => true,
                LOGICRES.FAIL_PICKUPTYPE => true,
                LOGICRES.FAIL_RESERVED => true,
                LOGICRES.FAIL_VAN_WRONGRDZ => true,
                _ => false,
            };
        }
        private void UpdateForNewKey(Randomization rdz, DropInfo di)
        {
            // This has been growing as I've been debugging,
            // probably needs refactoring as some point.

            // Big function to update a variety of class variables and 
            // logic, including Graph after a key has been placed
            KEYID keyid;
            if (IsMultiKey(di.ItemID))
            {
                if (!HandleMultiKey(di.ItemID, out keyid))
                    return; // nothing new to say
            }
            else
            {
                KeysPlacedSoFar.Add(di.ItemID); // std keys
                keyid = GetEnumKEYID(di.ItemID);
            }

            // Get node we're placing into, and calculate new Steiner set and distance
            var rdzNode = Nodes[rdz.RandoInfo.NodeKey]; // where we're placing key
            KeySteinerNodes[keyid] = rdzNode.SteinerNodes.ToHashSet(); // it must be unlocked and therefore have required nodes

            // check for new KeySets now achieved
            var relevantKSs = GetNewUnlocks();


            foreach (var ks in relevantKSs)
            {
                // Acknowledge keyset completion
                UniqueIncompleteKSs.Remove(ks);

                // Graph changes:
                var affectedNodes = Nodes.Where(ndkvp => ndkvp.Key.HasKeySet(ks)).ToList();
                if (affectedNodes.Count == 0)
                    continue;



                // Steiner nodes are given by the unique union of key nodes
                // that are used to unlock this KeySet.
                HashSet<int> kshash = new();
                foreach (var kid in ks.Keys)
                {
                    foreach (var i in KeySteinerNodes[kid])
                        kshash.Add(i); // add unique nodes
                }
                
                // Unlock/Update nodes:
                foreach (var nodekvp in affectedNodes)
                {
                    var node = nodekvp.Value;

                    // Steiner nodes are given by the unique union of key nodes
                    // that are used to unlock this KeySet.
                    HashSet<int> myhash = new(kshash);
                    var allnodes = AddHashset(myhash,AreaPaths[node.NodeKey.Area]).ToList();

                    // Require all previous keys, and to get to current location:
                    //List<int> allnodes = new(ksnodes) { Map2Id[node.NodeKey.Area] };

                    // Easy case:
                    if (node.IsLocked)
                    {
                        node.SteinerNodes = allnodes; // unlock
                        continue;
                    }

                    // Hard case: already unlocked and we're providing an alternative
                    // path to the Node.
                    var newdist = allnodes.Count;
                    //var newdist = SteinerTreeDist(RandoGraph, allnodes, out var steinsol);
                    var olddist = node.SteinerNodes.Count;

                    // Update if nodes provides a better path:
                    if (newdist < olddist)
                        node.SteinerNodes = allnodes;   
                }
            }
        }
        private static HashSet<int> AddHashset(HashSet<int> src, HashSet<int> newset)
        {
            foreach (var i in newset)
                src.Add(i);
            return src;
        }
        private static KEYID GetEnumKEYID(int keyid)
        {
            foreach (var k in Enum.GetValues(typeof(KEYID)).Cast<KEYID>())
            {
                if ((int)k == keyid) return k;
            }
            throw new Exception("Cannot find Enum for KEYID");
        }
        private static bool IsMultiKey(int itemID)
        {
            var multikeys = new List<KEYID>() { KEYID.TORCH, KEYID.SOULOFAGIANT, KEYID.NADALIAFRAGMENT,
                                                KEYID.PHARROSLOCKSTONE, KEYID.FRAGRANTBRANCH, KEYID.SMELTERWEDGE };
            return multikeys.Cast<int>().Contains(itemID);
        }
        private bool HandleMultiKey(int itemID, out KEYID keyid)
        {
            // This is messy af. TODO

            // Checks if a new number count for this ID reaches the boundary
            // If it does, add the new count-key to the list and return true
            keyid = KEYID.NONE;
            var count = AllPTF.Where(rdz => rdz.HasShuffledItemID(itemID)).Count(); // *technically an oversight*
            switch (itemID)
            {
                case (int)KEYID.TORCH:
                case (int)KEYID.SOULOFAGIANT:
                    if (count <= 3 || KeysPlacedSoFar.Contains(itemID))
                        return false;

                    KeysPlacedSoFar.Add(itemID); // itemID for torch and giant match keyid
                    if (itemID == (int)KEYID.TORCH)
                        keyid = KEYID.TORCH;
                    else
                        keyid = KEYID.SOULOFAGIANT;
                    return true;

                case (int)KEYID.NADALIAFRAGMENT:
                    if (KeysPlacedSoFar.Contains((int)KEYID.ALLNADSOULS))
                        return false;
                    if (count != 11)
                        return false;

                    KeysPlacedSoFar.Add((int)KEYID.ALLNADSOULS);
                    keyid = KEYID.ALLNADSOULS;
                    return true;

                case (int)KEYID.SMELTERWEDGE:
                    if (KeysPlacedSoFar.Contains((int)KEYID.ALLWEDGES))
                        return false;
                    if (count != 11)
                        return false;

                    KeysPlacedSoFar.Add((int)KEYID.ALLWEDGES);
                    keyid = KEYID.ALLWEDGES;
                    return true;

                case (int)KEYID.PHARROSLOCKSTONE:
                    if (KeysPlacedSoFar.Contains((int)KEYID.MEMEPHARROS))
                        return false; // already at max effect
                    
                    if (count == 10)
                    {
                        KeysPlacedSoFar.Add((int)KEYID.MEMEPHARROS);
                        keyid = KEYID.MEMEPHARROS;
                        return true;
                    }

                    if (KeysPlacedSoFar.Contains((int)KEYID.BIGPHARROS))
                        return false;

                    if (count == 2)
                    {
                        KeysPlacedSoFar.Add((int)KEYID.BIGPHARROS);
                        keyid = KEYID.BIGPHARROS;
                        return true;
                    }
                    return false;

                case (int)KEYID.FRAGRANTBRANCH:
                    if (KeysPlacedSoFar.Contains((int)KEYID.TENBRANCHLOCK))
                        return false; // already at max effect

                    if (count == 10)
                    {
                        KeysPlacedSoFar.Add((int)KEYID.TENBRANCHLOCK);
                        keyid = KEYID.TENBRANCHLOCK;
                        return true;
                    }

                    if (KeysPlacedSoFar.Contains((int)KEYID.BRANCH))
                        return false;

                    if (count == 3)
                    {
                        KeysPlacedSoFar.Add((int)KEYID.BRANCH);
                        keyid = KEYID.BRANCH;
                        return true;
                    }
                    return false;

                default:
                    throw new Exception("Unexpected mulitkey ID");
            }
        }
        private List<KeySet> GetNewUnlocks()
        {
            // Check over the incomplete keysets and see what completes:
            List<KeySet> newKeys = new();
            foreach(var ks in UniqueIncompleteKSs)
            {
                if (ks.Keys.All(keyid => ItemSetBase.IsPlaced(keyid, KeysPlacedSoFar)))
                    newKeys.Add(ks);
            }
            return newKeys;
        }

        private LOGICRES FindElligibleRdz(DropInfo di, SetType stype, out Randomization? rdz_ellig)
        {
            // Find an Rdz (at random) satisfying all constraints.
            rdz_ellig = default;
            int min_ellig_dist = 10000;
            int max_ellig_dist = 0;
            Randomization? min_ellig_Rdz = null;
            Randomization? max_ellig_Rdz = null;
            bool bfound_ellig_buttoonear = false;
            bool bfound_ellig_buttoofar = false;
            int numfail_distmin = 0;
            int numfail_distmax = 0;

            // List of remaining spots to search through
            var availRdzs = new List<Randomization>(UnfilledRdzs);

            while (availRdzs.Count > 0)
            {
                // Choose random rdz for item:
                var rdz = availRdzs[Rng.Next(availRdzs.Count)];
                var rescheck = PassedPlacementConds(rdz, di, stype, out var dist);

                // Prepare for unmeetable distance restrictions (these are ONLY 
                // used if NONE of the Rdz are elligible!
                if (rescheck == LOGICRES.FAIL_DIST_TOO_NEAR && dist > max_ellig_dist)
                {
                    // dist was lower than minimum setting.
                    // Find the highest distance we can, since everything we tried was
                    // too low.
                    max_ellig_dist = dist;
                    max_ellig_Rdz = rdz; // use as last resort
                    bfound_ellig_buttoonear = true;
                    numfail_distmin++;
                }
                if (rescheck == LOGICRES.FAIL_DIST_TOO_FAR && dist < min_ellig_dist)
                {
                    // All distances were higher than maximum allowed dist.
                    // Find the lowest distance possible and use it:
                    min_ellig_dist = dist;
                    min_ellig_Rdz = rdz;
                    bfound_ellig_buttoofar = true;
                    numfail_distmax++;
                }


                switch (rescheck)
                {
                    // Failed: remove it from available options
                    case LOGICRES.FAIL_RESERVED:
                    case LOGICRES.FAIL_VAN_WRONGRDZ:
                    case LOGICRES.FAIL_PICKUPTYPE:
                    case LOGICRES.FAIL_SOFTLOCK:
                    case LOGICRES.FAIL_DIST_TOO_NEAR:
                    case LOGICRES.FAIL_DIST_TOO_FAR:
                    case LOGICRES.FAIL_DIST_NOTAPPLICABLE:
                        availRdzs.Remove(rdz);
                        continue;

                    // Semi-fail: cannot place vanilla just yet
                    case LOGICRES.DELAY_VANLOCKED:
                        return LOGICRES.DELAY_VANLOCKED;

                    case LOGICRES.SUCCESS:
                    case LOGICRES.SUCCESS_VANPLACE:
                        if (dist != -1)
                            rdz.PlaceDist = dist;
                        rdz_ellig = rdz;
                        return rescheck;

                    default:
                        throw new Exception("?");
                }
            }

            // Try our very best to reach the distance requirement.
            // Delay placement and look again later when more
            // Rdzs are unlocked for placement
            if (bfound_ellig_buttoonear && stype == SetType.Keys)
            {
                // "Is there any key left to be placed that doesn't have
                // restrictions?" if so, we can delay and come back later
                if (ldkeys.Any(di => !IsRestrictedItem(di.ItemID)))
                    return LOGICRES.DELAY_MAXDIST;
            }

            // Best attempt (only failed on distance check comparison vs user value):
            if (bfound_ellig_buttoonear && !bfound_ellig_buttoofar || numfail_distmin >= numfail_distmax)
            {
                rdz_ellig = max_ellig_Rdz;
                if (rdz_ellig == null) throw new NullReferenceException();
                rdz_ellig.PlaceDist = max_ellig_dist;
                return LOGICRES.SUCCESS_DISTCOMPROMISE;
            }
            if (bfound_ellig_buttoofar && !bfound_ellig_buttoonear || numfail_distmin < numfail_distmax)
            {
                rdz_ellig = min_ellig_Rdz;
                if (rdz_ellig == null) throw new NullReferenceException();
                rdz_ellig.PlaceDist = min_ellig_dist;
                return LOGICRES.SUCCESS_DISTCOMPROMISE;
            }
            
            // True softlock - no elligible rdz place
            throw new Exception("True Softlock, please investigate");
        }


        // placement logic filters:
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
                // special Vanilla placement checks
                placeRes.AddCategoryRes(CategoryRes.VanillaOverride); // autopass: n/a for Vanilla placements
                placeRes.AddSoftlockRes(GetVanSoftlockLogic(rdz, di, st));
                return placeRes;
            }

            // normal randomizer placement checks
            placeRes.AddCategoryRes(GetPickupTypeCond(rdz, di, st));    // check pickuptypes
            if (!placeRes.PassedCategoryCond) return placeRes;          // exit on fail pickuptypes
            placeRes.AddSoftlockRes(GetVanSoftlockLogic(rdz, di, st));  // check softlock
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
        private SoftlockRes GetVanSoftlockLogic(Randomization rdz, DropInfo di, SetType st)
        {
            if (IsRotundaDeadlock(di))
                return SoftlockRes.DeadlockOverride; // Rotunda will just be placed in Heides without issue

            return GetSoftlockLogic(rdz, st);
        }
        private SoftlockRes GetSoftlockLogic(Randomization rdz, SetType st)
        {
            // This condition passes if:
            // 1. Not placing keys: logic was already resolved
            // 2. Placing keys, and all required keys for this specific Rdz 
            //      location are already placed, so this is in logic.

            if (!st.IsKeys())
                return SoftlockRes.NotRequired; // settypes cannot lock things behind them

            // Check whether rdz is open yet
            if (rdz.IsSoftlockPlacement(KeysPlacedSoFar))
                return SoftlockRes.Softlock;
            else
                return SoftlockRes.InLogic;
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
            if (isRaceMode && st.IsKeys())
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

        // placement logic utility helpers:
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
        private List<PICKUPTYPE> DefineAllowedPickupTypes(SetType st, DropInfo di)
        {
            // todo
            if (ItemSetBase.ManuallyRequiredItemsTypeRules.ContainsKey(di.ItemID))
                return ItemSetBase.FullySafeFlags; // to generalize with front-end

            if (IsRestrictedItem(di.ItemID))
                return ItemSetBase.FullySafeFlags; // front-end specified items

            if (st.IsKeys()) return AllowedKeyTypes;
            if (st.IsReqs()) return GetAllowedReqTypes(di);
            if (st.IsGens()) return AllowedGenTypes;
            throw new Exception("Unexpected fall through set type");
        }
        private bool IsRestrictedItem(int itemid) => RestrictedItems.Contains(itemid);
        
        

        // Traversible distance calculations
        private void SetupAreasGraph()
        {
            // Defines the adjacency matrix for connected game areas

            // Static for now (TODO):
            List<MapArea> MapList = new() // "Column/row lookup"
            {
                MapArea.ThingsBetwixt,
                MapArea.Majula,
                MapArea.FOFG,
                MapArea.HeidesTowerOfFlame,
                MapArea.NoMansWharf,
                MapArea.TheLostBastille,
                MapArea.BelfryLuna,
                MapArea.SinnersRise,
                MapArea.HuntsmansCopse,
                MapArea.UndeadPurgatory,
                MapArea.HarvestValley,
                MapArea.EarthenPeak,
                MapArea.IronKeep,
                MapArea.BelfrySol,
                MapArea.ShadedWoods,
                MapArea.DoorsOfPharros,
                MapArea.Tseldora,
                MapArea.ThePit,
                MapArea.GraveOfSaints,
                MapArea.TheGutter,
                MapArea.BlackGulch,
                MapArea.DrangleicCastle,
                MapArea.ShrineOfAmana,
                MapArea.UndeadCrypt,
                MapArea.AldiasKeep,
                MapArea.DragonAerie,
                MapArea.DragonShrine,
                MapArea.MemoryOfJeigh,
                MapArea.MemoryOfOrro,
                MapArea.MemoryOfVammar,
                MapArea.ShulvaSanctumCity,
                MapArea.DragonsSanctum,
                MapArea.CaveOfTheDead,
                MapArea.BrumeTower,
                MapArea.IronPassage,
                MapArea.MemoryOfTheOldIronKing,
                MapArea.FrozenEleumLoyce,
                MapArea.FrigidOutskirts,
            };

            // Create dictionary:
            Map2Id = new(); // Reset dictionary of Map -> int ID
            for (int i = 0; i < MapList.Count; i++)
                Map2Id[MapList[i]] = i;
            Id2Map = Map2Id.ToDictionary(x => x.Value, x => x.Key); // reverse lookup

            // Define area connections (hardcoded)
            Dictionary<MapArea, List<MapArea>> Connections = new()
            {
                [MapArea.ThingsBetwixt] = new()
                {
                    MapArea.Majula
                },

                [MapArea.Majula] = new()
                {
                    MapArea.ThingsBetwixt,
                    MapArea.FOFG,
                    MapArea.HeidesTowerOfFlame,
                    MapArea.HuntsmansCopse,
                    MapArea.ShadedWoods,
                    MapArea.ThePit,
                },

                [MapArea.FOFG] = new()
                {
                    MapArea.Majula,
                    MapArea.MemoryOfJeigh,
                    MapArea.MemoryOfOrro,
                    MapArea.MemoryOfVammar,
                    MapArea.TheLostBastille
                },

                [MapArea.HeidesTowerOfFlame] = new()
                {
                    MapArea.Majula,
                    MapArea.NoMansWharf,
                },
                
                [MapArea.NoMansWharf] = new()
                {
                    MapArea.HeidesTowerOfFlame,
                    MapArea.TheLostBastille,
                },
                
                [MapArea.TheLostBastille] = new()
                {
                    MapArea.FOFG,
                    MapArea.NoMansWharf,
                    MapArea.BelfryLuna,
                    MapArea.SinnersRise,
                },
                    
                [MapArea.BelfryLuna] = new() { MapArea.TheLostBastille },
                    
                [MapArea.SinnersRise] = new() { MapArea.TheLostBastille },
                    
                [MapArea.HuntsmansCopse] = new()
                {
                    MapArea.Majula,
                    MapArea.UndeadPurgatory,
                    MapArea.HarvestValley,
                },
                    
                [MapArea.UndeadPurgatory] = new() { MapArea.HuntsmansCopse, },
                    
                [MapArea.HarvestValley] = new()
                {
                    MapArea.HuntsmansCopse,
                    MapArea.EarthenPeak,
                },
                    
                [MapArea.EarthenPeak] = new()
                {
                    MapArea.HarvestValley,
                    MapArea.IronKeep,
                },
                    
                [MapArea.IronKeep] = new()
                {
                    MapArea.EarthenPeak,
                    MapArea.BelfrySol,
                    MapArea.BrumeTower,
                },
                    
                [MapArea.BelfrySol] = new() { MapArea.IronKeep, },
                    
                [MapArea.ShadedWoods] = new()
                {
                    MapArea.Majula,
                    MapArea.DoorsOfPharros,
                    MapArea.FrozenEleumLoyce,
                    MapArea.DrangleicCastle,
                    MapArea.AldiasKeep,
                },

                [MapArea.DoorsOfPharros] = new()
                {
                    MapArea.ShadedWoods,
                    MapArea.Tseldora,
                },
                [MapArea.Tseldora] = new()
                {
                    MapArea.DoorsOfPharros,
                },
                
                [MapArea.ThePit] = new()
                {
                    MapArea.Majula,
                    MapArea.GraveOfSaints,
                    MapArea.TheGutter,
                },
                
                [MapArea.GraveOfSaints] = new()
                {
                    MapArea.ThePit,
                },
                [MapArea.TheGutter] = new()
                {
                    MapArea.ThePit,
                    MapArea.BlackGulch,
                },
                [MapArea.BlackGulch] = new()
                {
                    MapArea.TheGutter,
                    MapArea.ShulvaSanctumCity,
                },
                
                [MapArea.DrangleicCastle] = new()
                {
                    MapArea.ShadedWoods,
                    MapArea.FrozenEleumLoyce,
                    MapArea.ShrineOfAmana
                },
                
                [MapArea.ShrineOfAmana] = new()
                { 
                    MapArea.DrangleicCastle,
                    MapArea.UndeadCrypt, 
                },
                
                [MapArea.UndeadCrypt] = new() { MapArea.ShrineOfAmana, },
                
                [MapArea.AldiasKeep] = new() 
                { 
                    MapArea.ShadedWoods,
                    MapArea.DragonAerie, 
                },
                
                [MapArea.DragonAerie] = new() 
                { 
                    MapArea.AldiasKeep, 
                    MapArea.DragonShrine
                },
                
                [MapArea.DragonShrine] = new() { MapArea.DragonAerie, },
                
                [MapArea.MemoryOfJeigh] = new() { MapArea.FOFG, },
                [MapArea.MemoryOfOrro] = new() { MapArea.FOFG },
                [MapArea.MemoryOfVammar] = new() { MapArea.FOFG },
                
                [MapArea.ShulvaSanctumCity] = new()
                {
                    MapArea.BlackGulch,
                    MapArea.DragonsSanctum,
                    MapArea.CaveOfTheDead,
                },

                [MapArea.DragonsSanctum] = new() { MapArea.ShulvaSanctumCity, },
                
                [MapArea.CaveOfTheDead] = new() { MapArea.ShulvaSanctumCity, },
                
                [MapArea.BrumeTower] = new()
                {
                    MapArea.IronKeep,
                    MapArea.IronPassage,
                    MapArea.MemoryOfTheOldIronKing,
                },
                [MapArea.IronPassage] = new() { MapArea.BrumeTower, },
                
                [MapArea.MemoryOfTheOldIronKing] = new() { MapArea.BrumeTower, },
                
                [MapArea.FrozenEleumLoyce] = new()
                {
                    MapArea.ShadedWoods,
                    MapArea.DrangleicCastle,
                    MapArea.FrigidOutskirts,
                },

                [MapArea.FrigidOutskirts] = new() { MapArea.FrozenEleumLoyce, }
            };

            // Create Adjacency matrix
            int N = MapList.Count;
            RandoGraph = new int[N,N];
            foreach (var kvp in Connections)
            {
                // Unpack:
                var row = Map2Id[kvp.Key];
                var conmaps = kvp.Value;

                // Populate connections
                foreach (var conmap in conmaps)
                {
                    var col = Map2Id[conmap];
                    RandoGraph[row,col] = 1;
                }
            }

        }
        private static int SteinerTreeDist(int[,] graph, List<int> terminals, out List<int> steinsol)
        {
            // Guard clauses:
            if (terminals.Count < 1)
                throw new Exception("I think you should not get here");
            if (terminals.Count == 1)
            {
                steinsol = terminals; // Betwixt only!
                return 1;
            }

            
            List<int> minSpanUnion = new();
            foreach (int tsink in terminals.TakeLast(terminals.Count - 1))
            {
                // Shortest point-to-point (from betwixt)
                var pathP2P = Dijkstras(graph, new List<int> { Map2Id[MapArea.ThingsBetwixt], tsink });

                // Add new distinct nodes to ongoing list:
                minSpanUnion = minSpanUnion.Union(pathP2P).ToList();
            }

            steinsol = minSpanUnion;
            return minSpanUnion.Count;

        }
        private static List<int> Dijkstras(int[,] graph, List<int> terminals)
        {
            // <This isn't actually dijstras, its some poor mans BFS I guess>
            var src = terminals[0];
            
            List<List<int>> initPaths = new() { new() { src } };
            List<List<int>> prevPaths;
            int depth = 0;
            
            prevPaths = initPaths;
            while (true)
            {
                var bsol = AddLayer(depth++, graph, terminals, prevPaths, out var newPaths, out var pathSol);
                if (bsol)
                    return pathSol; // exit

                // Next loop:
                prevPaths = newPaths;
            }
        }
        private static List<MapArea> HelperListID2Maps(List<int> ids)
        {
            // Add for further investigation and keep going
            var newpath_map = new List<MapArea>();
            foreach (var id in ids)
                newpath_map.Add(Id2Map[id]);
            return newpath_map;
        }

        // Recursion, save end result in pathsol
        private static bool AddLayer(int depth, int[,] graph, List<int> terminals, List<List<int>> prevPathsList, 
                                    out List<List<int>> newPathsList, out List<int> pathSol)
        {
            // Get new-depth PathsList
            newPathsList = new();
            pathSol = new();

            foreach (var path in prevPathsList)
            {
                // all connections from end node
                int row = path[^1];
                var connNodes = GetConnections(graph, row);

                foreach (var nd in connNodes)
                {
                    // Ignore cycles
                    if (path.Contains(nd))
                        continue;

                    // Add new node to path
                    var newpath = new List<int>(path) { nd }; // copy list & add node index nd

                    // Check for completion:
                    if (terminals.All(nd => newpath.Contains(nd)))
                    {
                        // In our case, the first time we hit it (in BFS) will be the shortest
                        // distance since it's undirected all weight 1!
                        pathSol = newpath;
                        return true;
                    }
                    newPathsList.Add(newpath);
                }
            }

            // No new paths added. Complete.
            if (newPathsList.Count == 0)
                throw new Exception("Unconnected nodes");
            return false;
        }
        private static List<int> GetConnections(int [,] graph, int src)
        {
            // list all nodes that connect to source node (except itself)
            List<int> connNodes = new();
            int DIMCOL = 1;
            int Ncols = graph.GetLength(DIMCOL);
            for (int i = 0; i < Ncols; i++)
            {
                if (i != src && graph[src, i] == 1)
                    connNodes.Add(i);
            }
            return connNodes;
        }
        private void SetupTravNodes()
        {
            // Each Node is a group of Rdzs that
            // exactly share MapArea & KeySet
            //
            // Any nodes with no KeySet reqs are considered
            // "unlocked" and safe for placement. Other
            // nodes will become unlocked as their keys
            // are placed.
            var ShopLotsPTF = AllPTF.Where(rdz => rdz is not DropRdz).ToList();
            var grps = ShopLotsPTF.GroupBy(rdz => rdz.RandoInfo.NodeKey)
                                   .Where(grp => !grp.Key.BadArea);

            // Create initial dictionary:
            foreach (var grp in grps)
                Nodes[grp.Key] = new Node(grp);
        }
        private void SolveBasicAreas()
        {
            // Fastest path to each area if all keys unlocked
            foreach (var map in Enum.GetValues(typeof(MapArea)).Cast<MapArea>())
            {
                if (map == MapArea.Undefined) continue;
                if (map == MapArea.Quantum) continue;

                List<int> terminals = new() { Map2Id[MapArea.ThingsBetwixt], Map2Id[map] };
                _ = SteinerTreeDist(RandoGraph, terminals, out var pathsol);
                AreaPaths[map] = pathsol.ToHashSet();
            }
        }
        
        // helpers:
        private void AddToAllLinkedRdz(Randomization rdz, DropInfo di)
        {
            // This is preliminary code if you want to randomize reinforcement/infusion
            FixReinforcement(di);
            FixInfusion(di);

            // Get linked Rdzs:
            List<Randomization> linkedrdz = new();
            if (!ReservedRdzs.ContainsKey(rdz))
                linkedrdz.Add(rdz);
            else
                linkedrdz = ReservedRdzs.Where(kvp => kvp.Value == di.ItemID)
                                        .Select(kvp => kvp.Key).ToList();

            foreach(var lrdz in linkedrdz)
                AddToRdz(lrdz, di);

        }
        private static void AddToRdz(Randomization rdz, DropInfo di)
        {
            rdz.AddShuffledItem(di);
        }
        private void FillLeftovers()
        {
            // ld: list of DropInfos
            int Nfc = ItemSetBase.FillerItems.Count; // fill count
            foreach (var rdz in AllPTF)
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

        private static void FixInfusion(DropInfo di)
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
        private static void FixReinforcement(DropInfo di)
        {
            var item = di.AsItemRow();
            var maxupgrade = GetItemMaxUpgrade(item);
            di.Reinforcement = (byte)Math.Min(di.Reinforcement, maxupgrade); // limit to item max upgrade
        }

        // Memory modification:
        internal static void WriteAllLots(List<ItemLotRow> lots)
        {
            lots.ForEach(lot => lot.StoreRow());
            ParamMan.ItemLotOtherParam?.WriteModifiedParam();
        }
        internal static void WriteAllDrops(List<ItemLotRow> lots)
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
            var shuffledlots = AllP.OfType<LotRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllLots(shuffledlots);
        }
        internal void WriteShuffledDrops()
        {
            var shuffleddrops = AllP.OfType<DropRdz>()
                                    .Where(ldz => ldz.ShuffledLot is not null)
                                    .Select(ldz => ldz.ShuffledLot).ToList();
            WriteAllDrops(shuffleddrops);
        }
        internal void WriteShuffledShops()
        {
            var shuffledshops = AllP.OfType<ShopRdz>().Select(sdz => sdz.ShuffledShop).ToList();
            WriteAllShops(shuffledshops);
        }

        // Utility:
        internal static Dictionary<int, string> ReadShopNames()
        {
            Dictionary<int, string> shopnames = new();

            // Read all:
            var lines = File.ReadAllLines("./Resources/Paramdex_DS2S_09272022/ShopLineupParam.txt");

            // Setup parser:
            Regex re = new(@"(?<paramid>\d+) (?<desc>.*)");
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
            List<string> lines = new()
            {
                // Intro line
                $"Printing key locations for seed {CurrSeed}",
                "---------------------------------------------"
            };

            // Main print loop
            foreach (int keyid in ItemSetBase.KeyOutputOrder.Cast<int>())
            {
                var rdzsWithKey = AllPTF.Where(rdz => rdz.HasShuffledItemID(keyid)).ToList();
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
            foreach (var ldz in AllPTF.OfType<LotRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Shops:
            lines.Add("");
            lines.Add("Shops:");
            foreach (var rdz in AllPTF.OfType<ShopRdz>())
                lines.Add(rdz.GetNeatDescription());

            // Enemy drops:
            lines.Add("");
            lines.Add("Enemy Drops:");
            foreach (var ldz in AllPTF.OfType<DropRdz>())
                lines.Add(ldz.GetNeatDescription());

            // Write file:
            File.WriteAllLines("./all_answers.txt", lines.ToArray());
        }

        // Miscellaneous post-processing
        internal void FixShopCopies()
        {
            // Maughlin / Gilligan / Gavlan
            var fillbycopy = AllP.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.FILL_BY_COPY).ToList();
            var done_shops = AllPTF.OfType<ShopRdz>();

            //// Define shops that need handling:
            //var LEvents = ShopRules.GetLinkedEvents();
            //var shopcopies = LEvents.Where(lev => lev.IsCopy && !lev.IsTrade);

            
            foreach (var shp in fillbycopy)
            {
                //var LE = shopcopies.FirstOrDefault(lev => lev.FillByCopy == shp.UniqueParamID) ?? throw new Exception("Cannot find linked event");
                var shop_to_copy = done_shops.First(srdz => srdz.UniqueParamID == shp?.RandoInfo?.RefInfoID);
                                                    //.FirstOrDefault() ?? throw new Exception("Cannot find shop to copy from");

                // Fill by copy:
                shp.ShuffledShop.CopyCoreValuesFrom(shop_to_copy.ShuffledShop);
                shp.MarkHandled();
            }
        }
        internal void FixNormalTrade()
        {
            var normal_trades = AllPTF.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.UNLOCKTRADE).ToList();
            foreach (var shp in normal_trades)
            {
                shp.ShuffledShop.EnableFlag = -1;  // enable (show) immediately (except Ornifex "1" trades that are locked behind event)
                shp.ShuffledShop.DisableFlag = -1;
                shp.MarkHandled();
            }
        }
        internal void FixShopSustains()
        {
            // Don't allow these events to be disabled
            var sustain_shops = AllPTF.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.SHOPSUSTAIN).ToList();
            foreach (var shp in sustain_shops)
            {
                shp.ShuffledShop.DisableFlag = -1; // Never disable
                shp.MarkHandled();
            }
        }
        internal void FixShopTradeCopies()
        {
            // Ornifex (non-free)
            var fillbycopy = AllP.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.TRADE_SHOP_COPY).ToList();
            var filled_shops = AllPTF.OfType<ShopRdz>();

            //// Define shops that need handling:
            //var LEvents = ShopRules.GetLinkedEvents();
            //var tradecopies = LEvents.Where(lev => lev.IsCopy && lev.IsTrade);

            foreach (var shp in fillbycopy)
            {
                //var LE = tradecopies.FirstOrDefault(lev => lev.FillByCopy == shp.UniqueParamID) ?? throw new Exception("Cannot find linked event");
                var shop_to_copy = filled_shops.Where(srdz => srdz.UniqueParamID == shp?.RandoInfo?.RefInfoID).First();
                                                //.FirstOrDefault() ?? throw new Exception("Cannot find shop to copy from");

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
            var shops_makefree = AllPTF.OfType<ShopRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.FREETRADE);
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
                                       .Where(rdz => rdz.Type == RDZ_TASKTYPE.SHOPREMOVE);
            foreach (var shp in shops_toremove)
            {
                shp.ZeroiseShuffledShop();
                shp.MarkHandled();
            }
        }
        internal void FixLotCopies()
        {
            var fillbycopy = AllP.OfType<LotRdz>()
                                 .Where(rdz => rdz.Type == RDZ_TASKTYPE.FILL_BY_COPY);
            foreach (var lot in fillbycopy)
            {
                // Get Randomized ItemLot to copy from:
                var lot_to_copy = AllPTF.OfType<LotRdz>().Where(ldz => ldz.UniqueParamID == lot?.RandoInfo?.RefInfoID).First();

                // Clone/Update:
                lot.ShuffledLot = lot.VanillaLot.CloneBlank();              // keep param reference for this ID
                lot.ShuffledLot.CloneValuesFrom(lot_to_copy.ShuffledLot);   // set to new values
                lot.MarkHandled();
            }
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
