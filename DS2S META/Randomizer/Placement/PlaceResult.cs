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
        public bool SoftlockPass;
        public CategoryRes? CategoryRes;
        public DistanceRes? DistanceRes;
        public bool DelayKey;

        // Constructor
        public PlaceResult() { }

        public void AddReservedRes(ReservedRes reservedRes) { ReservedRes = reservedRes; }
        public void AddSoftlockRes(bool didsoftlock) { SoftlockPass = !didsoftlock; }
        public void AddCategoryRes(CategoryRes categoryRes) { CategoryRes = categoryRes; }
        public void AddDistanceRes(DistanceRes distanceRes) { DistanceRes = distanceRes; }

        public void MarkAsDelay() { DelayKey = true; }

        // Logic checks:
        public bool PassedReservedCond => ReservedRes?.Passed == true;
        public bool PassedSoftlockCond => SoftlockPass;
        public bool PassedCategoryCond => CategoryRes?.Passed == true;
        public bool PassedDistanceCond => DistanceRes?.Passed == true;
        public bool PassedAll => PassedReservedCond && PassedSoftlockCond && PassedCategoryCond && PassedDistanceCond;
        public bool IsVanillaPlacement => ReservedRes?.IsVanillaPlacement == true;
        public bool IsDelayVanLocked => IsVanillaPlacement && !PassedSoftlockCond;
        public bool IsDistanceSoftFail => DistanceRes?.SoftFail == true; // exceeds max/min conditions
        public bool FailTooNear => DistanceRes?.FailTooNear == true;
        public bool FailTooFar => DistanceRes?.FailTooFar == true;
        public bool IsHardFail()
        {
            if (!PassedReservedCond) return true;
            if (!PassedSoftlockCond) return true;
            if (!PassedCategoryCond) return true;
            if (DistanceRes?.HardFail == true) return true;
            return false;
        }
        public bool RequiresDelay() { return DelayKey; }
    }
}
