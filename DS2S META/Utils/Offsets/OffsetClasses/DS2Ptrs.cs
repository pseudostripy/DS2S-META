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
        // Fields
        public PHPointer? BaseA;
        public PHPointer? GiveSoulsFunc;
        public PHPointer? RemoveSoulsFunc;
        public PHPointer? ItemGiveFunc;
        public PHPointer? ItemStruct2dDisplay;
        public PHPointer? phpDisplayItem;
        public PHPointer? SetWarpTargetFunc;
        public PHPointer? WarpManager;
        public PHPointer? WarpFunc;
        public PHPointer? SomePlayerStats;
        public PHPointer? PlayerName;
        public PHPointer? AvailableItemBag;
        public PHPointer? ItemGiveWindow;
        public PHPointer? PlayerBaseMisc;
        public PHPointer? PlayerCtrl;
        public PHPointer? PlayerPosition;
        public PHPointer? PlayerGravity;
        public PHPointer? PlayerParam;
        public PHPointer? PlayerType;
        public PHPointer? SpEffectCtrl;
        public PHPointer? ApplySpEffect;
        public PHPointer? PlayerMapData;
        public PHPointer? EventManager;
        public PHPointer? BonfireLevels;
        public PHPointer? NetSvrBloodstainManager;
        public PHPointer? BaseB;
        public PHPointer? Connection;
        public PHPointer? Camera;
        public PHPointer? Camera2;
        public PHPointer? SpeedFactorAccel;
        public PHPointer? SpeedFactorAnim;
        public PHPointer? SpeedFactorJump;
        public PHPointer? SpeedFactorBuildup;
        public PHPointer? LoadingState;
        public PHPointer? phDisableAI; // pointer head (missing final offset)
        public PHPointer? phBIKP1SkipVals; // pointer head (missing final offset)

        private REDataUnpacker _reDataUnpacker;
        public REDataUnpacker REDU => _reDataUnpacker;
        
        // Main HGO
        public CovenantHGO CovenantHGO;
        public ScalingBonusHGO ScalingBonusHGO;
        public FuncListPHP Func;
        public MiscPtrs MiscPtrs;
        public CorePHP Core;

        // Use factory below to make object
        public DS2Ptrs(DS2SHook hook, DS2VER ver) 
        {
            // Unpack
            _reDataUnpacker = new REDataUnpacker(hook, ver);

            // Assign to properties:
            CovenantHGO = new(hook, REDU.LeafGroups["CovenantsGroup"]);
            ScalingBonusHGO = new(hook, REDU.LeafGroups["BonusScalingTableGroup"]);
            Func = new(hook, REDU.PHPDict);
            MiscPtrs = new(hook, REDU.PHPDict);
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
