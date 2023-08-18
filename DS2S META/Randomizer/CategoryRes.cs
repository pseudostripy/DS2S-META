using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Encapsulates information relating to Rdz PickupType
    /// e.g. things like "can it be in a wooden chest?"
    /// </summary>
    internal class CategoryRes
    {
        // Fields
        public REASON Reason;
        public int Distance;

        public enum REASON
        {
            VANOVERRIDE, // bypass all checks since we're doing a Vanilla placement
            RACEKEYPASS, // racemode + key + valid
            RACEKEYFAIL, // racemode + key + invalid
            VALIDRDZ,    // no type restrictions hit
            FORBIDDENTYPE, // rdz contains forbidden pickuptype flag for this item category
        }

        // Constructor:
        private CategoryRes(REASON reason) { Reason = reason; }

        // helper factory methods:
        public static CategoryRes VanillaOverride => new(REASON.VANOVERRIDE);
        public static CategoryRes RaceKeyPass => new(REASON.RACEKEYPASS);
        public static CategoryRes RaceKeyFail => new(REASON.RACEKEYFAIL);
        public static CategoryRes ValidPickupTypes => new(REASON.VALIDRDZ);
        public static CategoryRes ForbiddenPickupTypes => new(REASON.FORBIDDENTYPE);

        // Wider logic utility
        public static List<REASON> LogicPasses = new()
        {
            REASON.VANOVERRIDE, REASON.RACEKEYPASS, REASON.VALIDRDZ
        };
        public bool Passed => LogicPasses.Contains(Reason);
    }
}
