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

namespace DS2S_META.Utils
{
    public static class ParamMan
    {
        // Top-level Fields
        public static DS2SHook Hook;
        public static readonly string ExeDir = Environment.CurrentDirectory;
        public static List<Param> RawParamsList = new();
        public static Dictionary<string, Param> AllParams = new();

        // Params:
        public static Param? WeaponParam => AllParams["WEAPON_PARAM"];
        public static Param? ItemParam => AllParams["ITEM_PARAM"];
        public static Param? WeaponReinforceParam => AllParams["WEAPON_REINFORCE_PARAM"];
        
        public static void Initialise(DS2SHook hook)
        {
            Hook = hook; // needed?
            GetRawParams();
            BuildParamDictionary();
        }

        // Core setup:
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
                param.initialise<Param.Row>();

                // Save param
                StoreLocalParam(param);
                paramList.Add(param);
            }
            paramList.Sort();
        }
        private static void BuildParamDictionary()
        {
            foreach(var param in RawParamsList)
                AllParams.Add(param.Name, param);
        }
        private static int hex2int(string hexbyte)
        {
            return int.Parse(hexbyte, System.Globalization.NumberStyles.HexNumber);
        }

        private static void StoreLocalParam(Param param)
        {
            //switch (param.Name)
            //{
            //    // Just save the ones we care about
            //    case "SHOP_LINEUP_PARAM":
            //        ShopLineupParam = param;
            //        break;

            //    case "ITEM_LOT_PARAM2":
            //        ItemLotOtherParam = param;
            //        break;

            //    case "ITEM_PARAM":
            //        ItemParam = param;
            //        break;

            //    case "WEAPON_PARAM":
            //        WeaponParam = param;
            //        break;

            //    case "WEAPON_REINFORCE_PARAM":
            //        WeaponReinforceParam = param;
            //        break;

            //    default:
            //        break;
            //}
        }

        private static PHPointer GetParamPointer(int[] offsets)
        {
            return Hook.CreateChildPointer(Hook.BaseA, offsets);
        }

    }
}
