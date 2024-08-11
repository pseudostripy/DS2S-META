using DS2S_META.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS2S_META.Utils.DS2Hook
{
    internal class SetupCleanupMan (DS2SHook hook)
    {
        internal void Setup_OnHooked()
        {
            hook.VerMan.OnHooked();
            hook.SetupPointers2();
            ParamMan.Initialise(hook);
            hook.SpeedhackMan?.Setup();
        }

        public void Cleanup()
        {
            HandleRivaAndSpeedhackUnhooking();
            ParamMan.Uninitialise();
            hook.MW.HKM.ClearHooks();
        }
        private enum UNHOOKINGSTYLE
        {
            VANILLA,
            SOTFS_NOSH,
            SOTFS_SH_NORIVA_RESTART,
            SOTFS_SH_RIVA_RESTART
        }
        private void HandleRivaAndSpeedhackUnhooking()
        {
            var unhookStyle = GetUnhookStyle();
            Action unhookFunc = unhookStyle switch
            {
                UNHOOKINGSTYLE.VANILLA => UnhookVanilla,
                UNHOOKINGSTYLE.SOTFS_NOSH => RivaHook.OnUnhooked,
                UNHOOKINGSTYLE.SOTFS_SH_NORIVA_RESTART => RivaUnhookSlow,
                UNHOOKINGSTYLE.SOTFS_SH_RIVA_RESTART => UnhookWithRivaRestart,
                _ => throw new Exception("Unexpected UNHOOKINGSTYLE enum")
            };
            unhookFunc(); // call unhooker
        }
        private UNHOOKINGSTYLE GetUnhookStyle()
        {
            // Logic for deciding which Riva/Speedhack unhooking process to use
            if (hook.VerMan.IsVanilla) return UNHOOKINGSTYLE.VANILLA;
            if (hook.SpeedhackMan?.SpeedhackEverEnabled != true) return UNHOOKINGSTYLE.SOTFS_NOSH;
            if (!Properties.Settings.Default.RestartRivaOnClose) return UNHOOKINGSTYLE.SOTFS_SH_NORIVA_RESTART;
            return UNHOOKINGSTYLE.SOTFS_SH_RIVA_RESTART;
        }
        private void UnhookVanilla()
        {
            RivaHook.OnUnhooked();
            hook.SpeedhackMan?.ClearSpeedhackInject();
        }
        private void RivaUnhookSlow()
        {
            // Unload and wait for RIVA to refresh itself ~2mins
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { RivaHook.RefreshEnd(); }));
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                hook.SpeedhackMan?.ClearSpeedhackInject();
                RivaHook.OnUnhooked();
            }));
        }
        private void UnhookWithRivaRestart()
        {
            // Try reopen RIVA programatically
            string rivaExePath = Properties.Settings.Default.RivaExePath;
            bool canFindRiva = File.Exists(rivaExePath);


            List<string> rtssProcNames = new() { "RTSS", "RTSSHooksLoader64" };
            var RTSSprocs = Process.GetProcesses().Where(proc => rtssProcNames.Contains(proc.ProcessName)).ToList();

            if (RTSSprocs.Count == 0)
            {
                // RTSS not open (nothing to do)
                hook.SpeedhackMan?.ClearSpeedhackInject();
                return;
            }
            if (!canFindRiva)
            {
                MetaInfoMessages.RivaNotFound(rivaExePath);
                RivaUnhookSlow();
                return;
            }

            // Kill RTSS and request to reopen it
            hook.SpeedhackMan?.ClearSpeedhackInject();
            foreach (var proc in RTSSprocs)
                proc.Kill();
            Util.ExecuteAsAdmin(rivaExePath);
        }
    }
}
