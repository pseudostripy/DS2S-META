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
    public class ArrowRow : Param.Row
    {
        public enum AmmoType
        {
            ARROW = 1,
            GREATARROW = 2,
            BOLT = 4,
        }

        // Behind-fields
        private byte _ammunitionType;
        
        internal byte AmmunitionType
        {
            get => _ammunitionType;
            set
            {
                _ammunitionType = value;
                WriteAt(29, BitConverter.GetBytes(value));
            }
        }
        
        // Constructor:
        public ArrowRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            AmmunitionType = (byte)ReadAt(29);
        }
    }
}
