using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class CovLocator
    {
        public CodeLocator Discovered;
        public CodeLocator Rank;
        public CodeLocator Progress;

        public CovLocator(int doff, int roff, int poff)
        {
            //int PlayerParam = 0xA;
            //Discovered = new OffsetLocator(PlayerParam, doff);
            //Rank = new OffsetLocator(PlayerParam, roff);
            //Progress = new OffsetLocator(PlayerParam, poff);
        }
    }
}
