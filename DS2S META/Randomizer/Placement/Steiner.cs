using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS2S_META.Randomizer.Placement
{
    /// <summary>
    /// Things related to distance calculations. Tried to keep as much of
    /// it together as possible.
    /// </summary>
    internal class Steiner
    {
        // Fields:
        internal int[,] RandoGraph;
        private Dictionary<KEYID, HashSet<int>> KeySteinerNodes = new(); // NodeID lookup
        private Dictionary<MapArea, HashSet<int>> AreaPaths = new();    // NodeID lookup
        internal Dictionary<NodeKey, Node> Nodes = new();
        internal static Dictionary<MapArea, int> Map2Id = new();
        internal static Dictionary<int, MapArea> Id2Map = new();
        private List<KeySet> UniqueIncompleteKSs;
        private List<KeySet> UniqueIncompleteKSOrig;

        // Static Fields/Definitions:
        public static List<MapArea> MapList = new() // "Column/row lookup"
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
        public static Dictionary<MapArea, List<MapArea>> Connections = new()
        {
            [MapArea.ThingsBetwixt] = new() { MapArea.Majula },

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
            [MapArea.Tseldora] = new() { MapArea.DoorsOfPharros },

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
        
        Presanitizer Scope;
        PlacementManager PlaceMan; // for placement information

        // Constructor
        internal Steiner(PlacementManager placeman, Presanitizer scope) 
        {
            Scope = scope;
            SetupAreasGraph();
            SetupNodesDict();
            SolveBasicAreas(); // solution if keys aren't a concept
            SetupUniqueKS();
        }

        internal void Reinitialize()
        {
            // more todo?
            KeySteinerNodes = new(); // NodeID lookup
            SetupUniqueKS();
        }


        // One-time setup (per scope initialization)
        private void SetupAreasGraph()
        {
            // Define the adjacency matrix for connected game areas
            // Create dictionary:
            Map2Id = new(); // Reset dictionary of Map -> int ID
            for (int i = 0; i < MapList.Count; i++)
                Map2Id[MapList[i]] = i;
            Id2Map = Map2Id.ToDictionary(x => x.Value, x => x.Key); // reverse lookup

            // Define area connections (hardcoded)
            // Create Adjacency matrix
            int N = MapList.Count;
            RandoGraph = new int[N, N];
            foreach (var kvp in Connections)
            {
                // Unpack:
                var row = Map2Id[kvp.Key];
                var conmaps = kvp.Value;

                // Populate connections
                foreach (var conmap in conmaps)
                {
                    var col = Map2Id[conmap];
                    RandoGraph[row, col] = 1;
                }
            }
        }
        private void SetupNodesDict()
        {
            // Each Node is a group of Rdzs that share both MapArea & KeySet
            //
            // Any nodes with no KeySet reqs are considered "unlocked" and safe
            // for placement. Other nodes will become unlocked as their keys are placed.
            var allptf = Scope.AllPtf;
            var shopLotsPTF = allptf.Where(rdz => rdz is not DropRdz).ToList();
            var grps = shopLotsPTF.GroupBy(rdz => rdz.RandoInfo.NodeKey)
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
        private void SetupUniqueKS()
        {
            // only need to calc once
            if (UniqueIncompleteKSOrig != null)
                SetupUniqueKSOnce();

            if (UniqueIncompleteKSOrig == null) throw new Exception("Issue creating orig incomplete keysets");
            UniqueIncompleteKSs = new List<KeySet>(UniqueIncompleteKSOrig); // work with copy
        }
        private void SetupUniqueKSOnce()
        {
            // Generate list of not yet unlocked KeySets
            List<KeySet> ksuniques = new();
            var shoplots = Scope.AllPtf.Where(rdz => rdz is not DropRdz).ToList();
            foreach (var rdz in shoplots)
            {
                if (rdz.RandoInfo.IsKeyless)
                    continue;

                foreach (var ks in rdz.RandoInfo.KSO)
                {
                    if (!ksuniques.Contains(ks))
                        ksuniques.Add(ks);
                }
            }
            UniqueIncompleteKSOrig = ksuniques;
        }


        public void UpdateSteinerNodesOnKey(KEYID keyid, NodeKey hitNodeKey)
        {
            // Get node we're placing into, and calculate new Steiner set and distance
            var rdzNode = Nodes[hitNodeKey]; // where we're placing key
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
                    var allnodes = AddHashset(myhash, AreaPaths[node.NodeKey.Area]).ToList();

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
        private List<KeySet> GetNewUnlocks()
        {
            // Check over the incomplete keysets and see what completes:
            List<KeySet> newKeys = new();
            foreach (var ks in UniqueIncompleteKSs)
            {
                if (ks.Keys.All(keyid => PlaceMan.KeysPlacedSoFar.Contains((int)keyid)))
                    newKeys.Add(ks);
            }
            return newKeys;
        }
        private static HashSet<int> AddHashset(HashSet<int> src, HashSet<int> newset)
        {
            foreach (var i in newset)
                src.Add(i);
            return src;
        }

        public int GetSteinerTreeDist(NodeKey nodekey, out List<int> steinsol)
        {
            // get node we're trying to place into
            var node = Nodes[nodekey];
            if (node.IsLocked)
                throw new Exception("Shouldn't get here. Locked rdzs should have been caught in SoftlockRes checks");
            var terminals = node.SteinerNodes;

            if (RandoGraph == null)
                throw new Exception("Steiner class not intiialized");
            return SteinerTreeDist(RandoGraph, terminals, out steinsol);
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
        private static List<int> GetConnections(int[,] graph, int src)
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
        
    }
}
