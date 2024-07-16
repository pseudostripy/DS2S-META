using DS2S_META.Utils.Offsets.OffsetClasses;
using PropertyHook;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook
{
    /// <summary>
    /// Place to store any Hook-related things that interact with assembly
    /// in order to tidy up DS2Hook a bit
    /// </summary>
    internal class AssemblyScripts
    {
        private readonly DS2SHook Hook;
        private readonly DS2Ptrs DS2P;
        public AssemblyScripts(DS2SHook hook, DS2Ptrs ds2p)
        {
            Hook = hook;
            DS2P = ds2p;
        }

        // Actual implementations:
        internal void GiveItems64(int[] itemids, short[] amounts, byte[] upgrades, byte[] infusions, bool showdialog = true)
        {
            // Last resort elegant crash checks (this func needs all the following hooks)
            if (DS2P.Func.ItemGive == null)
                throw new MetaFeatureException("ItemGive64.ItemGive");
            if (DS2P.MiscPtrs.AvailableItemBag == null)
                throw new MetaFeatureException("ItemGive64.AvailableItemBag");
            if (DS2P.Func.ItemStruct2dDisplay == null)
                throw new MetaFeatureException("ItemGive64.ItemStruct2dDisplay");
            if (DS2P.Func.ItemGiveWindow == null)
                throw new MetaFeatureException("ItemGive64.ItemGiveWindow");
            if (DS2P.Func.DisplayItemWindow == null)
                throw new MetaFeatureException("ItemGive64.DisplayItemWindow");


            // Improved assembly to handle multigive and showdialog more neatly.
            int numitems = itemids.Length;
            if (numitems > 8)
                throw new Exception("Item Give function in DS2 can only handle 8 items at a time");

            // Create item structure to pass to DS2
            var itemStruct = Hook.Allocate(0x8A); // should this be d128?
            for (int i = 0; i < itemids.Length; i++)
            {
                var offi = i * 16; // length of one itemStruct
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0x4, BitConverter.GetBytes(itemids[i]));
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0x8, BitConverter.GetBytes(float.MaxValue));
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0xC, BitConverter.GetBytes(amounts[i]));
                Kernel32.WriteByte(Hook.Handle, itemStruct + offi + 0xE, upgrades[i]);
                Kernel32.WriteByte(Hook.Handle, itemStruct + offi + 0xF, infusions[i]);
            }

            // Prepare values for assembly
            var numItems_bytes = BitConverter.GetBytes(numitems);
            var itemStructAddr_bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            var availBag_bytes = BitConverter.GetBytes(DS2P.MiscPtrs.AvailableItemBag.Resolve().ToInt64());
            var itemGive_bytes = BitConverter.GetBytes(DS2P.Func.ItemGive.Resolve().ToInt64());
            var itemStruct2Display_bytes = BitConverter.GetBytes(DS2P.Func.ItemStruct2dDisplay.Resolve().ToInt64());
            var itemGiveWindow_bytes = BitConverter.GetBytes(DS2P.Func.ItemGiveWindow.Resolve().ToInt64());
            var displayItem64_bytes = BitConverter.GetBytes(DS2P.Func.DisplayItemWindow.Resolve().ToInt64());
            var showWindow_bytes = BitConverter.GetBytes(Convert.ToInt32(showdialog));

            // Get reference assembly and populate links
            var asm = (byte[])DS2SAssembly.GiveItem64.Clone();
            Array.Copy(numItems_bytes, 0, asm, 0x9, numItems_bytes.Length);
            Array.Copy(itemStructAddr_bytes, 0, asm, 0xF, itemStructAddr_bytes.Length);
            Array.Copy(availBag_bytes, 0, asm, 0x1C, availBag_bytes.Length);
            Array.Copy(itemGive_bytes, 0, asm, 0x29, itemGive_bytes.Length);
            Array.Copy(showWindow_bytes, 0, asm, 0x37, showWindow_bytes.Length);
            Array.Copy(numItems_bytes, 0, asm, 0x42, numItems_bytes.Length);
            Array.Copy(itemStructAddr_bytes, 0, asm, 0x48, itemStructAddr_bytes.Length);
            Array.Copy(itemStruct2Display_bytes, 0, asm, 0x60, itemStruct2Display_bytes.Length);
            Array.Copy(itemGiveWindow_bytes, 0, asm, 0x72, itemGiveWindow_bytes.Length);
            Array.Copy(displayItem64_bytes, 0, asm, 0x7C, displayItem64_bytes.Length);

            Hook.Execute(asm);
            Hook.Free(itemStruct);
        }
        internal void GiveItems32(int[] itemids, short[] amounts, byte[] upgrades, byte[] infusions, bool showdialog = true)
        {
            // Last resort elegant crash checks (this func needs all the following hooks)
            if (DS2P.Func.ItemGive == null)
                throw new MetaFeatureException("ItemGive32.ItemGive");
            if (DS2P.MiscPtrs.AvailableItemBag == null)
                throw new MetaFeatureException("ItemGive32.AvailableItemBag");
            if (DS2P.Func.ItemStruct2dDisplay == null)
                throw new MetaFeatureException("ItemGive32.ItemStruct2dDisplay");
            if (DS2P.Func.ItemGiveWindow == null)
                throw new MetaFeatureException("ItemGive32.ItemGiveWindow");
            if (DS2P.Func.DisplayItemWindow == null)
                throw new MetaFeatureException("ItemGive32.DisplayItemWindow");

            // Improved assembly to handle multigive and showdialog more neatly.
            int numitems = itemids.Length;
            if (numitems > 8)
                throw new Exception("Item Give function in DS2 can only handle 8 items at a time");

            // Create item structure to pass to DS2
            var itemStruct = Hook.Allocate(0x8A); // should this be d128?
            for (int i = 0; i < itemids.Length; i++)
            {
                var offi = i * 16; // length of one itemStruct
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0x4, BitConverter.GetBytes(itemids[i]));
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0x8, BitConverter.GetBytes(float.MaxValue));
                Kernel32.WriteBytes(Hook.Handle, itemStruct + offi + 0xC, BitConverter.GetBytes(amounts[i]));
                Kernel32.WriteByte(Hook.Handle, itemStruct + offi + 0xE, upgrades[i]);
                Kernel32.WriteByte(Hook.Handle, itemStruct + offi + 0xF, infusions[i]);
            }

            // Store const float for later reference            
            var pfloat_store = Hook.Allocate(sizeof(float));
            Kernel32.WriteBytes(Hook.Handle, pfloat_store, BitConverter.GetBytes(6.0f));

            // Setup assembly fields for substitution
            var numItems_bytes = BitConverter.GetBytes(numitems);
            var pItemStruct_bytes = BitConverter.GetBytes(itemStruct.ToInt32());
            var availItemBag_bytes = BitConverter.GetBytes(DS2P.MiscPtrs.AvailableItemBag.Resolve().ToInt32());
            var itemGiveFunc_bytes = BitConverter.GetBytes(DS2P.Func.ItemGive.Resolve().ToInt32());
            var itemStruct2Display_bytes = BitConverter.GetBytes(DS2P.Func.ItemStruct2dDisplay.Resolve().ToInt32());
            var itemGiveWindow_bytes = BitConverter.GetBytes(DS2P.Func.ItemGiveWindow.Resolve().ToInt32());
            var pfloat_bytes = BitConverter.GetBytes(pfloat_store.ToInt32());
            var displayItem32_bytes = BitConverter.GetBytes(DS2P.Func.DisplayItemWindow.Resolve().ToInt32());
            var showWindow_bytes = BitConverter.GetBytes(Convert.ToInt32(showdialog));

            // Load and fix assembly:
            var asm = (byte[])DS2SAssembly.GiveItem32.Clone();
            Array.Copy(numItems_bytes, 0, asm, 0x7, numItems_bytes.Length);
            Array.Copy(pItemStruct_bytes, 0, asm, 0xC, pItemStruct_bytes.Length);
            Array.Copy(availItemBag_bytes, 0, asm, 0x11, availItemBag_bytes.Length);
            Array.Copy(itemGiveFunc_bytes, 0, asm, 0x1a, itemGiveFunc_bytes.Length);
            Array.Copy(showWindow_bytes, 0, asm, 0x21, showWindow_bytes.Length);
            Array.Copy(numItems_bytes, 0, asm, 0x2a, numItems_bytes.Length);
            Array.Copy(pItemStruct_bytes, 0, asm, 0x32, pItemStruct_bytes.Length);
            Array.Copy(pfloat_bytes, 0, asm, 0x38, pfloat_bytes.Length);
            Array.Copy(itemStruct2Display_bytes, 0, asm, 0x3e, itemStruct2Display_bytes.Length);
            Array.Copy(pfloat_bytes, 0, asm, 0x48, pfloat_bytes.Length);
            Array.Copy(itemGiveWindow_bytes, 0, asm, 0x4e, itemGiveWindow_bytes.Length);
            Array.Copy(displayItem32_bytes, 0, asm, 0x53, displayItem32_bytes.Length);

            Hook.Execute(asm);
            Hook.Free(itemStruct);
        }

        internal bool Warp64(ushort id, bool areadefault = false)
        {
            // Last resort elegant crash checks (this func needs all the following hooks)
            if (DS2P.Func.SetWarpTargetFunc == null)
                throw new MetaFeatureException("Warp64.SetWarpTargetFunc");
            if (DS2P.MiscPtrs.WarpManager == null)
                throw new MetaFeatureException("Warp64.WarpManager");
            if (DS2P.Func.WarpFunc == null)
                throw new MetaFeatureException("Warp64.WarpFunc");

            // area default means warp to the 0,0 part of the map (like a wrong warp)
            // areadefault = false is a normal "warp to bonfire"
            int WARPAREADEFAULT = 2;
            int WARPBONFIRE = 3;

            var value = Hook.Allocate(sizeof(short));
            Kernel32.WriteBytes(Hook.Handle, value, BitConverter.GetBytes(id));

            var asm = (byte[])DS2SAssembly.BonfireWarp64.Clone();
            var bytes = BitConverter.GetBytes(value.ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x9, bytes.Length);
            bytes = BitConverter.GetBytes(DS2P.Func.SetWarpTargetFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x21, bytes.Length);
            bytes = BitConverter.GetBytes(DS2P.MiscPtrs.WarpManager.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x2E, bytes.Length);
            bytes = BitConverter.GetBytes(DS2P.Func.WarpFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x3B, bytes.Length);

            int flag = areadefault ? WARPAREADEFAULT : WARPBONFIRE;
            bytes = BitConverter.GetBytes(flag);
            Array.Copy(bytes, 0x0, asm, 0x45, bytes.Length);

            var warped = false;
            if (!Hook.Multiplayer)
            {
                Hook.Execute(asm);
                warped = true;
            }

            Hook.Free(value);
            return warped;
        }
        internal bool Warp32(ushort bfid, bool areadefault = false)
        {
            // Last resort elegant crash checks (this func needs all the following hooks)
            if (DS2P.Func.SetWarpTargetFunc == null)
                throw new MetaFeatureException("Warp64.SetWarpTargetFunc");
            if (DS2P.Core.BaseA == null)
                throw new MetaFeatureException("Warp64.BaseA");
            if (DS2P.Func.WarpFunc == null)
                throw new MetaFeatureException("Warp64.WarpFunc");

            // area default means warp to the 0,0 part of the map (like a wrong warp)
            // areadefault = false is a normal "warp to bonfire"
            int WARPAREADEFAULT = 2;
            int WARPBONFIRE = 3;
            int flag = areadefault ? WARPAREADEFAULT : WARPBONFIRE;

            // Get assembly template
            var asm = (byte[])DS2SAssembly.BonfireWarp32.Clone();

            // Get variables for byte changes
            var bfiD_bytes = BitConverter.GetBytes(bfid);
            var pWarpTargetFunc = BitConverter.GetBytes(DS2P.Func.SetWarpTargetFunc.Resolve().ToInt32()); // same as warpman?
            var warptypeflag = BitConverter.GetBytes(flag);
            var pBaseA = BitConverter.GetBytes(DS2P.Core.BaseA.Resolve().ToInt32());
            var pWarpFun = BitConverter.GetBytes(DS2P.Func.WarpFunc.Resolve().ToInt32());

            // Change bytes
            Array.Copy(bfiD_bytes, 0x0, asm, 0xB, bfiD_bytes.Length);
            Array.Copy(pWarpTargetFunc, 0x0, asm, 0x14, pWarpTargetFunc.Length);
            Array.Copy(warptypeflag, 0x0, asm, 0x1F, warptypeflag.Length);
            Array.Copy(pBaseA, 0x0, asm, 0x24, pBaseA.Length);
            Array.Copy(pWarpFun, 0x0, asm, 0x36, pWarpFun.Length);

            // Safety checks
            var warped = false;
            if (Hook.Multiplayer)
                return warped; // No warping in multiplayer!

            // Execute:
            Hook.Execute(asm);
            warped = true;
            return warped;
        }
    }
}
