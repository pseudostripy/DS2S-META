using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Cut-down version of ItemRestriction for usage with Rando logic
    /// </summary>
    internal class Restriction
    {
        public RestrType Type;
        public int ItemId;
        public MinMax Dist;

        public Restriction(RestrType type, int itemId, MinMax dist)
        {
            Type = type;
            ItemId = itemId;
            Dist = dist;
        }
    }
}
