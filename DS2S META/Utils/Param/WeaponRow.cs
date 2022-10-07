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
    public class WeaponRow
    {
        internal Param.Row ParamRow;
        internal int ID => ParamRow.ID;

        internal float _damageMultiplier;
        internal float DamageMultiplier
        {
            get => _damageMultiplier;
            set
            {
                _damageMultiplier = value;
                WriteAt(34, BitConverter.GetBytes(value));
            }
        }
        internal int _reinforceID;
        internal int ReinforceID
        {
            get => _reinforceID;
            set
            {
                _reinforceID = value;
                WriteAt(2, BitConverter.GetBytes(value));
            }
        }
        internal WeaponReinforce ReinforceParam;
        
        // Constructor:
        internal WeaponRow(Param.Row paramrow, Param wrparam)
        {
            // Unpack data:
            ParamRow = paramrow;

            DamageMultiplier = (float)ReadAt(34);
            ReinforceID = (int)ReadAt(2);
            ReinforceParam = new(wrparam.Rows.Where(row => row.ID == ReinforceID).First());
        }
        
        public object ReadAt(int fieldindex) => ParamRow.Data[fieldindex];
        public void WriteAt(int fieldindex, byte[] valuebytes)
        {
            // Note: this function isn't generalised properly yet
            int fieldoffset = ParamRow.Param.Fields[fieldindex].FieldOffset;
            Array.Copy(valuebytes, 0, ParamRow.RowBytes, fieldoffset, valuebytes.Length);
        }
    }
}
