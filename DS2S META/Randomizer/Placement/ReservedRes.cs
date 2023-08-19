using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer.Placement
{
    /// <summary>
    /// DistanceResult. Encapsulates information from a Steiner Distance Check
    /// </summary>
    internal class ReservedRes
    {
        // Fields
        public LOGICRES LogicRes;
        public int Distance;

        public ReservedRes(LOGICRES logicres)
        {
            LogicRes = logicres;
        }

        private static List<LOGICRES> LogicFails = new()
        {
            LOGICRES.FAIL_DIST_NOTAPPLICABLE,
            LOGICRES.FAIL_DIST_TOO_NEAR,
            LOGICRES.FAIL_DIST_TOO_FAR,
            LOGICRES.FAIL_SOFTLOCK
        };
        public static List<LOGICRES> LogicPasses = new()
        {
            LOGICRES.SUCCESS,
            LOGICRES.SUCCESS_VANPLACE
        };

        public bool Passed => LogicPasses.Contains(LogicRes);
        public bool IsVanillaPlacement => LogicRes == LOGICRES.SUCCESS_VANPLACE;
    }
}
