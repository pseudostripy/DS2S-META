using Octokit;
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
    public enum ITEMCATEGORY
    {
        MeleeWeapon = 0,
        Armor = 1,
        Item = 2,
        Gesture = 3,
        KeyItems = 4,
        RemovedItems = 5,
        RangedWeapons = 6,
        Ammo = 7,
        Rings = 8,
        Shields = 9,
        Spells = 10,
        StaffChimes = 11,
    }
    internal class DS2SItemCategory
    {
        public ITEMCATEGORY Type;
        public IEnumerable<string> ChildNames;
        public IEnumerable<string> ChildPaths;
        private IEnumerable<IEnumerable<DS2SItem>> ChildLists; // unsure if ever going to be useful
        public IEnumerable<DS2SItem> Items;
        
        public DS2SItemCategory(ITEMCATEGORY type, IEnumerable<string> childnames, 
                                 IEnumerable<string> childpaths, IEnumerable<IEnumerable<DS2SItem>> childlists)
        {
            Type = type;
            ChildNames = childnames;
            ChildPaths = childpaths;
            ChildLists = childlists;
            Items = childlists.SelectMany(x => x);
        }
    }
}
