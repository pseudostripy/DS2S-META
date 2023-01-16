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
    public class WeaponStatsAffectRow : Param.Row
    {
        public enum SCTYPE // scaling type
        { 
            STR = 0,
            DEX = 1,
            MAGIC = 2,
            LIGHTNING = 3,
            FIRE = 4,
            DARK = 5,
            POISON = 6,
            BLEED = 7,
            UNK = 8,
        } 

        // Behind-fields
        private float _baseDmgMultiplier;

        internal float BaseDmgMultiplier
        {
            get => _baseDmgMultiplier;
            set
            {
                _baseDmgMultiplier = value;
                WriteAt(0, BitConverter.GetBytes(value));
            }
        }
        
        // Constructor:
        public WeaponStatsAffectRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            _baseDmgMultiplier = (float)ReadAt(0);
        }

        public float ReadScalingValue(SCTYPE sctype, int upgr)
        {
            return (float)ReadAt(2 + upgr * 9 + (int)sctype);
        }
    }
}
