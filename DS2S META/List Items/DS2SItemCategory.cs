using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META
{
    internal class DS2SItemCategory
    {
        public string Name;
        public List<DS2SItem> Items;

        private static Regex ItemEntryRx = new Regex(@"^(?<id>\S+) (?<show>\S+) (?<path>\S+) (?<name>.+)$");
        private DS2SItemCategory(string name, int type, string itemList, bool showIDs)
        {
            Name = name;
            Items = new List<DS2SItem>();
            foreach (string line in GetTxtResourceClass.RegexSplit(itemList, "[\r\n]+"))
            {
                if (GetTxtResourceClass.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    Items.Add(new DS2SItem(line, type, showIDs));
            };
            Items.Sort();
        }
        private DS2SItemCategory(string config)
        {
            Match itemEntry = ItemEntryRx.Match(config);
            Name = itemEntry.Groups["name"].Value;
            Items = new List<DS2SItem>();
            foreach (string line in GetTxtResourceClass.RegexSplit(GetTxtResourceClass.GetTxtResource(itemEntry.Groups["path"].Value), "[\r\n]+"))
            {
                if (GetTxtResourceClass.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    Items.Add(new DS2SItem(line, Convert.ToInt32(itemEntry.Groups["id"].Value), bool.Parse(itemEntry.Groups["show"].Value)));
            };
            Items.Sort();
        }
        public override string ToString()
        {
            return Name;
        }
        static DS2SItemCategory()
        {
            foreach (string line in GetTxtResourceClass.RegexSplit(GetTxtResourceClass.GetTxtResource("Resources/Equipment/DS2SItemCategories.txt"), "[\r\n]+"))
            {
                if (GetTxtResourceClass.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    All.Add(new DS2SItemCategory(line));
            };
            AllItems = All.SelectMany(cat => cat.Items).ToList();
            var wepcatnames = new List<string>() { "Melee Weapons", "Ranged Weapons", "Staff/Chimes" };
            AllWeapons = All.Where(cat => wepcatnames.Contains(cat.Name))
                            .SelectMany(cat => cat.Items).ToList();      
        }

        public static List<DS2SItemCategory> All = new List<DS2SItemCategory>();
        public static List<DS2SItem> AllItems = new List<DS2SItem>();
        public static List<DS2SItem> AllWeapons = new List<DS2SItem>();
    }
}
