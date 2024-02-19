using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class DropRdz : GLotRdz<ItemDropRow>
    {
        // Constructors:
        internal DropRdz(ItemDropRow vanlot, RandoInfo ri, RDZ_TASKTYPE status) : base(vanlot, ri, status)
        {
            IsDropTable = true;
        }

        internal override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {CasualItemSet.DropData[ParamID].Description}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();

            for (int i = 0; i < ShuffledLot.NumDrops; i++)
            {
                sb.Append($"\t{ShuffledLot.Items[i].AsMetaName()}");
                sb.Append($" x{ShuffledLot.Quantities[i]}");
                sb.Append($" ({ShuffledLot.Chances[i]}%)");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r', '\n');
        }
        private static Regex SplitArea = new(@"(?<area>\[.*?\]) (?<desc>.*)");
        internal override string GetNeatDescriptionNoId(int itemId, out string area)
        {
            area = "[UNKNOWN META AREA]";
            var rawdesc = CasualItemSet.DropData[ParamID].Description;
            if (rawdesc == null)
                return string.Empty;

            string droptest = IsGuaranteedDrop ? "100% drop." : "random drop.";
            var m = SplitArea.Match(rawdesc ?? "");
            area = m.Groups["area"].Value;
            string desc = m.Groups["desc"].Value;

            var di = VanillaLot?.Flatlist.Where(di => di.ItemID == itemId).FirstOrDefault();
            string quant = di?.Quantity > 1 ? $"x{di.Quantity} " : string.Empty;

            if (ParamID == 5036000)
                droptest = "67% drop."; // DLC2 lizard. Theres probs others I haven't caught programatically.

            return $"{droptest} {quant}{desc}";
        }
        internal bool IsGuaranteedDrop
        {
            get
            {
                if (VanillaLot == null)
                    return false;
                for (int i = 0; i < VanillaLot.NumDrops; i++)
                {
                    if (VanillaLot.Chances[i] != 100F)
                        return false;
                }
                return true;
            }
        }
    }
}
