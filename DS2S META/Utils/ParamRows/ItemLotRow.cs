using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace DS2S_META.Utils
{
    public class ItemLotRow : ItemLotBaseRow
    {
        // Constructors:
        public ItemLotRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            IsDropTable = false;
        }
        internal override int NumDrops => Quantities.Where(q => q != 0).Count();
        internal override ItemLotRow CloneBlank()
        {
            // Performs a deep clone on the Lot blanking all data
            var ilclone = new ItemLotRow(Param, Name, ID, DataOffset)
            {
                _items = new List<int>(new int[10]),
                _quantities = new List<byte>(new byte[10]),
                _reinforcements = new List<byte>(new byte[10]),
                _infusions = new List<byte>(new byte[10])
            };

            for (int i = 0; i < 10; i++)
                ilclone.StoreDataWrapper(MINILOTS.QUANT, i, 0); // ancient dragon memes

            return ilclone;
        }
    }
}
