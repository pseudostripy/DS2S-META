using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace DS2S_META.List_Items
{
    public static class ResParseLibrary
    {
        // Generic IO
        public static readonly string? ExeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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

        public static List<T> Parse<T>(string path, Func<string, T> entityParser)
        {
            // read and split entries:
            var alltext = GetTxtResource(path);
            var entries = alltext.RegexSplit("[\r\n]+")
                                 .Where(IsValidTxtResource);
            List<T> objs = new();
            foreach (var entry in entries)
                objs.Add(entityParser(entry));
            return objs;
        }

        // Regex line parsers:
        private static readonly Regex ItemCategoryRx = new(@"^(?<id>\S+) (?<path>\S+) (?<name>.+)$");
        private static readonly Regex ItemEntryRx = new(@"^\s*(?<id>\S+)\s+(?<metashowtype>\d)\s+(?<name>.+)$");
        private static readonly Regex BonfireRE = new(@"^(?<area>\S+) (?<id>\S+) (?<name>.+)$");
        private static readonly Regex BonfireHubEntryRx = new(@"^(?<name>.+?):\s+(?<bonfires>.+)$"); // Regex won't catch names with ":"
        private static readonly Regex ClassEntryRx = new(@"^(?<id>\S+) (?<sl>\S+) (?<vig>\S+) (?<end>\S+) (?<vit>\S+) (?<att>\S+) (?<str>\S+) (?<dex>\S+) (?<adp>\S+) (?<int>\S+) (?<fth>\S+) (?<name>.+)$");
        private static readonly Regex CovenantEntryRx = new(@"^(?<id>\S+) (?<name>.+) \((?<levels>\S+)\)$");

        // Deserializers:
        public static DS2SBonfire ParseToBonfire(string line)
        {
            Match m = BonfireRE.Match(line);
            var areaId = Convert.ToInt32(m.Groups["area"].Value);
            var id = Convert.ToUInt16(m.Groups["id"].Value);
            var name = m.Groups["name"].Value;
            return new DS2SBonfire(areaId, id, name);
        }
        public static DS2SBonfireHub ParseToBonfireHub(string line)
        {
            Match m = BonfireHubEntryRx.Match(line);

            var name = m.Groups["name"].Value;
            var bfnames = m.Groups["bonfires"].Value
                                .Split('-')
                                .Select(x => x.Trim())
                                .ToList();
            return new DS2SBonfireHub(name, bfnames);
        }
        public static DS2SItem ParseToItem(string lineentry)
        {
            Match m = ItemEntryRx.Match(lineentry);
            var itemid = Convert.ToInt32(m.Groups["id"].Value);
            var metashowtype = Convert.ToInt32(m.Groups["metashowtype"].Value);
            var name = m.Groups["name"].Value;
            return new DS2SItem(itemid, name, metashowtype);
        }
        public static DS2SItemCategoryEntry ParseToItemCategory(string txtline)
        {
            // Unpack category entry regex:
            Match m = ItemCategoryRx.Match(txtline);
            var id = (ITEMCATEGORY)int.Parse(m.Groups["id"].Value);
            var path = m.Groups["path"].Value;
            var name = m.Groups["name"].Value;
            return new DS2SItemCategoryEntry(id, name, path);
        }
        public static DS2SClass ParseToDS2Class(string line)
        {
            var cls = new DS2SClass();
            Match classEntry = ClassEntryRx.Match(line);

            cls.Name = classEntry.Groups["name"].Value;
            cls.ID = (PLAYERCLASS)Enum.Parse(enumType: typeof(PLAYERCLASS), classEntry.Groups["id"].Value);
            cls.SoulLevel = Convert.ToInt16(classEntry.Groups["sl"].Value);
            cls.Vigor = Convert.ToInt16(classEntry.Groups["vig"].Value);
            cls.Endurance = Convert.ToInt16(classEntry.Groups["end"].Value);
            cls.Vitality = Convert.ToInt16(classEntry.Groups["vit"].Value);
            cls.Attunement = Convert.ToInt16(classEntry.Groups["att"].Value);
            cls.Strength = Convert.ToInt16(classEntry.Groups["str"].Value);
            cls.Dexterity = Convert.ToInt16(classEntry.Groups["dex"].Value);
            cls.Adaptability = Convert.ToInt16(classEntry.Groups["adp"].Value);
            cls.Intelligence = Convert.ToInt16(classEntry.Groups["int"].Value);
            cls.Faith = Convert.ToInt16(classEntry.Groups["fth"].Value);
            cls.BuildMinLevelsDict();
            return cls;
        }
        public static DS2SCovenant ParseToCovenant(string config)
        {
            Match covenantEntry = CovenantEntryRx.Match(config);
            var id = (COV)Convert.ToByte(covenantEntry.Groups["id"].Value);
            string name = covenantEntry.Groups["name"].Value;
            
            // convert levels to rank dictionary
            var strLevels = covenantEntry.Groups["levels"].Value.Split('/');
            var ranklevels = new Dictionary<int, int> { { 0, 0 } }; // all covenants default 0 prog to rank 0
            for (int i = 0; i < strLevels.Length; i++)
                ranklevels.Add(i + 1, int.Parse(strLevels[i]));
            return new DS2SCovenant(id, name, ranklevels);
        }

    }
}
