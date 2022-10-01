using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META
{
    // Taken from DSR Gadget because TK code is better than anything I could write.
    // Parses output from https://defuse.ca/online-x86-assembler.htm
    // I like to keep the whole thing for quick reference to line numbers and so on
    static class DS2SAssembly
    {
        private static Regex asmLineRx = new Regex(@"^[\w\d]+:\s+((?:[\w\d][\w\d] ?)+)");

        private static byte[] LoadDefuseOutput(string lines)
        {
            List<byte> bytes = new List<byte>();
            foreach (string line in Regex.Split(lines, "[\r\n]+"))
            {
                Match match = asmLineRx.Match(line);
                string hexes = match.Groups[1].Value;
                foreach (Match hex in Regex.Matches(hexes, @"\S+"))
                    bytes.Add(Byte.Parse(hex.Value, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            return bytes.ToArray();
        }

        public static byte[] AddSouls = LoadDefuseOutput(Properties.Resources.AddSouls);
        public static byte[] GetItem = LoadDefuseOutput(Properties.Resources.GiveItemWithMenu);
        public static byte[] GetItemNoMenu = LoadDefuseOutput(Properties.Resources.GiveItemWithoutMenu);
        public static byte[] SpeedFactorAccel = LoadDefuseOutput(Properties.Resources.SpeedFactorAccel);
        public static byte[] OgSpeedFactorAccel = LoadDefuseOutput(Properties.Resources.OgSpeedFactorAccel);
        public static byte[] SpeedFactor = LoadDefuseOutput(Properties.Resources.SpeedFactor);
        public static byte[] OgSpeedFactor = LoadDefuseOutput(Properties.Resources.OgSpeedFactor);
        public static byte[] BonfireWarp = LoadDefuseOutput(Properties.Resources.BonfireWarp);
        public static byte[] ApplySpecialEffect = LoadDefuseOutput(Properties.Resources.ApplySpecialEffect);
    }
}
