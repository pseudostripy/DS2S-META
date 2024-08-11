using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils.Offsets.OffsetClasses;
using DS2S_META.Dialog;
using DS2S_META.Utils.DS2Hook.MemoryMods;
using static DS2S_META.Utils.DS2Hook.CommonGameRequests;


namespace DS2S_META.Utils.DS2Hook
{
    public class DS2SHook : PHook, INotifyPropertyChanged
    {
        public MainWindow MW { get; set; }

        // Event Handling:
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }
        public event EventHandler<GameStateEventArgs> OnGameStateHandler;
        public void RaiseGameStateChange(int oldstate, int newstate)
        {
            OnGameStateHandler?.Invoke(this, new GameStateEventArgs(this, oldstate, newstate));
        }
        
        // wrapper properties
        public DS2VER DS2Ver => VerMan.DS2Ver;
        public bool InGame => DS2P?.CGS.InGame ?? false;
        public bool Focused => Hooked && User32.GetForegroundProcessID() == Process.Id;
        
        // Main property managers
        public DS2Ptrs DS2P;
        internal AssemblyScripts ASM;
        internal SpeedhackManager? SpeedhackMan;
        public DS2Versions VerMan;
        internal SetupCleanupMan SetupCleanupMan;
        internal CommonGameRequests CGR;

        // -----------------------------------------   Constructor   ---------------------------------------
        public DS2SHook(int refreshInterval, int minLifetime) :
            base(refreshInterval, minLifetime, p => p.MainWindowTitle == "DARK SOULS II")
        {
            OnHooked += DS2Hook_OnHooked;
            OnUnhooked += DS2Hook_OnUnhooked;
            VerMan = new(this);
            SetupCleanupMan = new(this);
            CGR = new(this);
        }

        
        public void SetupPointers2()
        {
            DS2P = new(this, DS2Ver);
            ASM = new AssemblyScripts(this, DS2P);
            SpeedhackMan = new SpeedhackManager(this);
        }

        // Exposed interfaces
        public void EnableSpeedhack(bool value) => SpeedhackMan?.Speedhack(value);
        public void SetSpeedhackSpeed(double value) => SpeedhackMan?.SetSpeed(value);

        // Hook setup / cleanup
        private void DS2Hook_OnHooked(object? sender, PHEventArgs e)
        {
            SetupCleanupMan.Setup_OnHooked();
            OnPropertyChanged(nameof(Hooked));
        }
        private void DS2Hook_OnUnhooked(object? sender, PHEventArgs e)
        {
            SetupCleanupMan.Cleanup();
            OnPropertyChanged(nameof(Hooked));
        }
        public void UpdateMainProperties()
        {
            DS2P?.UpdateProperties();
        }

