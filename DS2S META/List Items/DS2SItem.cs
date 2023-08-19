using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SItem : IComparable<DS2SItem>
    {
        private static readonly Regex ItemEntryRx = new(@"^\s*(?<id>\S+)\s+(?<name>.+)$");

        public string Name;
        public int ItemId;
        //public int itemID => Type == ITEMCATEGORY.Armor ? ID - 10000000 : ID; // Interface fix for DS2SItem to normal itemID
        

        public DS2SItem(int itemid, string name)
        {
            ItemId = itemid;
            Name = name;
        }
        public static DS2SItem ParseNew(string lineentry)
        {
            Match m = ItemEntryRx.Match(lineentry);
            var itemid = Convert.ToInt32(m.Groups["id"].Value);
            var name = m.Groups["name"].Value;
            return new DS2SItem(itemid, name);
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
