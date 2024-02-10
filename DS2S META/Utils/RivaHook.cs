using DS2S_META.Utils;
using DS2S_META.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS2S_META
{
    internal static class RivaHook
    {
        // C++ definitions
        private const string dllname = "Resources\\DLLs\\RivaHook.dll"; // relative to meta.exe folder
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport(dllname, CharSet = CharSet.Ansi)]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
        private static extern int displayText(string id, string txt, int x, int y, int sz);

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport(dllname, CharSet = CharSet.Ansi)]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
        private static extern int clearText(string id);
        private static bool DllLoadable = true;

        // C# Wrappers
        static readonly string MetaIDStr = "meta";
        private static bool OnHookComplete = false;
        private static void DisplayText()
        {
            if (!DllLoadable) return; // error already displayed don't retry
            
            // get user settings
            var xpx = Properties.Settings.Default.RivaXPixels;
            var ypx = Properties.Settings.Default.RivaYPixels;
            var txtsz = Properties.Settings.Default.RivaTextSize;

            try
            {
                var str = $"META v{DS2SViewModel.MVI.MetaVersionStr}";
                _ = displayText(MetaIDStr, str, xpx, ypx, txtsz);
            }
            catch (Exception e)
            {
                DllLoadable = false; // only try once
                HandleMetaException(e);
            }
        }
        private static void ShowMetaExceptionWindow(string logmsg)
        {
            var meWindow = new MetaException(logmsg); // notify user on screen
            meWindow.ShowDialog();
        }
        private static void ClearText()
        {
            if (!DllLoadable) return; // error already displayed don't retry
            try
            {
                _ = clearText(MetaIDStr);
            }
            catch (Exception e)
            {
                DllLoadable = false; // only try once
                HandleMetaException(e);
            }
        }
        public static void HandleMetaException(Exception e)
        {
            // Used to cleanup exception message cleanup before passing into Dispatcher
            // thread for handling UI error messaging
            ((App)Application.Current).LogCaughtException(e);   // dump to log file

            // Invoke on Dispatcher thread to avoid STA errors
            var logmsg = e.ToLogString(Environment.StackTrace); // get clean stack trace
            Application.Current.Dispatcher.Invoke(() => ShowMetaExceptionWindow(logmsg));
        }
        public static void Refresh()
        {
            // ensure the hook code goes through first in race
            if (!OnHookComplete) return; 
            ClearText();
            DisplayText();
        }

        // On events
        public static void OnHooked()
        {
            DisplayText();
            OnHookComplete = true;
        } 

        public static void OnUnhooked() => ClearText();
    }
}
