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
        public List<LeafDefn> Leaves;

        public LeafLocatorGroup(string parentid, params DetachedLeaf[] detleaves)
        {
            ParentPtrId = parentid;

            // update information and convert to generalized format
            Leaves = new List<LeafDefn>();
            foreach (var detl in detleaves)
            {
                var locatorDefns = detl.VerOffs.Select
                            (vo => new LocatorDefn(vo.ValidVersions, new OffsetLocator(parentid, vo.Offsets))).ToList();
                Leaves.Add(new LeafDefn(detl.Id, locatorDefns));
            }
        }
    }
}
