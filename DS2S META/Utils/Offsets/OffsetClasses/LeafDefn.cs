using DS2S_META.Utils.Offsets.CodeLocators;
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
        public string Id;
        public List<LocatorDefn> Locators;

        public LeafDefn(string id, List<LocatorDefn> locators)
        {
            Id = id;
            Locators = locators;
        }
        public LeafDefn(string id, params LocatorDefn[] locators)
        {
            Id = id;
            Locators = locators.ToList();
        }

        public LeafCL? GetLocator(DS2VER ver)
        {
            var locdef = Locators.FirstOrDefault(lo => lo.ValidVersions.Contains(ver));
            if (locdef == null) return null;
            return (LeafCL)locdef.CodeLocator;
        }
    }
}
