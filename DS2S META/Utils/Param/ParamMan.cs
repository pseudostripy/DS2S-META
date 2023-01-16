using PropertyHook;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.Utils.Offsets.DS2SOffsets;
using static SoulsFormats.PARAMDEF;
using SoulsFormats;
using DS2S_META.Randomizer;
using static DS2S_META.Utils.ItemRow;
using System.Reflection;

namespace DS2S_META.Utils
{
    public static class ParamMan
    {
        // Top-level Fields
        private static bool _loaded = false;
        public static bool IsLoaded
        {
            get => _loaded;
            private set => _loaded = value;
        }
        public static DS2SHook? Hook { get; set; }
        public static readonly string ExeDir = Environment.CurrentDirectory;
        public static List<Param> RawParamsList { get; set; } = new();
        public static Dictionary<string, Param> AllParams { get; set; } = new();
        public static List<Param> GeneratorParams { get; set; } = new();
        public static List<Param> GeneratorRegistParams { get; set; } = new();

        public static class PAliases
        {
            // Names matching underlying DEF
            public const string SHOP_LINEUP = "SHOP_LINEUP_PARAM";
            public const string WEAPON = "WEAPON_PARAM";
            public const string WEAPON_REINFORCE = "WEAPON_REINFORCE_PARAM";
            public const string ITEM = "ITEM_PARAM";
            public const string WEAPON_TYPE = "WEAPON_TYPE_PARAM";
            public const string CUSTOM_ATTR_SPEC = "CUSTOM_ATTR_SPEC_PARAM";
            public const string WEAPON_STATS_AFFECT = "WEAPON_STATS_AFFECT_PARAM";
            public const string PLAYER_STATUS = "PLAYER_STATUS_PARAM";
            public const string ITEM_USAGE = "ITEM_USAGE_PARAM";
            public const string ARROW = "ARROW_PARAM";
            public const string PLAYER_STATUS_ITEM = "PLAYER_STATUS_ITEM_PARAM";

            // Names different from underlying DEF
            public const string ITEM_LOT_OTHER = "ITEM_LOT_OTHER";
            public const string ITEM_LOT_CHR = "ITEM_LOT_CHR";
            public const string ENEMY_PARAM = "ENEMY_PARAM";

            public static List<string> GENERATOR_PARAM { get; set; } = new()
            {
                "generatorparam_m10_02_00_00",
                "generatorparam_m10_04_00_00",
                "generatorparam_m10_10_00_00",
                "generatorparam_m10_14_00_00",
                "generatorparam_m10_15_00_00",
                "generatorparam_m10_16_00_00",
                "generatorparam_m10_17_00_00",
                "generatorparam_m10_18_00_00",
                "generatorparam_m10_19_00_00",
                "generatorparam_m10_23_00_00",
                "generatorparam_m10_25_00_00",
                "generatorparam_m10_27_00_00",
                "generatorparam_m10_30_00_00",
                "generatorparam_m10_31_00_00",
                "generatorparam_m10_32_00_00",
                "generatorparam_m10_33_00_00",
                "generatorparam_m10_34_00_00",
                "generatorparam_m20_10_00_00",
                "generatorparam_m20_11_00_00",
                "generatorparam_m20_21_00_00",
                "generatorparam_m20_24_00_00",
                "generatorparam_m20_26_00_00",
                "generatorparam_m40_03_00_00",
                "generatorparam_m50_35_00_00",
                "generatorparam_m50_36_00_00",
                "generatorparam_m50_37_00_00",
                "generatorparam_m50_38_00_00",
            };
            public static List<string> GENERATOR_REGIST { get; set; } = new()
            {
                "generatorregistparam_m10_02_00_00",
                "generatorregistparam_m10_04_00_00",
                "generatorregistparam_m10_10_00_00",
                "generatorregistparam_m10_14_00_00",
                "generatorregistparam_m10_15_00_00",
                "generatorregistparam_m10_16_00_00",
                "generatorregistparam_m10_17_00_00",
                "generatorregistparam_m10_18_00_00",
                "generatorregistparam_m10_19_00_00",
                "generatorregistparam_m10_23_00_00",
                "generatorregistparam_m10_25_00_00",
                "generatorregistparam_m10_27_00_00",
                "generatorregistparam_m10_30_00_00",
                "generatorregistparam_m10_31_00_00",
                "generatorregistparam_m10_32_00_00",
                "generatorregistparam_m10_33_00_00",
                "generatorregistparam_m10_34_00_00",
                "generatorregistparam_m20_10_00_00",
                "generatorregistparam_m20_11_00_00",
                "generatorregistparam_m20_21_00_00",
                "generatorregistparam_m20_24_00_00",
                "generatorregistparam_m20_26_00_00",
                "generatorregistparam_m40_03_00_00",
                "generatorregistparam_m50_35_00_00",
                "generatorregistparam_m50_36_00_00",
                "generatorregistparam_m50_37_00_00",
                "generatorregistparam_m50_38_00_00",
            };
        };