        // Common request wrappers:
        public void UnlockBonfires() => CGR.UnlockBonfires();
        public void WarpLast() => CGR.WarpLast();
        public void AddSouls(int numsouls) => CGR.AddSouls(numsouls);
        public void RestoreHumanity() => CGR.RestoreHumanity();
        public void Give17kReward() => CGR.Give17kReward();
        public void Give3Chunk1Slab() => CGR.Give3Chunk1Slab();
        public void NewTestCharacter() => CGR.NewTestCharacter();
        public void SetRapierOHKO(bool ohko) => CGR.SetWeaponOHKO(ITEMID.RAPIER, ohko);
        public void SetDisableAI(bool disableAI) => CGR.SetDisableAI(disableAI);
        public void SetNoGravity(bool noGravity) => CGR.SetNoGravity(noGravity);
        public void SetNoCollision(bool noCollision) => CGR.SetNoCollision(noCollision);
        public void SetFistOHKO(bool ohko) => CGR.SetWeaponOHKO(ITEMID.FISTS, ohko);
        public void SetInfiniteStamina(bool infstam) => CGR.SetInfiniteStamina(infstam);
        public void SetNoDeath(bool nodeath) => CGR.SetNoDeath(nodeath);
        public bool WarpBonfire(DS2SBonfire bf, bool ww, bool brest) => CGR.WarpBonfire(bf, ww, brest);
        public void GiveItem(ITEMID id, short amt = 1, byte upgr = 0, byte infusion = 0, GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
            => CGR.GiveItem(id, amt, upgr, infusion, opt);


        // TODO ModMan
        public NoDmgMod? NoDmgMod;
        public NopableInject? DisableSkirtDamage;
        public NopableInject? InfiniteSpells;
        public NopableInject? InfiniteGoods;
        public void UninstallBIKP1Skip()
        {
            //if (InstalledInjects.Contains(InstallInject))
            BIKP1Skip(false, false);
        } 
        internal bool BIKP1Skip(bool enable, bool doLoad)
        {
            if (!Hooked) return false;
            //ASM.ApplyBIKP1Skip(enable);
            
            if (doLoad)
                CGR.WarpLast(); // force a reload to fix some memes; only on first click
            return enable; // turned on or off now
        }
        private void EnsureInstalledNoDmgMod()
        {
            if (NoDmgMod?.IsInstalled == true)
                return;
            NoDmgMod ??= new NoDmgMod(this);
            NoDmgMod.Install();
        }
        public void GeneralizedDmgMod(bool dealFullDmg, bool dealNoDmg, bool recvNoDmg)
        {
            // catch awkward logical bugs
            if (dealFullDmg && dealNoDmg) throw new MetaLogicException("Cannot do zero and full damage together. Should be handled in ViewModel.");

            bool affectDealtDmg = dealFullDmg || dealNoDmg;
            bool affectRcvdDmg = recvNoDmg;
            int HIGHDMG = 0x4b18967f; // float 9999999.0
            var dmgfacDealt = dealFullDmg ? HIGHDMG : 0; // irrelevant if affectDealtDmg == false
            int dmgfacRcvd = recvNoDmg ? 0 : 1;

            EnsureInstalledNoDmgMod();
            NoDmgMod?.SetDmgModSettings(affectDealtDmg, affectRcvdDmg, dmgfacDealt, dmgfacRcvd);
        }
        public void UninstallDmgMod() => NoDmgMod?.Uninstall();
        public void UninstallDisableSkirtDamage() => DisableSkirtDamage?.Uninstall();
        public void UninstallInfiniteSpells() => InfiniteSpells?.Uninstall();
        public void UninstallInfiniteGoods() => InfiniteGoods?.Uninstall();
        private void EnsureInstalledDisableSkirtDamage()
        {
            if (DisableSkirtDamage?.IsInstalled == true)
                return;

            // Create and install inject
            var injptr = DS2P?.Func.DisableSkirtDamage?.Resolve()
                ?? throw new MetaMemoryException("DisableSkirtDamage function pointer not initialized correctly"); ;
            var origbytes = Injects.GetDefinedBytes(DS2Ver, Injects.NOPINJECTS.DISABLESKIRT);
            DisableSkirtDamage ??= new NopableInject(this, injptr, origbytes);
            DisableSkirtDamage.Install();
        }
        public void SetDisableSkirtDamage(bool disableSkirtDamage)
        {
            if (MetaFeature.IsInactive(METAFEATURE.DISABLESKIRTDAMAGE)) return;
            if (disableSkirtDamage)
                EnsureInstalledDisableSkirtDamage();
            else
                UninstallDisableSkirtDamage();
        }
        public void ResetPartyWalkTimer()
        {
            // Final sanity checks before dangerous operation:
            var PHGrab = DS2P?.MiscPtrs.DisablePartyWalkTimer;
            if (PHGrab == null || PHGrab.Resolve() == IntPtr.Zero)
                throw new MetaMemoryException("Pointer to GrabStruct (required for DisablePartyWalkTimer) cannot be resolved.");

            // Only apply timer resets to specified (Player?) grab types
            List<int> applicableGrabTypes = [30100, 30300, 30500];
            var grabParamId = PHGrab.ReadInt32(0x8);
            if (!applicableGrabTypes.Contains(grabParamId))
                return; // no relevant timers to reset

            // Reset timer back to 1s every time this function is called
            int PartyWalkTimerOffset = 0xc;
            PHGrab.WriteSingle(PartyWalkTimerOffset, 1);
        }
        private void EnsureInstalledInfiniteSpells()
        {
            if (InfiniteSpells?.IsInstalled == true)
                return;

            var injptr = DS2P?.Func.InfiniteSpells?.Resolve()
                ?? throw new MetaMemoryException("InfiniteSpells function pointer not initialized correctly");
            var origbytes = Injects.GetDefinedBytes(DS2Ver, Injects.NOPINJECTS.INFINITESPELLS);
            InfiniteSpells ??= new NopableInject(this, injptr, origbytes);
            InfiniteSpells.Install();
        }
        public void SetInfiniteSpells(bool infiniteSpells)
        {
            if (MetaFeature.IsInactive(METAFEATURE.INFINITESPELLS)) return;
            if (infiniteSpells)
                EnsureInstalledInfiniteSpells();
            else
                UninstallInfiniteSpells();
        }
        private void EnsureInstalledInfiniteGoods()
        {
            if (InfiniteGoods?.IsInstalled == true)
                return;
            var injptr = DS2P?.Func.InfiniteGoods?.Resolve()
                ?? throw new MetaMemoryException("InfiniteGoods function pointer not initialized correctly");
            var origbytes = Injects.GetDefinedBytes(DS2Ver, Injects.NOPINJECTS.INFINITEGOODS);
            InfiniteGoods ??= new NopableInject(this, injptr, origbytes);
            InfiniteGoods.Install();
        }
        public void SetInfiniteGoods(bool infiniteGoods)
        {
            if (MetaFeature.IsInactive(METAFEATURE.INFINITEGOODS)) return;
            if (infiniteGoods)
                EnsureInstalledInfiniteGoods();
            else
                UninstallInfiniteGoods();
        }

        // TODO queryman?
        public bool CheckLoadedEnemies(CHRID chrid) => CheckLoadedEnemies((int)chrid);
        public bool CheckLoadedEnemies(int queryChrId)
        {
            if (DS2P.MiscPtrs.PHLoadedEnemiesTable == null)
                throw new Exception("Version error, should be handled in front end");

            int nmax = 70; // I think this is the most?
            int psize = Is64Bit ? 8 : 4;
            for (int i = 0; i < nmax; i++)
            {
                var chrclass = CreateChildPointer(DS2P.MiscPtrs.PHLoadedEnemiesTable, i * psize);
                int chrId = chrclass.ReadInt32(0x28); // to check generality
                if (chrId == queryChrId)
                    return true;
            }

            // not found in whole table
            return false;
        }
        internal int GetMaxUpgrade(ItemRow item)
        {
            if (!ParamMan.IsLoaded)
                return 0;

            // Until we figure out more about the internal params:
            if (item.ItemID == (int)ITEMID.BINOCULARS || item.ItemID == (int)ITEMID.KEYTOEMBEDDED)
                return 0;


            int? upgr;
            switch (item.ItemType)
            {
                case eItemType.WEAPON1:
                case eItemType.WEAPON2:
                    upgr = item.WeaponRow?.MaxUpgrade;
                    return upgr ?? 0;

                case eItemType.HEADARMOUR:
                case eItemType.LEGARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                    upgr = item.ArmorRow?.ArmorReinforceRow?.MaxReinforceLevel;
                    return upgr ?? 0;
                default:
                    return 0;
            }
        }
        internal int GetHeld(ItemRow itemrow)
        {
            switch (itemrow.ItemType)
            {
                //case eItemType.AMMO+
                // return GetHeldInInventoryUnstackable(item.ID); // TODO

                case eItemType.CONSUMABLE:
                    return GetHeldInInventoryStackable(itemrow.ID);

                default:
                    return 0;
            }
        }
        private int GetHeldInInventoryStackable(int id)
        {
            var inventorySlot = 0x30;
            var itemOffset = 0x0;
            var boxOffset = 0x4;
            var heldOffset = 0x8;
            var nextOffset = 0x10;

            if (DS2P.MiscPtrs.AvailableItemBag == null) return 0;
            var endPointer = DS2P.MiscPtrs.AvailableItemBag.ReadIntPtr(0x10).ToInt64();
            var bagSize = endPointer - DS2P.MiscPtrs.AvailableItemBag.Resolve().ToInt64();

            var inventory = DS2P.MiscPtrs.AvailableItemBag.ReadBytes(0x0, (uint)bagSize);

            while (inventorySlot < bagSize)
            {
                // Get next item in inventory
                var itemID = BitConverter.ToInt32(inventory, inventorySlot + itemOffset);

                if (itemID == id)
                {
                    var boxValue = BitConverter.ToInt32(inventory, inventorySlot + boxOffset);
                    var held = BitConverter.ToInt32(inventory, inventorySlot + heldOffset);

                    if (boxValue == 0)
                        return held;
                }
                inventorySlot += nextOffset;
            }

            return 0;
        }
        private int GetHeldInInventoryUnstackable(int id)
        {
            throw new NotImplementedException();
        }
        internal bool GetIsDroppable(ItemRow item)
        {
            if (!Hooked || !ParamMan.IsLoaded)
                return false;

            if (ParamMan.ItemUsageParam == null)
                return false;

            if (item == null)
                throw new Exception("Cannot find item for GetIsDroppable");

            bool? drp = item.ItemUsageRow?.IsDroppable;
            if (drp == null)
                return false;
            return (bool)drp;
        }
    }
}
