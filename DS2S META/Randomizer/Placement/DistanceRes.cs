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
    internal class DistanceRes
    {
        // Fields
        public REASON Reason;
        public int Distance;

        public enum REASON
        {
            NORESTRICTION,  // no distance constraint (common)
            INCALCULABLE,   // represents Quantum rdzs (eg Lucatiel). Currently this should be impossible.
            INDISTLOGIC,    // good pass: rdz abides by distance constraints
            TOONEAR,        // failed dist constraint: rdz too close
            TOOFAR,         // failed dist constraint: rdz too far
        }

        // Constructor:
        private DistanceRes(REASON reason) { Reason = reason; }
        private DistanceRes(REASON reason, int dist) 
        { 
            Reason = reason; 
            Distance = dist;
        }

        // helper factory methods:
        public static DistanceRes NoRestriction => new(REASON.NORESTRICTION);
        public static DistanceRes Incalculable => new(REASON.INCALCULABLE);
        public static DistanceRes DistancePass(int dist) => new(REASON.INDISTLOGIC, dist);
        public static DistanceRes TooNear(int dist) => new(REASON.TOONEAR, dist);
        public static DistanceRes TooFar(int dist) => new(REASON.TOOFAR, dist);

        // Wider logic utility
        public static List<REASON> LogicPasses = new() { REASON.NORESTRICTION, REASON.INDISTLOGIC };
        public bool Passed => LogicPasses.Contains(Reason);
    }
}