        // Params Shorthand:
        public static Param? ShopLineupParam => AllParams[PAliases.SHOP_LINEUP];
        public static Param? ItemLotOtherParam => AllParams[PAliases.ITEM_LOT_OTHER];
        public static Param? ItemLotChrParam => AllParams[PAliases.ITEM_LOT_CHR];
        public static Param? WeaponParam => AllParams[PAliases.WEAPON];
        public static Param? ItemParam => AllParams[PAliases.ITEM];
        public static Param? WeaponReinforceParam => AllParams[PAliases.WEAPON_REINFORCE];
        public static Param? WeaponTypeParam => AllParams[PAliases.WEAPON_TYPE];
        public static Param? WeaponStatsAffect => AllParams[PAliases.WEAPON_STATS_AFFECT];
        public static Param? CustomAttrSpecParam => AllParams[PAliases.CUSTOM_ATTR_SPEC];
        public static Param? PlayerStatusClassParam => AllParams[PAliases.PLAYER_STATUS];
        public static Param? PlayerStatusItemParam => AllParams[PAliases.PLAYER_STATUS_ITEM];
        public static Param? ItemUsageParam => AllParams[PAliases.ITEM_USAGE];
        public static Param? ArrowParam => AllParams[PAliases.ARROW];
        public static Param? EnemyParam => AllParams[PAliases.ENEMY_PARAM];
        //public static Param? GenForestParam => AllParams[PAliases.GENFOREST];

        public static void Initialise(DS2SHook hook)
        {
            Hook = hook; // needed?
            GetParams();
            IsLoaded = true;
        }
        public static void Uninitialise()
        {
            // OnUnhooked
            RawParamsList = new();
            AllParams = new();
            _loaded = false;
        }

