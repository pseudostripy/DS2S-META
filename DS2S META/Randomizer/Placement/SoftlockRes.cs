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
    internal class SoftlockRes
    {
        // Fields
        public REASON Reason;
        public int Distance;

        public enum REASON
        {
            NOTREQUIRED, // logic not required: nothing locked behind it
            INLOGIC,     // already unlocked in logic
            SOFTLOCK,    // not unlocked in logic yet
            DEADLOCKOVERRIDE, // memes related to circular blocking things that won't be an issue
        }

        // Constructor:
        private SoftlockRes(REASON reason) { Reason = reason; }

        // helper factory methods:
        public static SoftlockRes NotRequired => new(REASON.NOTREQUIRED);
        public static SoftlockRes InLogic => new(REASON.INLOGIC);
        public static SoftlockRes Softlock => new(REASON.SOFTLOCK);
        public static SoftlockRes DeadlockOverride => new(REASON.DEADLOCKOVERRIDE);

        // Wider logic utility
        public static List<REASON> LogicPasses = new()
        {
            REASON.NOTREQUIRED, REASON.INLOGIC, REASON.DEADLOCKOVERRIDE
        };
        public bool Passed => LogicPasses.Contains(Reason);
    }
}
