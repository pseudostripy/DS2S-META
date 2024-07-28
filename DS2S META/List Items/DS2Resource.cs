using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using DS2S_META.List_Items;
using DS2S_META.Utils;

namespace DS2S_META
{
    /// <summary>
    /// Top-level class for defining and loading lists of hand-coded 
    /// DS2 resources for usage across META
    /// </summary>
    internal class DS2Resource
    {
        public static readonly string? ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static List<DS2SItemCategory> ItemCategories = new();
        public static List<DS2SItem> Items;
        public static Dictionary<int, string> ItemNames;
        public static List<DS2SItem> Weapons;
        public static List<DS2SBonfire> Bonfires;
        public static List<DS2SClass> Classes;
        public static List<DS2SBonfireHub> BonfireHubs;
        public static List<DS2SCovenant> Covenants;

        //public static readonly string? ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string PathCategories = "Resources/Equipment/DS2SItemCategories.txt";
        private static readonly string PathBonfires = "Resources/Systems/Bonfires.txt";
        private static readonly string PathClasses = "Resources/Systems/Classes.txt";
        private static readonly string PathBonfireHubs = "Resources/Systems/BonfireHubs.txt";
        private static readonly string PathCovenants = "Resources/Systems/Covenants.txt";

        private static readonly List<ITEMCATEGORY> WepTypes = new() { ITEMCATEGORY.MeleeWeapon, ITEMCATEGORY.RangedWeapons, ITEMCATEGORY.Shields, ITEMCATEGORY.StaffChimes };
        private static readonly Dictionary<long, DS2SBonfire> BonfireHashDict;


        // One-time setup (static constructors are "called at most once" in C#)
        static DS2Resource()
        {
            // Parse & group ItemCategories
            var protocats = ResParseLibrary.Parse(PathCategories, ResParseLibrary.ParseToItemCategory);
            var groups = protocats.GroupBy(pcat => pcat.Type);
            foreach (var group in groups)
            {
                var type = group.Key;
                var childpaths = group.Select(p => p.Path).ToList();
                var childnames = group.Select(p => p.Name).ToList();
                var childlists = group.Select(p => ResParseLibrary.Parse(p.Path, ResParseLibrary.ParseToItem));
                ItemCategories.Add(new DS2SItemCategory(type, childpaths, childnames, childlists));
            }
                
            // Query handy things
            Items = ItemCategories.SelectMany(p => p.Items).ToList();
            ItemNames = Items.ToDictionary(it => it.ItemId, it => it.Name);
            Weapons = ItemCategories.Where(cat => WepTypes.Contains(cat.Type)).SelectMany(cat => cat.Items).ToList();

            /////////////////////////////////
            Bonfires = ResParseLibrary.Parse(PathBonfires, ResParseLibrary.ParseToBonfire);
            var unlinkedBfHubs = ResParseLibrary.Parse(PathBonfireHubs, ResParseLibrary.ParseToBonfireHub);
            BonfireHubs = unlinkedBfHubs.Select(ubfh => DS2SBonfireHub.LinkBonfireObjects(ubfh, Bonfires)).ToList();
            Classes = ResParseLibrary.Parse(PathClasses, ResParseLibrary.ParseToDS2Class);
            foreach (var bf in Bonfires)
                bf.Hub = BonfireHubs.Where(bfh => bfh.Bonfires.Contains(bf)).FirstOrDefault();
            
            // Setup fast lookup:
            BonfireHashDict = Bonfires.ToDictionary(bf => Bfidhash(bf.AreaID, bf.ID), bf => bf);

            /////////////////////////////////
            Covenants = ResParseLibrary.Parse(PathCovenants, ResParseLibrary.ParseToCovenant);
        }
        private static long Bfidhash(int areaid, int id) => areaid*0x10000 + id; // maybe?
        public static DS2SBonfire? LookupBonfire(int areaid, int id)
        {
            return BonfireHashDict.TryGetValue(Bfidhash(areaid, id), out var bonfire) ? bonfire : null;
        }
        public static DS2SBonfire GetBonfireByName(string name)
        {
            var bf = Bonfires.Where(bf => bf.Name == name).FirstOrDefault();
            if (bf != null)
                return bf;

            // Issue
            MetaExceptionStaticHandler.Raise($"Cannot establish link between bonfire name {name} and Hook property.");
            return DS2SBonfire.EmptyBonfire;
        }
        public static DS2SCovenant GetCovById(COV? id) => Covenants.First(cv => cv.ID == id);
    }
}
