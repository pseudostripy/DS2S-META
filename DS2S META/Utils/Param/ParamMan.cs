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
        
        public static void Initialise(DS2SHook hook)
        {
            Hook = hook; // needed?
            SetupParamStringNames();
            GetRawParams();
            BuildParamDictionary();
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
        public static T GetLink<T>(PNAME pname, int linkID)
        {
            var lookup =  AllParams[pname].Rows
                            .Where(row => row.ID == linkID).OfType<T>();
            if (lookup.Count() == 0)
                return default;
            return lookup.First();
        }

        // This is an extra indirection to avoid typo bugs
        public enum PNAME
        {
            SHOP_LINEUP_PARAM,
            WEAPON_PARAM,
            WEAPON_REINFORCE_PARAM,
            ITEM_PARAM,
            ITEM_LOT_PARAM2,
        }


    }
}
