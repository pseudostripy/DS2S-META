using DS2S_META.Utils.Offsets.OffsetClasses;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
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

        // Use factory below to make object
        private DS2Ptrs() { }

        public static void SetupPointers(DS2SHook hook, DS2VER ver)
        {
            // Factory:
            var DS2P = new DS2Ptrs();

            // Handle AoB stuff first as has no parents.
            // Note all AoB scans are PointerDefns
            Dictionary<string, CodeLocator> aobPtrDict =
                DS2REData.PointerDefns.Where(x => x.HasVerLocator(ver))
                                       .ToDictionary(x => x.Identifier, x => x.GetVerLocator(ver));

            // AoB first // probs need an AoBAbsFollowOffsetsPHPointer then can do this whole section together
            foreach (var kvp in aobPtrDict.Where(kvp => kvp.Value is AbsoluteAOBCL))
                SetObjectFieldByName(DS2P, kvp.Key, kvp.Value?.Init(hook));
            hook.RescanAOB(); // resolve AbsAob things in one go

            // Now do relative or more complex AoB resolutions:


            // Now we have to be more careful because there could be hierarchy resolution issues...



            // Collect things of the right version only (rest stays null)
            BaseBSetup = RegisterAbsoluteAOB(Offsets.Func.BaseBAoB);
            SpeedFactorAccel = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAccelOffset);
            SpeedFactorAnim = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAnimOffset);
            SpeedFactorJump = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorJumpOffset);
            SpeedFactorBuildup = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorBuildupOffset);
            GiveSoulsFunc = RegisterAbsoluteAOB(Offsets.Func.GiveSoulsFuncAoB);
            RemoveSoulsFunc = RegisterAbsoluteAOB(Offsets.Func.RemoveSoulsFuncAoB);
            ItemGiveFunc = RegisterAbsoluteAOB(Offsets.Func.ItemGiveFunc);
            ItemStruct2dDisplay = RegisterAbsoluteAOB(Offsets.Func.ItemStruct2dDisplay);
            SetWarpTargetFunc = RegisterAbsoluteAOB(Offsets.Func.SetWarpTargetFuncAoB);
            WarpFunc = RegisterAbsoluteAOB(Offsets.Func.WarpFuncAoB);

            // Version Specific AOBs:
            ApplySpEffect = RegisterAbsoluteAOB(Offsets.Func.ApplySpEffectAoB);
            phpDisplayItem = RegisterAbsoluteAOB(Offsets.Func.DisplayItem); // CAREFUL WITH THIS!
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
