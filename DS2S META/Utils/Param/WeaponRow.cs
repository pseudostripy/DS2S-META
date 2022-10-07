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
    public class WeaponRow : Param.Row
    {
        public float _damageMultiplier;
        public float DamageMultiplier
        {
            get => _damageMultiplier;
            set
            {
                _damageMultiplier = value;
                WriteAt(34, BitConverter.GetBytes(value));
            }
        }
        public int _reinforceID;
        public int ReinforceID
        {
            get => _reinforceID;
            set
            {
                _reinforceID = value;
                WriteAt(2, BitConverter.GetBytes(value));
            }
        }

        // Linked param:
        internal WeaponReinforceRow ReinforceParam => ParamMan.GetLink<WeaponReinforceRow>(ParamMan.PNAME.WEAPON_REINFORCE_PARAM, ReinforceID);
        
        // Constructor:
        public WeaponRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            DamageMultiplier = (float)ReadAt(34);
            ReinforceID = (int)ReadAt(2);
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
