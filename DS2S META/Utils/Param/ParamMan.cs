using PropertyHook;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.DS2SOffsets;
using static SoulsFormats.PARAMDEF;
using SoulsFormats;
using DS2S_META.Randomizer;

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
        public static DS2SHook Hook;
        public static readonly string ExeDir = Environment.CurrentDirectory;
        public static List<Param> RawParamsList = new();
        public static Dictionary<PNAME, Param> AllParams = new();
        public static Dictionary<PNAME, string> ParamStringNames = new();
        public static Dictionary<string, PNAME> ParamsFromStrings = new(); // inverse of above
        
        // Params:
        public static Param? ShopLineupParam => AllParams[PNAME.SHOP_LINEUP_PARAM];
        public static Param? WeaponParam => AllParams[PNAME.WEAPON_PARAM];
        public static Param? ItemParam => AllParams[PNAME.ITEM_PARAM];
        public static Param? WeaponReinforceParam => AllParams[PNAME.WEAPON_REINFORCE_PARAM];
        public static Param? WeaponTypeParam => AllParams[PNAME.WEAPON_TYPE_PARAM];
        public static Param? CustomAttrSpecParam => AllParams[PNAME.CUSTOM_ATTR_SPEC_PARAM];
        public static Param? PlayerStatusClassParam => AllParams[PNAME.PLAYER_STATUS_CLASS_PARAM];
        public static Param? ItemUsageParam => AllParams[PNAME.ITEM_USAGE_PARAM];
        public static Param? ArrowParam => AllParams[PNAME.ARROW_PARAM];

        public static void Initialise(DS2SHook hook)
        {
            Hook = hook; // needed?
            SetupParamStringNames();
            GetRawParams();
            BuildParamDictionary();
            IsLoaded = true;
        }

        // Core setup:
        private static void SetupParamStringNames()
        {
            var d = new Dictionary<PNAME, string>();

            d.Add(PNAME.SHOP_LINEUP_PARAM, "SHOP_LINEUP_PARAM");
            d.Add(PNAME.WEAPON_PARAM, "WEAPON_PARAM");
            d.Add(PNAME.WEAPON_REINFORCE_PARAM, "WEAPON_REINFORCE_PARAM");
            d.Add(PNAME.ITEM_PARAM, "ITEM_PARAM");
            d.Add(PNAME.ITEM_LOT_PARAM2, "ITEM_LOT_PARAM2");
            d.Add(PNAME.WEAPON_TYPE_PARAM, "WEAPON_TYPE_PARAM");
            d.Add(PNAME.CUSTOM_ATTR_SPEC_PARAM, "CUSTOM_ATTR_SPEC_PARAM");
            d.Add(PNAME.WEAPON_STATS_AFFECT_PARAM, "WEAPON_STATS_AFFECT_PARAM");
            d.Add(PNAME.PLAYER_STATUS_CLASS_PARAM, "PLAYER_STATUS_PARAM");
            d.Add(PNAME.ITEM_USAGE_PARAM, "ITEM_USAGE_PARAM");
            d.Add(PNAME.ARROW_PARAM, "ARROW_PARAM");

            ParamStringNames = d;
            ParamsFromStrings = d.ToDictionary(kvp => kvp.Value, kvp => kvp.Key); // reverse them
        }
        private static void GetRawParams()
        {
            List<Param> paramList = new List<Param>();
            string paramPath = $"{ExeDir}/Resources/Paramdex_DS2S_09272022/";

            string pointerPath = $"{paramPath}/Pointers/";
            string[] paramPointers = Directory.GetFiles(pointerPath, "*.txt");
            foreach (string path in paramPointers)
            {
                string[] pointers = File.ReadAllLines(path);
                AddParam(paramList, paramPath, path, pointers);
            }
            RawParamsList = paramList;
        }
        private static void AddParam(List<Param> paramList, string paramPath, string path, string[] pointers)
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
                Param param = new Param(pointer, offsets, paramDef, name);

                RowOverloadHandler(param);
                
                // Save param
                paramList.Add(param);
            }
            paramList.Sort();
        }
        private static void BuildParamDictionary()
        {
            foreach(var param in RawParamsList)
                AllParams.Add(ParamsFromStrings[param.Name], param);
        }
        private static int hex2int(string hexbyte)
        {
            return int.Parse(hexbyte, System.Globalization.NumberStyles.HexNumber);
        }
        private static void RowOverloadHandler(Param param)
        {
            // couldn't find a way to do this dynamically :(
            PNAME pname = ParamsFromStrings[param.Name];
            switch (pname)
            {
                // Just save the ones we care about
                case PNAME.SHOP_LINEUP_PARAM:
                    param.initialise<ShopRow>();
                    break;

                case PNAME.WEAPON_PARAM:
                    param.initialise<WeaponRow>();
                    break;

                case PNAME.WEAPON_REINFORCE_PARAM:
                    param.initialise<WeaponReinforceRow>();
                    break;

                case PNAME.WEAPON_TYPE_PARAM:
                    param.initialise<WeaponTypeRow>();
                    break;

                case PNAME.ITEM_PARAM:
                    param.initialise<ItemRow>();
                    break;

                case PNAME.CUSTOM_ATTR_SPEC_PARAM:
                    param.initialise<CustomAttrSpecRow>();
                    break;

                case PNAME.WEAPON_STATS_AFFECT_PARAM:
                    param.initialise<WeaponStatsAffectRow>();
                    break;

                case PNAME.PLAYER_STATUS_CLASS_PARAM:
                    param.initialise<PlayerStatusClassRow>();
                    break;

                case PNAME.ITEM_USAGE_PARAM:
                    param.initialise<ItemUsageRow>();
                    break;

                case PNAME.ARROW_PARAM:
                    param.initialise<ArrowRow>();
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
        public static T? GetLink<T>(PNAME pname, int linkID)
        {
            var lookup = AllParams[pname].Rows
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

        // This is an extra indirection to avoid typo bugs
        public enum PNAME
        {
            SHOP_LINEUP_PARAM,
            WEAPON_PARAM,
            WEAPON_REINFORCE_PARAM,
            ITEM_PARAM,
            ITEM_LOT_PARAM2,
            WEAPON_TYPE_PARAM,
            CUSTOM_ATTR_SPEC_PARAM,
            WEAPON_STATS_AFFECT_PARAM,
            PLAYER_STATUS_CLASS_PARAM,
            ITEM_USAGE_PARAM,
            ARROW_PARAM,
        }


    }
}
