using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public abstract class CodeLocator
    {
        //internal string Name { get; set; } // propertyname
        //internal string Type
        public abstract PHPointer Register();
    }
}
