using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SItem : IComparable<DS2SItem>
    {
        private static readonly Regex ItemEntryRx = new(@"^\s*(?<id>\S+)\s+(?<metashowtype>\d)\s+(?<name>.+)$");

        public string Name;
        public int ItemId;
        public int MetaShowType;

        public bool ShowInMeta => MetaShowType == 1; // adjust as necessary
        //public int itemID => Type == ITEMCATEGORY.Armor ? ID - 10000000 : ID; // Interface fix for DS2SItem to normal itemID

        public DS2SItem(int itemid, string name, int metashowtype)
        {
            ItemId = itemid;
            Name = name;
            MetaShowType = metashowtype;
        }
        public static DS2SItem ParseNew(string lineentry)
        {
            Match m = ItemEntryRx.Match(lineentry);
            var itemid = Convert.ToInt32(m.Groups["id"].Value);
            var metashowtype = Convert.ToInt32(m.Groups["metashowtype"].Value);
            var name = m.Groups["name"].Value;
            return new DS2SItem(itemid, name, metashowtype);
        }

        public override string ToString() => Name;
        public int CompareTo(DS2SItem? other) => Name.CompareTo(other?.Name);
        public bool NameContains(string txtfrag)
        {
            // Used for easier filtering
            return Name.ToLower().Contains(txtfrag.ToLower());
        }
    }
}