        private static void GetParams()
        {
            List<Param> paramList = new List<Param>();
            string resourcePath = $"{ExeDir}/Resources";
            string paramPath = $"{ExeDir}/Resources/Paramdex_DS2S_09272022";

            string pointerPath = $"{paramPath}/Pointers";
            
            // Add all [Memory-Pointer-Offset] based params:
            string paramPointers = $"{pointerPath}/ParamOffsets.txt";
            string[] pointers = File.ReadAllLines(paramPointers);
            AddMemoryParams(paramList, paramPath, paramPointers, pointers);
            AddFileParams(paramList);

            RawParamsList = paramList;
            BuildParamDictionary(); // Populate "AllParams"
        }
        private static void AddMemoryParams(List<Param> paramList, string paramPath, string path, string[] pointers)
        {
            foreach (string entry in pointers)
            {
                if (!Util.IsValidTxtResource(entry))
                    continue;

                string[] info = entry.TrimComment().Split(':');
                string name = info[1];
                string defName = info.Length > 2 ? info[2] : name;

                string defPath = $"{paramPath}/Defs/{defName}.xml";
                if (!File.Exists(defPath))
                    throw new($"The PARAMDEF {defName} does not exist for {entry}.");

                // Make param
                int[] offsets = info[0].Split(';').Select(s => hex2int(s)).ToArray();
                PHPointer pointer = GetParamPointer(offsets);
                PARAMDEF paramDef = XmlDeserialize(defPath);
                Param param = new(pointer, offsets, paramDef, name);

                // Currently this works for the names we've used. In general we need a dictionary:
                param.StringsFileName = param.Name.Replace("_", string.Empty);

                // Main Type Switch
                RowOverloadHandler(param);
                
                // Save param
                paramList.Add(param);
            }
            paramList.Sort();
        }
        private static void AddFileParams(List<Param> paramList)
        {
            // Add file-based:
            string paramPath = $"{ExeDir}/Resources/Paramdex_DS2S_09272022";
            string paramFilesPath = $"{ExeDir}/Resources/ParamFiles";
            var pfiles = Directory.GetFiles(paramFilesPath);
            string defName;

            foreach (var pfile in pfiles)
            {
                string name = Path.GetFileNameWithoutExtension(pfile);
                if (pfile.Contains("regist"))
                    defName = "GENERATOR_REGIST_PARAM";
                else
                    defName = "GENERATOR_PARAM";

                string defPath = $"{paramPath}/Defs/{defName}.xml";
                PARAMDEF paramDef = XmlDeserialize(defPath);
                Param param = new(pfile, paramDef, name);
                param.StringsFileName = $"{name}";
                RowOverloadHandler(param);
                paramList.Add(param);
            }
        }
        private static void BuildParamDictionary()
        {
            foreach(var param in RawParamsList)
                AllParams.Add(param.Name, param);

            // add the generators to an easier dictionary
            for (int i = 0; i < PAliases.GENERATOR_PARAM.Count; i++)
                GeneratorParams.Add(AllParams[PAliases.GENERATOR_PARAM[i]]);

            for (int i = 0; i < PAliases.GENERATOR_REGIST.Count; i++)
                GeneratorRegistParams.Add(AllParams[PAliases.GENERATOR_REGIST[i]]);

        }
        private static int hex2int(string hexbyte)
        {
            return int.Parse(hexbyte, System.Globalization.NumberStyles.HexNumber);
        }
        private static void RowOverloadHandler(Param param)
        {
            // couldn't find a way to do this dynamically :(
            switch (param.Type)
            {
                // Just save the ones we care about
                case "SHOP_LINEUP_PARAM":
                    param.initialise<ShopRow>();
                    break;

                case "ITEM_LOT_PARAM2":
                    if (param.Name == "ITEM_LOT_OTHER")
                        param.initialise<ItemWLotRow>();
                    else
                        param.initialise<ItemDropRow>();
                    break;

                case "WEAPON_PARAM":
                    param.initialise<WeaponRow>();
                    break;

                case "WEAPON_REINFORCE_PARAM":
                    param.initialise<WeaponReinforceRow>();
                    break;

                case "WEAPON_TYPE_PARAM":
                    param.initialise<WeaponTypeRow>();
                    break;

                case "ITEM_PARAM":
                    param.initialise<ItemRow>();
                    break;

                case "CUSTOM_ATTR_SPEC_PARAM":
                    param.initialise<CustomAttrSpecRow>();
                    break;

                case "WEAPON_STATS_AFFECT_PARAM":
                    param.initialise<WeaponStatsAffectRow>();
                    break;

                case "PLAYER_STATUS_PARAM":
                    param.initialise<PlayerStatusClassRow>();
                    break;

                case "PLAYER_STATUS_ITEM_PARAM":
                    param.initialise<PlayerStatusItemRow>();
                    break;

                case "ITEM_USAGE_PARAM":
                    param.initialise<ItemUsageRow>();
                    break;

                case "ARROW_PARAM":
                    param.initialise<ArrowRow>();
                    break;

                case "CHR_PARAM":
                    param.initialise<ChrRow>();
                    break;

                case "GENERATOR_PARAM":
                    param.initialise<GeneratorParamRow>();
                    break;

                case "GENERATOR_REGIST_PARAM":
                    param.initialise<GeneratorRegistRow>();
                    break;

                default:
                    // Generic no extension:
                    param.initialise<Param.Row>();
                    break;
            }
        }
        private static PHPointer GetParamPointer(int[] offsets)
        {
            return Hook.CreateChildPointer(Hook.BaseA, offsets);
        }
        
        // Core Functionality:
        public static T? GetLink<T>(Param? linkParam, int linkID)
        {
            if (linkParam == null)
                return default;

            var lookup = linkParam.Rows
                            .Where(row => row.ID == linkID).OfType<T>();
            if (lookup.Count() == 0)
                return default;
            return lookup.First();
        }
        public static T? GetGenRegistLink<T>(int linkID, string startname)
        {
            // Get "equivalent" GenRegist Param
            var id = PAliases.GENERATOR_PARAM.IndexOf(startname);
            var linkParam = GeneratorRegistParams[id];

            var lookup = linkParam.Rows
                            .Where(row => row.ID == linkID).OfType<T>();
            if (lookup.Count() == 0)
                return default;
            return lookup.First();
        }
        public static ItemRow? GetItemFromID(int id)
        {
            // Just a useful QOL to avoid replicating code everywhere
            if (ItemParam == null)
                throw new Exception("Weapon param table not initialised");

            return ItemParam.Rows.FirstOrDefault(row => row.ID == id) as ItemRow;
        }
        public static WeaponRow? GetWeaponFromID(int? id)
        {
            if (id == null) return null;
            
            // Just a useful QOL to avoid replicating code everywhere
            if (WeaponParam == null)
                throw new Exception("Weapon param table not initialised");

            return WeaponParam.Rows.FirstOrDefault(row => row.ID == id) as WeaponRow;
        }

    }
}
