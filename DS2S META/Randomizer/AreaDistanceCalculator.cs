using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;

namespace DS2S_META.Randomizer
{
    internal static class AreaDistanceCalculator
    {
        // Price for area transition [from][to] = price
        internal static Dictionary<MapArea, Dictionary<MapArea, int>> ConnectedAreaDistances = new()
        {
            [MapArea.ThingsBetwixt] = new() { [MapArea.Majula] = 1 },
            [MapArea.Majula] = new()
            {
                [MapArea.ThingsBetwixt] = 1,
                [MapArea.ForestOfFallenGiants] = 1,
                [MapArea.HeidesTowerOfFlame] = 1,
                [MapArea.HuntsmansCopse] = 1,
                [MapArea.ShadedWoods] = 1,
                [MapArea.ThePit] = 1
            },
            [MapArea.ForestOfFallenGiants] = new()
            {
                [MapArea.Majula] = 1,
                [MapArea.TheLostBastille] = 1,
                [MapArea.MemoryOfOrro] = 1,
                [MapArea.MemoryOfJeigh] = 1,
                [MapArea.MemoryOfVammar] = 1
            },
            [MapArea.HeidesTowerOfFlame] = new()
            {
                [MapArea.Majula] = 1,
                [MapArea.CathedralOfBlue] = 1,
                [MapArea.NoMansWharf] = 1
            },
            [MapArea.CathedralOfBlue] = new() { [MapArea.HeidesTowerOfFlame] = 1 },
            [MapArea.NoMansWharf] = new()
            {
                [MapArea.HeidesTowerOfFlame] = 1,
                [MapArea.TheLostBastille] = 1
            },
            [MapArea.TheLostBastille] = new()
            {
                [MapArea.ForestOfFallenGiants] = 1,
                [MapArea.NoMansWharf] = 1,
                [MapArea.BelfryLuna] = 1,
                [MapArea.SinnersRise] = 1
            },
            [MapArea.BelfryLuna] = new() { [MapArea.TheLostBastille] = 1, /*[MapArea.BellKeepers] = 0*/ },
            [MapArea.SinnersRise] = new() { [MapArea.TheLostBastille] = 1, [MapArea.Majula] = 1 },
            [MapArea.HuntsmansCopse] = new()
            {
                [MapArea.Majula] = 1,
                [MapArea.UndeadPurgatory] = 1,
                [MapArea.HarvestValley] = 1
            },
            [MapArea.UndeadPurgatory] = new() { [MapArea.HuntsmansCopse] = 1 },
            [MapArea.HarvestValley] = new()
            {
                [MapArea.HuntsmansCopse] = 1,
                [MapArea.EarthenPeak] = 1
            },
            [MapArea.EarthenPeak] = new()
            {
                [MapArea.HarvestValley] = 1,
                [MapArea.IronKeep] = 1
            },
            [MapArea.IronKeep] = new()
            {
                [MapArea.EarthenPeak] = 1,
                [MapArea.BelfrySol] = 1,
                [MapArea.BrumeTower] = 1,
                [MapArea.Majula] = 1
            },
            [MapArea.BelfrySol] = new() { [MapArea.IronKeep] = 1, /*[MapArea.BellKeepers] = 0*/ },
            [MapArea.ShadedWoods] = new()
            {
                [MapArea.Majula] = 1,
                [MapArea.DoorsOfPharros] = 1,
                [MapArea.ShrineOfWinter] = 1,
                [MapArea.AldiasKeep] = 1,
                //[MapArea.PilgrimsOfDark] = 0
            },
            [MapArea.DoorsOfPharros] = new()
            {
                [MapArea.ShadedWoods] = 1,
                [MapArea.BrightstoneCoveTseldora] = 1,
                //[MapArea.RatKingCovenant] = 0
            },
            [MapArea.BrightstoneCoveTseldora] = new()
            {
                [MapArea.DoorsOfPharros] = 1,
                [MapArea.DragonMemories] = 1,
                [MapArea.LordsPrivateChamber] = 1
            },
            [MapArea.LordsPrivateChamber] = new() { [MapArea.BrightstoneCoveTseldora] = 1, [MapArea.Majula] = 1 },
            [MapArea.ThePit] = new()
            {
                [MapArea.Majula] = 1,
                [MapArea.GraveOfSaints] = 1,
                [MapArea.TheGutter] = 1
            },
            [MapArea.GraveOfSaints] = new()
            {
                [MapArea.ThePit] = 1,
                [MapArea.TheGutter] = 1,
                //[MapArea.RatKingCovenant] = 0
            },
            [MapArea.TheGutter] = new()
            {
                [MapArea.ThePit] = 1,
                [MapArea.GraveOfSaints] = 1,
                [MapArea.BlackGulch] = 1
            },
            [MapArea.BlackGulch] = new()
            {
                [MapArea.TheGutter] = 1,
                [MapArea.ShulvaSanctumCity] = 1,
                [MapArea.Majula] = 1,
                //[MapArea.PilgrimsOfDark] = 0
            },
            [MapArea.ShrineOfWinter] = new()
            {
                [MapArea.ShadedWoods] = 1,
                [MapArea.DrangleicCastle] = 1,
                [MapArea.FrozenEleumLoyce] = 1
            },
            [MapArea.DrangleicCastle] = new()
            {
                [MapArea.ShrineOfWinter] = 1,
                [MapArea.ThroneOfWant] = 1,
                [MapArea.KingsPassage] = 1,
                //[MapArea.PilgrimsOfDark] = 0
            },
            [MapArea.KingsPassage] = new() { [MapArea.DrangleicCastle] = 1, [MapArea.ShrineOfAmana] = 1 },
            [MapArea.ShrineOfAmana] = new() { [MapArea.KingsPassage] = 1, [MapArea.UndeadCrypt] = 1 },
            [MapArea.UndeadCrypt] = new() { [MapArea.ShrineOfAmana] = 1 },
            [MapArea.ThroneOfWant] = new() { [MapArea.DrangleicCastle] = 1 },
            [MapArea.AldiasKeep] = new() { [MapArea.ShadedWoods] = 1, [MapArea.DragonAerie] = 1 },
            [MapArea.DragonAerie] = new() { [MapArea.AldiasKeep] = 1, [MapArea.DragonShrine] = 1 },
            [MapArea.DragonShrine] = new() { [MapArea.DragonAerie] = 1 },
            [MapArea.MemoryOfJeigh] = new() { [MapArea.ForestOfFallenGiants] = 1 },
            [MapArea.MemoryOfOrro] = new() { [MapArea.ForestOfFallenGiants] = 1 },
            [MapArea.MemoryOfVammar] = new() { [MapArea.ForestOfFallenGiants] = 1 },
            [MapArea.DragonMemories] = new() { [MapArea.BrightstoneCoveTseldora] = 1 },
            [MapArea.ShulvaSanctumCity] = new()
            {
                [MapArea.BlackGulch] = 1,
                [MapArea.DragonsSanctum] = 1,
                [MapArea.CaveOfTheDead] = 1
            },
            [MapArea.DragonsSanctum] = new() { [MapArea.ShulvaSanctumCity] = 1, [MapArea.DragonsRest] = 1 },
            [MapArea.DragonsRest] = new() { [MapArea.DragonsSanctum] = 1 },
            [MapArea.CaveOfTheDead] = new() { [MapArea.ShulvaSanctumCity] = 1 },
            [MapArea.BrumeTower] = new()
            {
                [MapArea.IronKeep] = 1,
                [MapArea.IronPassage] = 1,
                [MapArea.MemoryOfTheOldIronKing] = 1
            },
            [MapArea.IronPassage] = new() { [MapArea.BrumeTower] = 1 },
            [MapArea.MemoryOfTheOldIronKing] = new() { [MapArea.BrumeTower] = 1 },
            [MapArea.FrozenEleumLoyce] = new()
            {
                [MapArea.ShrineOfWinter] = 1,
                [MapArea.GrandCathedral] = 1,
                [MapArea.FrigidOutskirts] = 1
            },
            [MapArea.GrandCathedral] = new() { [MapArea.FrozenEleumLoyce] = 1, [MapArea.TheOldChaos] = 1 },
            [MapArea.TheOldChaos] = new() { [MapArea.GrandCathedral] = 1 },
            [MapArea.FrigidOutskirts] = new() { [MapArea.FrozenEleumLoyce] = 1 }
        };

