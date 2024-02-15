using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace DS2S_META
{
    /// <summary>
    /// Top-level class for defining and loading lists of hand-coded 
    /// DS2 resources for usage across META
    /// </summary>
    internal class DS2Resource
    {
        public static List<DS2SItemCategory> ItemCategories = new();
        public static List<DS2SItem> Items;
        public static Dictionary<int, string> ItemNames;
        public static List<DS2SItem> Weapons;
        public static List<DS2SBonfire> Bonfires;
        public static List<DS2SClass> Classes;

        public static readonly string? ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static readonly string PathCategories = "Resources/Equipment/DS2SItemCategories.txt";
        public static readonly string PathBonfires = "Resources/Systems/Bonfires.txt";
        public static readonly string PathClasses = "Resources/Systems/Classes.txt";

        private static readonly List<ITEMCATEGORY> WepTypes = new() { ITEMCATEGORY.MeleeWeapon, ITEMCATEGORY.RangedWeapons, ITEMCATEGORY.Shields, ITEMCATEGORY.StaffChimes };
        private static readonly Dictionary<int, DS2SBonfire> BonfireHashDict;


        // One-time setup (static constructors are "called at most once" in C#)
        static DS2Resource()
        {
            // Parse & group ItemCategories
            var protocats = ParseResource(PathCategories, DS2SItemCategoryEntry.ParseNew);
            var groups = protocats.GroupBy(pcat => pcat.Type);
            foreach (var group in groups)
            {
                var type = group.Key;
                var childpaths = group.Select(p => p.Path).ToList();
                var childnames = group.Select(p => p.Name).ToList();
                var childlists = group.Select(p => ParseResource(p.Path, DS2SItem.ParseNew));
                ItemCategories.Add(new DS2SItemCategory(type, childpaths, childnames, childlists));
            }
                
            // Query handy things
            Items = ItemCategories.SelectMany(p => p.Items).ToList();
            ItemNames = Items.ToDictionary(it => it.ItemId, it => it.Name);
            Weapons = ItemCategories.Where(cat => WepTypes.Contains(cat.Type)).SelectMany(cat => cat.Items).ToList();

            /////////////////////////////////
            Bonfires = ParseResource(PathBonfires, DS2SBonfire.ParseNew);
            Classes = ParseResource(PathClasses, DS2SClass.ParseNew);

            // Setup fast lookup:
            BonfireHashDict = Bonfires.ToDictionary(bf => Bfidhash(bf.AreaID, bf.ID), bf => bf);
        }
        private static int Bfidhash(int areaid, int id) => areaid*10000 + id; // maybe?
        public static DS2SBonfire? LookupBonfire(int areaid, int id)
        {
            return BonfireHashDict.TryGetValue(Bfidhash(areaid, id), out var bonfire) ? bonfire : null;
        }

        public static List<T> ParseResource<T>(string path, Func<string, T>parser)
        {
            // read and split entries:
            var alltext = GetTxtResource(path);
            var entries = alltext.RegexSplit("[\r\n]+")
                                 .Where(IsValidTxtResource);
            List<T> objs = new();
            foreach(var entry in entries)
                objs.Add(parser(entry));
            return objs;
        }

        public static string GetTxtResource(string filePath)
        {
            //Get local directory + file path, read file, return string contents of file
            return File.ReadAllText($@"{ExeDir}/{filePath}");
        }
        public static bool IsValidTxtResource(string txtLine)
        {
            //see if txt resource line is valid and should be accepted 
            //(bare bones, only checks for a couple obvious things)
            if (txtLine.Contains("//"))
                txtLine = txtLine[..txtLine.IndexOf("//")]; // keep everything until '//' comment

            return !string.IsNullOrWhiteSpace(txtLine); //empty line check
        }
        
    }
}
