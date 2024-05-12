using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DS2S_META
{
    public class DS2SItemCategoryEntry
    {
        public string Name;
        public ITEMCATEGORY Type;
        public string Path;

        // Constructor
        public DS2SItemCategoryEntry(ITEMCATEGORY type, string name, string path)
        {
            Name = name;
            Type = type;
            Path = path;
        }
        public override string ToString() => Name;
    }
}
