using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    /// <summary>
    /// Data Class for storing Weapons
    /// </summary>
    public class ItemUsageRow : Param.Row
    {
        // Constructor:
        public ItemUsageRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
        }
    }
}
