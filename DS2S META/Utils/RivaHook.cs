using DS2S_META.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    internal static class RivaHook
    {
        // C++ definitions
        [DllImport("./Resources/DLLs/RivaHook.dll", CharSet = CharSet.Ansi)]
        private static extern int displayText(string id, string txt, int x, int y, int sz);

        [DllImport("./Resources/DLLs/RivaHook.dll", CharSet = CharSet.Ansi)]
        private static extern int clearText(string id);

        // C# Wrappers
        static readonly string MetaIDStr = "meta";
        private static void DisplayText()
        {
            // get user settings
            var xpx = Properties.Settings.Default.RivaXPixels;
            var ypx = Properties.Settings.Default.RivaYPixels;
            var txtsz = Properties.Settings.Default.RivaTextSize;

            var str = $"META v{DS2SViewModel.MVI.MetaVersionStr}";
            _ = displayText(MetaIDStr, str, xpx, ypx, txtsz);
            return;
        }
        private static void ClearText()
        {
            _ = clearText(MetaIDStr);
        }
        public static void Refresh()
        {
            ClearText();
            DisplayText();
        }

        // On events
        public static void OnHooked() => DisplayText();
        public static void OnUnhooked() => ClearText();
    }
}
