using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    // "OFfset Locator Definition"
    public class OFLD : LocatorDefn
    {
        public OFLD(List<DS2VER> validVersions, string parentId, params int[] offsets)
        {
            CodeLocator = new OffsetLocator(parentId,offsets);
            ValidVersions = validVersions;
        }
    }
}
