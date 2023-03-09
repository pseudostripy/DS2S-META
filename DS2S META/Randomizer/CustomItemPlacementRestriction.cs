using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public enum ItemRestrictionType
    {
        Anywhere,
        Vanilla,
        AreaDistance
    }

    internal abstract class CustomItemPlacementRestriction
    {
        internal abstract List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR);

        internal abstract bool ArePlacementLocationsExpendable();

        internal abstract List<int> ExpandPlacements(in List<int> unfilledLocations, in List<Randomization> AllPTR);
    }

    // Allows any location for the item
    internal class NoPlacementRestriction : CustomItemPlacementRestriction
    {
        internal override List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            return unfilledLocations;
        }

        internal override bool ArePlacementLocationsExpendable() { return false; }

        internal override List<int> ExpandPlacements(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            throw new NotImplementedException("Attempted to expand possible placements of an unexpendable restriction type.");
        }
    }

    // Allows only the item's vanilla location
    internal class VanillaPlacementRestriction : CustomItemPlacementRestriction
    {
        internal int ItemID;
        internal VanillaPlacementRestriction(int itemID = 0)
        {
            ItemID = itemID;
        }

        internal override List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            List<int> indices = new();
            foreach (int index in unfilledLocations)
            {
                if (AllPTR[index].HasVannilaItemID(ItemID))
                {
                    indices.Add(index);
                }
            }

            if (indices.Count > 0)
            {
                return indices;
            }

            throw new Exception("Vanilla location wasn't present among unfilled locations - probably error in logic (order of item placement)");
        }

        internal override bool ArePlacementLocationsExpendable() { return false; }

        internal override List<int> ExpandPlacements(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            throw new NotImplementedException("Attempted to expand possible placements of an unexpendable restriction type.");
        }
    }

    // Allows placement in areas adhering to the specified distance bounds from the provided area.
    // If no places in the selected areas would allow the placement of the item, new areas can be iteratively added, from the ones closest to the bounds.
    internal class AreaDistancePlacementRestriction : CustomItemPlacementRestriction
    {
        MapArea Area;
        int LowerBound = 0;
        int UpperBound = int.MaxValue;

        int LowerDistanceArrayIndex = 0;
        int UpperDistanceArrayIndex = 0;

        internal AreaDistancePlacementRestriction(MapArea area, int lowerBound, int upperBound)
        {
            Area = area;

            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        // Gets locations within the distance boundaries
        internal override List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            var areasSortedByDistance = AreaDistanceCalculator.SortedAreasByDistanceMatrix[(int)Area];

            LowerDistanceArrayIndex = areasSortedByDistance.ToList().FindIndex(kvp => kvp.Key >= LowerBound);
            UpperDistanceArrayIndex = areasSortedByDistance.ToList().FindLastIndex(kvp => kvp.Key <= UpperBound);

            // Lower bound is bigger than any distance in the vector
            if (LowerDistanceArrayIndex == -1)
            {
                LowerDistanceArrayIndex = areasSortedByDistance.Length;
            }

            var areasWithinUpperBounds = new ArraySegment<KeyValuePair<int, MapArea>>(areasSortedByDistance, 0, UpperDistanceArrayIndex + 1).Select(DistanceToArea => DistanceToArea.Value);
            var areasWithinLowerBounds = new ArraySegment<KeyValuePair<int, MapArea>>(areasSortedByDistance, LowerDistanceArrayIndex, areasSortedByDistance.Length - LowerDistanceArrayIndex).Select(DistanceToArea => DistanceToArea.Value);

            IEnumerable<MapArea> areasWithinBounds = LowerBound > UpperBound ? areasWithinLowerBounds.Union(areasWithinUpperBounds) : areasWithinLowerBounds.Intersect(areasWithinUpperBounds);

            List<int> locations = new();

            foreach (var index in unfilledLocations)
            {
                if (AreaDistanceCalculator.IsLocationInAreas(AllPTR[index].ParamID, areasWithinBounds))
                {
                    locations.Add(index);
                }
            }

            return locations;
        }

        internal override bool ArePlacementLocationsExpendable() { return true; }

        internal override List<int> ExpandPlacements(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            var distances = AreaDistanceCalculator.SortedAreasByDistanceMatrix[(int)Area].ToList();

            // Can't expand anymore
            if (LowerBound <= UpperBound && LowerDistanceArrayIndex == 0 && UpperDistanceArrayIndex == distances.Count - 1
                || LowerBound > UpperBound && LowerDistanceArrayIndex - UpperDistanceArrayIndex <= 1)
            {
                return new();
            }

            // Distances from bounds to closest areas, which haven't been used yet
            int closerAreaDistance = LowerDistanceArrayIndex == 0 ? int.MaxValue : LowerBound - distances[LowerDistanceArrayIndex - 1].Key;
            int furtherAreaDistance = UpperDistanceArrayIndex == distances.Count - 1 ? int.MaxValue : distances[UpperDistanceArrayIndex + 1].Key - UpperBound;

            // Get list of yet unused areas, which are closest to the bounds
            List<MapArea> closestAreas = new();

            if (closerAreaDistance <= furtherAreaDistance)
            {
                closestAreas.AddRange(distances.Where(a => a.Key == LowerBound - closerAreaDistance).Select((value, index) => value.Value));
                LowerDistanceArrayIndex = distances.FindIndex(distance => distance.Key == LowerBound - closerAreaDistance);
            }

            if (closerAreaDistance >= furtherAreaDistance)
            {
                closestAreas.AddRange(distances.Where(a => a.Key == UpperBound + furtherAreaDistance).Select((value, index) => value.Value));
                UpperDistanceArrayIndex = distances.FindLastIndex(distance => distance.Key == UpperBound + furtherAreaDistance);
            }

            // Get locations in those areas
            List<int> locations = new();

            foreach (var index in unfilledLocations)
            {
                if (AreaDistanceCalculator.IsLocationInAreas(AllPTR[index].ParamID, closestAreas))
                {
                    locations.Add(index);
                }
            }

            return locations;
        }
    }

}
