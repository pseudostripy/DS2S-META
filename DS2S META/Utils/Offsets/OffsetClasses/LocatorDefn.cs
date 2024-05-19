using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META;
using DS2S_META.Utils.Offsets.CodeLocators;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class LocatorDefn
    {
        public List<DS2VER> ValidVersions;
        public CodeLocator CodeLocator;
       
        // Constructors
        public LocatorDefn(List<DS2VER> validVersions, CodeLocator codeloc)
        {
            CodeLocator = codeloc;
            ValidVersions = validVersions;
        }
    }
}
