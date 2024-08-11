using mrousavy;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook
{
    public class CommonGameRequests (DS2SHook hook)
    {
        public enum GIVEOPTIONS
        {
            DEFAULT,
            SHOWDIALOG,
            GIVESILENTLY,
        }

        private AssemblyScripts ASM => hook.ASM;
        public enum WARPOPTIONS
        {
            DEFAULT,
            WARPREST,
            WARPONLY,
        }

        public void UnlockBonfires()
        {
            foreach (var bf in DS2Resource.Bonfires.Where(bf => bf.ID != DS2SBonfire._GameStartId))
                hook.DS2P.BonfiresHGO.SetBonfireLevelById(bf.ID, 1);
        }
        internal bool WarpBonfire(DS2SBonfire toBonfire, bool bWrongWarp, bool restAfterWarp) // Events one day surely
        {
            if (toBonfire == null) return false;
            hook.DS2P.BonfiresHGO.LastBonfireAreaID = toBonfire.AreaID;
            var wopt = restAfterWarp ? WARPOPTIONS.WARPREST : WARPOPTIONS.WARPONLY;
            var bfid = toBonfire.ID != 0 ? toBonfire.ID : DS2Resource.GetBonfireByName("Fire Keepers' Dwelling").ID; // fix _Game Start
            return Warp(bfid, bWrongWarp, wopt);
        }
        internal bool Warp(ushort id, bool areadefault = false, WARPOPTIONS wopt = WARPOPTIONS.DEFAULT)
        {
            bool bsuccess;
            if (hook.Is64Bit)
                bsuccess = ASM.Warp64(id, areadefault);
            else
                bsuccess = ASM.Warp32(id, areadefault);

            // Multiplayer mode cannot warp
            if (!bsuccess)
                return false;

            if (!Enum.IsDefined(typeof(WARPOPTIONS), wopt))
            {
                MetaExceptionStaticHandler.Raise($"Unexpected enum type for WARPOPTIONS. Value received: {wopt}");
                return false;
            }

            // Apply rest after warp
            bool do_rest = wopt switch
            {
                WARPOPTIONS.DEFAULT => Properties.Settings.Default.RestAfterWarp,
                WARPOPTIONS.WARPREST => true,
                WARPOPTIONS.WARPONLY => false,
                _ => throw new Exception("Impossible, thrown properly above")
            };
            if (do_rest)
                AwaitBonfireRest();
            return true;
        }
        internal bool WarpLast()
        {
            // TO TIDY with bonfire objects

            // Handle betwixt start warps:
            bool PrevBonfireSet = hook.DS2P.BonfiresHGO.LastBonfireAreaID != 0 && hook.DS2P.BonfiresHGO.LastBonfireID != 0;
            if (PrevBonfireSet)
                return Warp((ushort)hook.DS2P.BonfiresHGO.LastBonfireID);

            // Handle first area warp:
            int BETWIXTAREA = 167903232;
            ushort BETWIXTBFID = 2650;
            hook.DS2P.BonfiresHGO.LastBonfireAreaID = BETWIXTAREA;
            return Warp(BETWIXTBFID, true);
        }
        internal async void AwaitBonfireRest()
        {
            // This is useful to ensure you're at full hp
            // after a load, so that things like lifering+3
            // effects are accounted for before healing

            bool finishedload = await NextLoadComplete();
            if (!finishedload)
                return; // timeout issue

            // Apply bonfire rest in non-loadscreen
            BonfireRest();
        }
        internal async Task<bool> NextLoadComplete()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int loadingTimeout_ms = 15000;
            int dlay = 10;

            // wait for start of load
            while (!hook.DS2P.CGS.IsLoading)
            {
                await Task.Delay(dlay);
                if (sw.ElapsedMilliseconds > loadingTimeout_ms)
                    return false;
            }

            // Now its loading, wait for it to finish:
            while (hook.DS2P.CGS.IsLoading)
            {
                await Task.Delay(dlay);
                if (sw.ElapsedMilliseconds > loadingTimeout_ms)
                    return false;
            }
            return true;
        }
        internal void BonfireRest()
        {
            ApplySpecialEffect((int)SPECIAL_EFFECT.BONFIREREST);
        }
        internal void ApplySpecialEffect(int spEffect)
        {
            if (hook.Is64Bit)
                hook.ASM.ApplySpecialEffect64(spEffect);
            else if (hook.VerMan.IsOldPatch)
                hook.ASM.ApplySpecialEffect32OP(spEffect);
            else
                hook.ASM.ApplySpecialEffect32(spEffect);
        }
        internal void AddSouls(int numsouls)
        {
            if (hook.Is64Bit)
                hook.ASM.UpdateSoulCount64(numsouls);
            else
                hook.ASM.UpdateSoulCount32(numsouls);
        }
        internal void RestoreHumanity()
        {
            ApplySpecialEffect((int)SPECIAL_EFFECT.RESTOREHUMANITY);
        }

        public void SetNoGravity(bool noGravity)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NOGRAVITY)) return;
            hook.DS2P.CGS.Gravity = !noGravity;
        }
        public void SetNoCollision(bool noCollision)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NOCOLLISION)) return;
            hook.DS2P.CGS.Collision = !noCollision;
        }
        public void SetDisableAI(bool disableAI)
        {
            if (MetaFeature.IsInactive(METAFEATURE.DISABLEAI)) return;
            hook.DS2P.CGS.DisableAI = disableAI;
        }
        public void SetNoDeath(bool noDeath)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NODEATH)) return;
            hook.DS2P.PlayerState.HealthMin = noDeath ? 1 : -99999;
        }

        public void SetInfiniteStamina(bool infiniteStamina)
        {
            if (MetaFeature.IsInactive(METAFEATURE.INFINITESTAMINA)) return;
            hook.DS2P.PlayerState.MinStamina = infiniteStamina ? hook.DS2P.PlayerState.MaxStamina : 0;
        }

        
        public void SetFistOHKO(bool ohko)
        {
            if (MetaFeature.IsInactive(METAFEATURE.OHKO_FIST)) return;
            SetWeaponOHKO(ITEMID.FISTS, ohko);
        }
        internal void SetWeaponOHKO(ITEMID wpn, bool ohko)
        {
            if (!hook.Hooked) return;
            float DMGMOD = 50000;

            // Write to memory
            var wpnrow = ParamMan.WeaponParam?.Rows.First(r => r.ID == (int)wpn) as WeaponRow ?? throw new NullReferenceException();
            wpnrow.DamageMultiplier = ohko ? DMGMOD : 1;
            wpnrow.WriteRow();
        }



        // QoL Wrappers:
        public void GiveItem(ITEMID itemid, short amount = 1, byte upgrade = 0, byte infusion = 0,
                             GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            GiveItem((int)itemid, amount, upgrade, infusion, opt);
        }
        public void GiveItem(int itemid, short amount = 1,
                             byte upgrade = 0, byte infusion = 0,
                             GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            // Simple wrapper for programmer QoL
            var ids = new int[1] { itemid };
            var amounts = new short[1] { amount };
            var upgrades = new byte[1] { upgrade };
            var infusions = new byte[1] { infusion };
            GiveItems(ids, amounts, upgrades, infusions, opt); // call actual implementation
        }
        public void GiveItems(int[] itemids, short[] amounts, GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            // Fix wrapping for optionals
            var len = itemids.Length;
            var upgrades_list = new List<byte>(len);
            var infusions_list = new List<byte>(len);
            for (int i = 0; i < len; i++)
            {
                upgrades_list.Add(0);
                infusions_list.Add(0);
            }

            byte[] upgrades = upgrades_list.ToArray();
            byte[] infusions = infusions_list.ToArray();

            // Call function
            GiveItems(itemids, amounts, upgrades, infusions, opt);
        }

        // Main outward facing interface wrapper
        public void GiveItems(int[] itemids, short[] amounts, byte[] upgrades, byte[] infusions, GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            var showdialog = opt switch
            {
                GIVEOPTIONS.DEFAULT => !Properties.Settings.Default.SilentItemGive,
                GIVEOPTIONS.SHOWDIALOG => true,
                GIVEOPTIONS.GIVESILENTLY => false,
                _ => throw new Exception("Unexpected flag for Silent Item switch")
            };

            // call actual assembly function:
            if (hook.Is64Bit)
                ASM.GiveItems64(itemids, amounts, upgrades, infusions, showdialog);
            else
                ASM.GiveItems32(itemids, amounts, upgrades, infusions, showdialog);
        }
        public void Give17kReward()
        {
            // Add Soul of Pursuer x1 Ring of Blades x1 / 
            var itemids = new int[2] { 0x03D09000, 0x0264CB00 };
            var amounts = new short[2] { 1, 1 };
            GiveItems(itemids, amounts);
            AddSouls(17001);
        }
        public void Give3Chunk1Slab()
        {
            // For the lizard in dlc2
            var items = new ITEMID[2] { ITEMID.TITANITECHUNK, ITEMID.TITANITESLAB };
            var itemids = items.Cast<int>().ToArray();
            var amounts = new short[2] { 3, 1 };
            GiveItems(itemids, amounts);
        }
        public void SetMaxLevels()
        {
            // Possibly to tidy
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.VGR, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.END, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.VIT, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.ATN, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.STR, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.DEX, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.ADP, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.INT, 99);
            hook.DS2P.PlayerData.SetAttributeLevel(ATTR.FTH, 99);
        }
        public void NewTestCharacter()
        {
            // shorthand
            GIVEOPTIONS GIVESILENT = GIVEOPTIONS.GIVESILENTLY;

            // Consumables / Multi-items
            var multi_items = new List<ITEMID>()
                { ITEMID.LIFEGEM, ITEMID.OLDRADIANT, ITEMID.MUSHROOM, ITEMID.DIVINEBLESSING, ITEMID.HUMANEFFIGY,
                  ITEMID.POISONMOSS, ITEMID.WILTEDDUSKHERB, ITEMID.AROMATICOOZE, ITEMID.GOLDPINERESIN, ITEMID.DARKPINERESIN,
                  ITEMID.AGEDFEATHER, ITEMID.FRAGRANTBRANCH, ITEMID.WITCHINGURN, ITEMID.FIREBOMB, ITEMID.BLACKFIREBOMB,
                  ITEMID.DUNGPIE, ITEMID.POISONTHROWINGKNIFE, ITEMID.SOULOFAGREATHERO, ITEMID.OLDDEADONESOUL,
                  ITEMID.ALLURINGSKULL, ITEMID.TORCH, ITEMID.TITANITESHARD, ITEMID.LARGETITANITESHARD,
                  ITEMID.TITANITESLAB, ITEMID.TWINKLINGTITANITE, ITEMID.PETRIFIEDDRAGONBONE, ITEMID.BOLTSTONE,
                  ITEMID.DARKNIGHTSTONE, ITEMID.RAWSTONE, ITEMID.PALESTONE, ITEMID.PHARROSLOCKSTONE,
                  ITEMID.BRIGHTBUG, ITEMID.ASCETIC};
            foreach (int id in multi_items.Cast<int>())
                GiveItem(id, 95, 0, 0, GIVESILENT);

            // Ammo
            var ammo_items = new List<ITEMID>()
                { ITEMID.WOODARROW, ITEMID.IRONARROW, ITEMID.MAGICARROW, ITEMID.FIREARROW,
                  ITEMID.POISONARROW, ITEMID.HEAVYBOLT };
            foreach (int id in ammo_items.Cast<int>())
                GiveItem(id, 950, 0, 0, GIVESILENT);

            // Common Gear:
            var single_items = new List<ITEMID>()
                { ITEMID.ESTUS, ITEMID.BINOCULARS, ITEMID.BUCKLER, ITEMID.GOLDENFALCONSHIELD, ITEMID.IRONPARMA,
                  ITEMID.CHLORANTHYRING1, ITEMID.RINGOFBLADES, ITEMID.CATRING, ITEMID.SILVERSERPENTRING1, ITEMID.FLYNNSRING,
                  ITEMID.SORCERERSSTAFF, ITEMID.CLERICSACREDCHIME, ITEMID.PYROFLAME, ITEMID.DARKWEAPON, ITEMID.SUNLIGHTBLADE,
                  ITEMID.BUTTERFLYSKIRT, ITEMID.BUTTERFLYWINGS, ITEMID.TSELDORAHAT, ITEMID.TSELDORAROBE,
                  ITEMID.TSELDORAGLOVES, ITEMID.TSELDORAPANTS};
            foreach (int id in single_items.Cast<int>())
                GiveItem(id, 1, 0, 0, GIVESILENT);

            // Upgraded testing weapons:
            var upgr_weapons = new List<ITEMID>()
                { ITEMID.DAGGER, ITEMID.RAPIER, ITEMID.MACE, ITEMID.SHORTBOW, ITEMID.LIGHTCROSSBOW, ITEMID.UCHI };
            foreach (int id in upgr_weapons.Cast<int>())
                GiveItem(id, 1, 10, 0, GIVESILENT);

            // Common speedrun weapons:
            GiveItem(ITEMID.RAPIER, 1, 0, 0, GIVESILENT);   // basic rapier
            GiveItem(ITEMID.RAPIER, 1, 10, 3, GIVESILENT);  // lightning rapier
            GiveItem(ITEMID.RAPIER, 1, 10, 4, GIVESILENT);  // dark rapier
            GiveItem(ITEMID.REDIRONTWINBLADE, 1, 10, 3, GIVESILENT);    // lightning RITB
            GiveItem(ITEMID.DBGS, 1, 5, 0, GIVESILENT);

            // Keys
            var keyIds = ParamMan.ItemRows?.Where(ir => ir.ItemUsageID == (int)ITEMUSAGE.ITEMUSAGEKEY)
                       .Select(ir => ir.ItemID).ToList() ?? Enumerable.Empty<int>();
            foreach (var id in keyIds)
                GiveItem(id, 1, 0, 0, GIVESILENT);

            // Gestures:
            GiveItem(ITEMID.DECAPITATEGESTURE, 1, 0, 0); // show visibly

            // Used to create a character with commonly useful things
            RestoreHumanity();
            SetMaxLevels();
            AddSouls(9999999);
            UnlockBonfires();

            // to tidy:
            DS2SBonfire majula = new(168034304, 4650, "The Far Fire");
            hook.DS2P.BonfiresHGO.LastBonfireID = majula.ID;
            hook.DS2P.BonfiresHGO.LastBonfireAreaID = majula.AreaID;
            Warp(majula.ID, false, WARPOPTIONS.WARPREST);
        }
    }
}
