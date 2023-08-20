using DS2S_META.Utils;
using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class LotRdz : GLotRdz<ItemLotRow>
    {
        // Constructors:
        internal LotRdz(ItemLotRow vanlot, RandoInfo ri, RDZ_TASKTYPE status) : base(vanlot, ri, status)
        {
            IsDropTable = false;
        }
    }
}
