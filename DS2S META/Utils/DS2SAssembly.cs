using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Resources;

namespace DS2S_META
{
    // Taken from DSR Gadget because TK code is better than anything I could write.
    // Parses output from https://defuse.ca/online-x86-assembler.htm
    // I like to keep the whole thing for quick reference to line numbers and so on
    static class DS2SAssembly
    {
        private static readonly Regex asmLineRx = new(@"^[\w\d]+:\s+((?:[\w\d][\w\d] ?)+)");

        private static byte[] LoadDefuseOutput(string lines)
        {
            List<byte> bytes = new();
            foreach (string line in Regex.Split(lines, "[\r\n]+"))
            {
                Match match = asmLineRx.Match(line);
                string hexes = match.Groups[1].Value;
                foreach (Match hex in Regex.Matches(hexes, @"\S+").Cast<Match>())
                    bytes.Add(byte.Parse(hex.Value, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            return bytes.ToArray();
        }

        public static byte[] AddSouls = LoadDefuseOutput(Properties.Resources.AddSouls);
        public static byte[] GiveItem64 = LoadDefuseOutput(Properties.Resources.GiveItemWithMenu64);
        public static byte[] GiveItem32 = LoadDefuseOutput(Properties.Resources.GiveItemWithMenu32);
        public static byte[] GetItemNoMenu = LoadDefuseOutput(Properties.Resources.GiveItemWithoutMenu);
        public static byte[] SpeedFactorAccel = LoadDefuseOutput(Properties.Resources.SpeedFactorAccel);
        public static byte[] OgSpeedFactorAccel = LoadDefuseOutput(Properties.Resources.OgSpeedFactorAccel);
        public static byte[] SpeedFactor = LoadDefuseOutput(Properties.Resources.SpeedFactor);
        public static byte[] OgSpeedFactor = LoadDefuseOutput(Properties.Resources.OgSpeedFactor);
        public static byte[] BonfireWarp64 = LoadDefuseOutput(Properties.Resources.BonfireWarp64);
        public static byte[] BonfireWarp32 = LoadDefuseOutput(Properties.Resources.BonfireWarp32);
        public static byte[] ApplySpecialEffect64 = LoadDefuseOutput(Properties.Resources.ApplySpecialEffect64);
        public static byte[] ApplySpecialEffect32 = LoadDefuseOutput(Properties.Resources.ApplySpecialEffect32);

        // Debugging resource memes
        //public static byte[] AddSouls = new byte[1];
        //public static byte[] GetItem = new byte[1];
        //public static byte[] GetItemNoMenu = new byte[1];
        //public static byte[] SpeedFactorAccel = new byte[1];
        //public static byte[] OgSpeedFactorAccel = new byte[1];
        //public static byte[] SpeedFactor = new byte[1];
        //public static byte[] OgSpeedFactor = new byte[1];
        //public static byte[] BonfireWarp = new byte[1];
        //public static byte[] ApplySpecialEffect64 = new byte[1];
        //public static byte[] ApplySpecialEffect32 = new byte[1];


    }
}
