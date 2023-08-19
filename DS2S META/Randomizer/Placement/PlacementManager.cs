using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        IEnumerable<Diset> Disets { get; set; }

        public PlacementManager(IEnumerable<Diset> disets) 
        {
            Disets = disets;
        }

        internal void PlaceSets()
        {
            foreach (var diset in Disets)
                PlaceSet(diset);
        }
        internal void PlaceSet(List<DropInfo> ld)
        {
            // ld: list of DropInfos
            while (ld.Count > 0)
            {
                if (UnfilledRdzs.Count == 0)
                    break;

                int keyindex = Rng.Next(ld.Count);
                DropInfo di = ld[keyindex]; // get item to place

                if (ResVanPlacedSoFar.Contains(di.ItemID))
                {
                    // All Vanilla instances were placed on a previous
                    // call to this which had the same ID.
                    ld.RemoveAt(keyindex);
                    continue;
                }


                var logicres = PlaceItem(di, settype);
                if (settype == SetType.Keys &&
                    logicres == LOGICRES.DELAY_VANLOCKED || logicres == LOGICRES.DELAY_MAXDIST)
                    continue; // leave in pool and redraw

                if (logicres == LOGICRES.SUCCESS_VANPLACE)
                    ResVanPlacedSoFar.Add(di.ItemID);

                // Item placed successfully
                ld.RemoveAt(keyindex);

            }

            // Must have ran out of space to place things:
            if (ld.Count > 0 && settype != SetType.Gens)
                throw new Exception("Ran out of space to place keys/reqs. Likely querying issue.");
        }
        private LOGICRES PlaceItem(DropInfo di, SetType stype)
        {
            // Placement logic:
            var logicres = FindElligibleRdz(di, stype, out var rdz);
            if (logicres == LOGICRES.DELAY_VANLOCKED || logicres == LOGICRES.DELAY_MAXDIST)
                return logicres; // handled above

            // Extra checks:
            if (IsFailure(logicres))
                throw new Exception("Shouldn't get here");
            if (rdz == null)
                throw new NullReferenceException();

            // Place Item:
            AddToAllLinkedRdz(rdz, di);

            // Update graph/logic on key placement
            if (stype == SetType.Keys)
                UpdateForNewKey(rdz, di);

            // Handle saturation
            if (!rdz.IsSaturated())
                return logicres;

            rdz.MarkHandled();
            if (UnfilledRdzs.Count == 0) throw new Exception("No places left available to place item");
            UnfilledRdzs.Remove(rdz); // now filled!
            return logicres;
        }
    }
}
