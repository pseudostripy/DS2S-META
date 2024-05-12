using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SItem : IComparable<DS2SItem>
    {
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
        
        public override string ToString() => Name;
        public int CompareTo(DS2SItem? other) => Name.CompareTo(other?.Name);
        public bool NameContains(string txtfrag)
        {
            // Used for easier filtering
            return Name.ToLower().Contains(txtfrag.ToLower());
        }
    }
}
