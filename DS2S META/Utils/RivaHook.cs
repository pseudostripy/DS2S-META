﻿using DS2S_META.Utils;
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
using System.Windows.Threading;

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
        private static void DisplayText() => DisplayTextWrapper(DS2SViewModel.WindowName);
        public static void DisplayTextWrapper(string msg)
        {
            if (!DllLoadable) return; // error already displayed don't retry

            // get user settings
            var xpx = Properties.Settings.Default.RivaXPixels;
            var ypx = Properties.Settings.Default.RivaYPixels;
            var txtsz = Properties.Settings.Default.RivaTextSize;

            try
            {
                var displayStr = msg;
                _ = displayText(MetaIDStr, displayStr, xpx, ypx, txtsz);
            }
            catch (Exception e)
            {
                DllLoadable = false; // only try once
                MetaExceptionStaticHandler.Handle(e);
            }
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
                MetaExceptionStaticHandler.Handle(e);
            }
        }
        public static void Refresh()
        {
            // ensure the hook code goes through first in race
            if (!OnHookComplete) return; 
            ClearText();
            DisplayText();
        }
        public static void RefreshEnd()
        {
            // ensure the hook code goes through first in race
            if (!OnHookComplete) return;
            ClearText();
            DisplayTextWrapper("Closing Meta: RIVA will eventually autofix itself (could be up to 5 mins). Please wait or restart RIVA.");
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
