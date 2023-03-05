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
        internal int ItemID;

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
        internal VanillaPlacementRestriction(int itemID)
        {
            ItemID = itemID;
        }

        internal override List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {
            //for (int index = 0; index < unfilledLocations.Count; index++)
            //{
            //    if (AllPTR[unfilledLocations[index]].HasVannilaItemID(ItemID))
            //    {
            //        return new List<int> { unfilledLocations[index] };
            //    }
            //}

            foreach (int index in unfilledLocations)
            {
                if (AllPTR[index].HasVannilaItemID(ItemID))
                {
                    return new() { index };
                }
            }

            throw new Exception("Vanilla location wasn't provided among unfilled locations - probably error in logic");
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

        internal AreaDistancePlacementRestriction(int itemId, MapArea area, int lowerBound, int upperBound)
        {
            Area = area;
            ItemID = itemId;

            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        // Gets locations within the distance boundaries
        internal override List<int> GetFeasibleLocations(in List<int> unfilledLocations, in List<Randomization> AllPTR)
        {

            LowerDistanceArrayIndex = AreaDistanceCalculator.SortedAreasByDistanceMatrix[(int)Area].ToList().FindIndex(kvp => kvp.Key >= LowerBound);
            UpperDistanceArrayIndex = AreaDistanceCalculator.SortedAreasByDistanceMatrix[(int)Area].ToList().FindLastIndex(kvp => kvp.Key <= UpperBound);

            var areasWithinBounds = new ArraySegment<KeyValuePair<int, MapArea>>(AreaDistanceCalculator.SortedAreasByDistanceMatrix[(int)Area], LowerDistanceArrayIndex, UpperDistanceArrayIndex - LowerDistanceArrayIndex + 1)
                .Select(DistanceToArea => DistanceToArea.Value);

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
            if (LowerDistanceArrayIndex == 0 && UpperDistanceArrayIndex == distances.Count - 1)
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
