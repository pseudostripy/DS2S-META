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
    public class CustomAttrSpecRow : Param.Row
    {
        // Behind-fields
        internal int _allowedInfusionsBitField;
        internal int AllowedInfusionsBitfield
        {
            get => _allowedInfusionsBitField;
            set
            {
                _allowedInfusionsBitField = value;
                // currently unsettable!
            }
        }

        // Constructor:
        public CustomAttrSpecRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            AllowedInfusionsBitfield = (int)ReadAtFieldNum(0);
        }
    }
}
