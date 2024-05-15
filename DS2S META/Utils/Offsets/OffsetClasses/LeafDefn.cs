using DS2S_META.Utils.Offsets.OffsetClasses;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public class LeafDefn
    {
        public string Identifier;
        public List<LocatorDefn> Locators;

        public LeafDefn(string id, List<LocatorDefn> locators)
        {
            Identifier = id;
            Locators = locators;
        }
        public LeafDefn(string id, params LocatorDefn[] locators)
        {
            Identifier = id;
            Locators = locators.ToList();
        }
    }
}
