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
        public bool HasVerLocator(DS2VER ver) => Locators.Any(x => x.ValidVersions.Contains(ver));
        public CodeLocator? TryGetVerLocator(DS2VER ver) => Locators.FirstOrDefault(x => x.ValidVersions.Contains(ver))?.CodeLocator;
        public CodeLocator GetVerLocator(DS2VER ver) => Locators.First(x => x.ValidVersions.Contains(ver)).CodeLocator;
    }
}
