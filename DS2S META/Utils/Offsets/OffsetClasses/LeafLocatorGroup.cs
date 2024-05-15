using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class LeafLocatorGroup
    {
        public string ParentPtrId;
        public List<DS2VER> ValidVersions;
        public List<OffsetLocator> Leaves;

        public LeafLocatorGroup(string parentid,  List<DS2VER> validVersions)
        {
            ParentPtrId = parentid;
            ValidVersions = validVersions;
            Leaves = new List<OffsetLocator>();
        }
        public LeafLocatorGroup(string parentid, List<DS2VER> validVersions, List<OffsetLocator> leaves)
        {
            ParentPtrId = parentid;
            ValidVersions = validVersions;
            Leaves = leaves;
        }
    }
}
