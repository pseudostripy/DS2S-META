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
        public int _maxReinforce;

        public float MinUpgrDmgPhys;
        public float MaxUpgrDmgPhys;
        public int MaxReinforce
        {
            get => _maxReinforce;
            set
            {
                _maxReinforce = value;
                WriteAt(18, BitConverter.GetBytes(value));
            }
        }
        public int WeaponStatsAffectID;
        public int CustomSpecAttrID;

        public WeaponStatsAffectRow? WeaponStatsAffectRow => ParamMan.GetLink<WeaponStatsAffectRow>(ParamMan.WeaponStatsAffect, WeaponStatsAffectID);
        public CustomAttrSpecRow? CustomAttrSpec => ParamMan.GetLink<CustomAttrSpecRow>(ParamMan.CustomAttrSpecParam, CustomSpecAttrID);


        // Constructor:
        public WeaponReinforceRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            MinUpgrDmgPhys = (float)ReadAt(0);
            MaxUpgrDmgPhys = (float)ReadAt(9);
            MaxReinforce = (int)ReadAt(18);
            WeaponStatsAffectID = (int)ReadAt(19);
            CustomSpecAttrID = (int)ReadAt(58);
        }

        // Methods:
        public List<DS2SInfusion> GetInfusionList()
        {
            var infusions = new List<DS2SInfusion>() { DS2SInfusion.Infusions[0] };
            if (CustomAttrSpec == null)
                return infusions;

            var bitField = CustomAttrSpec?.AllowedInfusionsBitfield;
            if (bitField == 0)
                return infusions;

            for (int i = 1; i < DS2SInfusion.Infusions.Count; i++)
            {
                if ((bitField & (1 << i)) != 0)
                    infusions.Add(DS2SInfusion.Infusions[i]);
            }

            return infusions;
        }

        public float GetPhysDmg(int upgr)
        {
            return MinUpgrDmgPhys + (MaxUpgrDmgPhys - MinUpgrDmgPhys) * upgr / MaxReinforce;
        }
    }
}
