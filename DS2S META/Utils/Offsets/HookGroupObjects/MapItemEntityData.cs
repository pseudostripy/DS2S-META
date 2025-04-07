using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class MapItemEntityData
    {
        public MapItemEntityData(int whois, short bagType, short bagid) 
        {
            WhoIs = whois;
            BagType = (BAGTYPE)bagType;
            BagId = bagid;
        }

        public int WhoIs { get; set; }
        public BAGTYPE BagType { get; set; }
        public short BagId { get; set; }
    }
}
