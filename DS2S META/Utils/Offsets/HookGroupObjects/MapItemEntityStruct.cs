using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class MapItemEntityStruct
    {
        public long unkn1 { get; set; }
        public MapItemEntityData MapItemEntityData { get; set; }

        public PHPointer PHMapItemEntityData { get; set; }
        public PHPointer PHMapItemEntityStruct { get; set; } // ptr to here!

        public MapItemEntityStruct(DS2SHook hook, PHPointer _PHMapItemEntityStruct) 
        {
            PHMapItemEntityStruct = _PHMapItemEntityStruct;
            unkn1 = PHMapItemEntityStruct.ReadInt64(0x0);

            int whois = PHMapItemEntityStruct.ReadInt32(0x8 + 0x0);
            short bagtype = PHMapItemEntityStruct.ReadInt16(0x8 + 0x4);
            short bagid = PHMapItemEntityStruct.ReadInt16(0x8 + 0x6);
            MapItemEntityData = new MapItemEntityData(whois, bagtype, bagid);

            // just in case we need it one day
            PHMapItemEntityData = hook.CreateBasePointer(PHMapItemEntityStruct.Resolve() + 0x8);
        }
    }
}
