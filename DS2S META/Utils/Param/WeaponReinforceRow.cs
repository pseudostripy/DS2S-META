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
    public class WeaponReinforceRow : Param.Row
    {
        internal int _maxReinforce;
        internal int MaxReinforce
        {
            get => _maxReinforce;
            set
            {
                _maxReinforce = value;
                WriteAt(18, BitConverter.GetBytes(value));
            }
        }
        internal int CustomSpecAttrID;

        
        // Constructor:
        public WeaponReinforceRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            MaxReinforce = (int)ReadAt(18);
            CustomSpecAttrID = (int)ReadAt(58);
        }
        
        public object ReadAt(int fieldindex) => Data[fieldindex];
        public void WriteAt(int fieldindex, byte[] valuebytes)
        {
            // Note: this function isn't generalised properly yet
            int fieldoffset = Param.Fields[fieldindex].FieldOffset;
            Array.Copy(valuebytes, 0, RowBytes, fieldoffset, valuebytes.Length);
        }
    }
}
