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
    internal class DS2SItemCategoryEntry
    {
        public string Name;
        public ITEMCATEGORY Type;
        public string Path;
        
        private static readonly Regex ItemCategoryRx = new(@"^(?<id>\S+) (?<show>\S+) (?<path>\S+) (?<name>.+)$");

        // Constructor
        private DS2SItemCategoryEntry(ITEMCATEGORY type, string name, string path)
        {
            Name = name;
            Type = type;
            Path = path;
        }

        // Exposed factory constructor
        public static DS2SItemCategoryEntry ParseNew(string txtline)
        {
            // Unpack category entry regex:
            Match m = ItemCategoryRx.Match(txtline);
            var id = (ITEMCATEGORY)int.Parse(m.Groups["id"].Value);
            var name = m.Groups["name"].Value;
            var path = m.Groups["path"].Value;
            return new DS2SItemCategoryEntry(id, name, path);
        }
        public override string ToString() => Name;
    }
}
