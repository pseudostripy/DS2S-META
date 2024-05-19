using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils.Offsets.CodeLocators;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class LeafLocatorGroup
    {
        public string GroupId;
        public string ParentPtrId;
        public List<LeafDefn> Leaves = new();

        public LeafLocatorGroup(string groupId,string parentid, params DetachedLeaf[] detleaves)
        {
            GroupId = groupId;
            ParentPtrId = parentid;

            // unpack/sanitize types
            foreach (var detleaf in detleaves)
            {
                var leafdefns = detleaf.VerOffs.Select(vo => DS2REData.OFLD(vo.ValidVersions, parentid, vo.Offset)).ToArray();
                Leaves.Add(new LeafDefn(detleaf.Id, leafdefns));
            }
        }
    }
}
