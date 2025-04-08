using DS2S_META.Utils;
using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public class LotRdz : GLotRdz<ItemLotRow>
    {
        // Constructors:
        public LotRdz(ItemLotRow vanlot, RandoInfo ri, RDZ_TASKTYPE status) : base(vanlot, ri, status)
        {
            IsDropTable = false;
        }

        public override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {CasualItemSet.LotData[ParamID].Description}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();

            for (int i = 0; i < ShuffledLot.NumDrops; i++)
            {
                sb.Append($"\t{ShuffledLot.Items[i].AsMetaName()}");
                sb.Append($" x{ShuffledLot.Quantities[i]}");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r', '\n');
        }
        public override string GetNeatDescriptionNoId(int itemId, out string area)
        {
            // return area and chopped desc
            area = "[UNKNOWN META AREA]";
            var rawdesc = CasualItemSet.LotData[ParamID].Description;
            if (rawdesc == null)
                return string.Empty;

            var m = SplitArea.Match(rawdesc);
            area = m.Groups["area"].Value;
            var newdesc = m.Groups["desc"].Value.Trim();

            // Add quantity
            var di = VanillaLot?.Flatlist.Where(di => di.ItemID == itemId).FirstOrDefault();
            string quant = di?.Quantity > 1 ? $"x{di.Quantity} " : string.Empty;
            return $"{quant}{newdesc}";
        }
        private static readonly Regex SplitArea = new(@"(?<area>\[.*?\]) (?<desc>.*)");
    }
}