        internal static int AreaCount = Enum.GetValues(typeof(MapArea)).Length;
        internal static int[][] DistanceMatrix = new int[AreaCount][];
        internal static KeyValuePair<int, MapArea>[][] SortedAreasByDistanceMatrix = new KeyValuePair<int, MapArea>[AreaCount][];

        internal static void CalculateDistanceMatrix()
        {
            for (int areaID = 0; areaID < AreaCount; areaID++)
            {
                MapArea area = (MapArea)areaID;

                DistanceMatrix[areaID] = new int[AreaCount];
                Array.Fill<int>(DistanceMatrix[areaID], int.MaxValue, 0, AreaCount);
                DistanceMatrix[areaID][areaID] = 0;

                var areasToExpand = GetConnectedAreaDistancesList(area);
                while (areasToExpand.Count() > 0)
                {
                    var expandedArea = areasToExpand.First().Key;
                    var currentPrice = areasToExpand.First().Value;

                    if (currentPrice < DistanceMatrix[areaID][(int)expandedArea])
                    {
                        DistanceMatrix[areaID][(int)expandedArea] = currentPrice;
                        foreach (var kvp in GetConnectedAreaDistancesList(expandedArea))
                        {
                            if (kvp.Key == area)
                            {
                                continue;
                            }

                            areasToExpand.Add(new KeyValuePair<MapArea, int>(kvp.Key, kvp.Value + currentPrice));
                        }
                    }
                    areasToExpand.RemoveAt(0);
                }

                // Idea: Special Shrine of Winter path cost calculations for areas before the door - something like Max(Primals) + (ShadedWoods -> SoW)? Or perhaps even add cost of path to Shaded Woods from the area?
                // This logic wouldn't apply to areas after SoW, of course (and paths from SoW to SW), but either option would cause the cost of passing the SoW door from areas before Shaded Woods to be ridiculously high
                // For now, it could be simulated by increased Shaded Woods -> Shrine of Winter price

                // Simpler (?) version of this problem would be cost adjustment of key-dependent transitions based on the location of the necessary key, but that would require distance matrix calculation after the key placement phase
                // |-> Simpler perhaps in sense there's just one area you need to visit in order to be able to pass through the transition, but path to the key's area may need some other key(s), what would quickly complicate stuff
                // At that point, all transitions traversable only in one-way could be considered (eg. Pit/Grave->Gutter, Gutter->Gulch, Wharf/FoFG->Bastille, Copse->Valley
                // Then distance matrix would contain the price of traversing between areas, instead of distance of areas (and probably there
            }

            CreateSortedDistanceMatrix();
        }

        internal static void CreateSortedDistanceMatrix()
        {
            for (int areaID = 0; areaID < AreaCount; areaID++)
            {
                SortedAreasByDistanceMatrix[areaID] = DistanceMatrix[areaID].Select((distance, index) => new KeyValuePair<int, MapArea>(distance, (MapArea)index)).ToArray();
                Array.Sort(SortedAreasByDistanceMatrix[areaID], (a, b) => a.Key < b.Key ? -1 : a.Key == b.Key ? 0 : 1);
            }
        }

        internal static bool IsLocationInAreas(int locationID, IEnumerable<MapArea> areas)
        {
            foreach (var area in areas)
            {
                if (MapAreas.Items.ContainsKey(area) && MapAreas.Items[area].Contains(locationID))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool HasConnectedAreas(MapArea area)
        {
            return ConnectedAreaDistances.ContainsKey(area) && ConnectedAreaDistances[area].Any();
        }

        internal static List<KeyValuePair<MapArea, int>> GetConnectedAreaDistancesList(MapArea area)
        {
            if (HasConnectedAreas(area))
            {
                return ConnectedAreaDistances[area].ToList();
            }

            return new();
        }
    }
}
