using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class PointerDefn
    {
        public string Identifier;
        public List<LocatorDefn> Locators;

        public PointerDefn(string id, List<LocatorDefn> locators)        
        {
            Identifier = id;
            Locators = locators;
        }
        public PointerDefn(string id, params LocatorDefn[] locators)
        {
            Identifier = id;
            Locators = locators.ToList();
        }
    }
}
