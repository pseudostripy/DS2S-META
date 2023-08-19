using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer.Placement
{
    /// <summary>
    /// Encapsulates information from all different placement-type checks
    /// </summary>
    internal class PlaceResult
    {
        // Fields
        public ReservedRes? ReservedRes;
        public SoftlockRes? SoftlockRes;
        public CategoryRes? CategoryRes;
        public DistanceRes? DistanceRes;

        // Constructor
        public PlaceResult() { }

        public void AddReservedRes(ReservedRes reservedRes) { ReservedRes = reservedRes; }
        public void AddSoftlockRes(SoftlockRes softlockRes) { SoftlockRes = softlockRes; }
        public void AddCategoryRes(CategoryRes categoryRes) { CategoryRes = categoryRes; }
        public void AddDistanceRes(DistanceRes distanceRes) { DistanceRes = distanceRes; }

        // Logic checks:
        public bool PassedReservedCond => ReservedRes?.Passed == true;
        public bool PassedSoftlockCond => SoftlockRes?.Passed == true;
        public bool PassedCategoryCond => CategoryRes?.Passed == true;
        public bool PassedDistanceCond => DistanceRes?.Passed == true;
        public bool IsVanillaPlacement => ReservedRes?.IsVanillaPlacement == true;
        public bool IsDelayVanLocked => IsVanillaPlacement && !PassedSoftlockCond;
    }
}
