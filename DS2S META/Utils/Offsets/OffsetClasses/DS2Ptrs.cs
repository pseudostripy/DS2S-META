using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.CodeLocators;
using DS2S_META.Utils.Offsets.HookGroupObjects;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public class DS2Ptrs
    {
        private REDataUnpacker _reDataUnpacker;
        public REDataUnpacker REDU => _reDataUnpacker;
        
        // Main HGO
        public CovenantHGO CovenantHGO;
        public ScalingBonusHGO ScalingBonusHGO;
        public FuncListPHP Func;
        public MiscPtrs MiscPtrs;
        public CorePHP Core;
        public CoreGameState CGS;
        public BonfiresHGO BonfiresHGO;
        public PlayerStateHGO PlayerState;
        public PlayerDataHGO PlayerData;
        public CameraHGO CameraHGO;

        // Use factory below to make object
        public DS2Ptrs(DS2SHook hook, DS2VER ver) 
        {
            // Unpack
            _reDataUnpacker = new REDataUnpacker(hook, ver);

            // Assign to properties:
            Core = new(hook, REDU.PHPDict);
            CovenantHGO = new(hook, REDU.LeafGroups["CovenantsGroup"],REDU.Leaves);
            ScalingBonusHGO = new(hook, REDU.LeafGroups["BonusScalingTableGroup"]);
            Func = new(hook, REDU.PHPDict);
            MiscPtrs = new(hook, REDU.PHPDict, REDU.Leaves);
            CGS = new(hook, REDU.Leaves);
            BonfiresHGO = new(hook, REDU.LeafGroups["BonfireLevelsGroup"], REDU.LeafGroups["LastBonfireGroup"]);
            PlayerState = new(hook, REDU.LeafGroups["PlayerGroup"], REDU.LeafGroups["WarpGroup"]);
            PlayerData = new(hook, REDU.LeafGroups["PlayerEquipmentGroup"],REDU.LeafGroups["AttributeGroup"],
                                    REDU.LeafGroups["PlayerParamGroup"],REDU.Leaves);
            CameraHGO = new(hook, REDU.LeafGroups["CameraGroup"]);
        }

        public void UpdateProperties()
        {
            CovenantHGO.UpdateProperties();
            ScalingBonusHGO.UpdateProperties();
            CGS.UpdateProperties();
            BonfiresHGO.UpdateProperties();
            PlayerState.UpdateProperties();
            PlayerData.UpdateProperties();
            CameraHGO.UpdateProperties();
        }

        // Reflection Utility:
        private static void SetObjectFieldByName(object obj, string fieldName, object? value)
        {
            var finfo = typeof(DS2Ptrs).GetField(fieldName) ??
                throw new Exception($"Field {fieldName} cannot be found in class DS2Ptrs");
            finfo.SetValue(obj, value);
        }



    }
}
