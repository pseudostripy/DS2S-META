using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace DS2S_META
{
    public enum PLAYERCLASS : int
    {
        WARRIOR = 1,
        KNIGHT = 2,
        BANDIT = 4,
        CLERIC = 6,
        SORCERER = 7,
        EXPLORER = 8,
        SWORDSMAN = 9,
        DEPRIVED = 10
    }

    public class DS2SClass
    {
        public string Name;
        public PLAYERCLASS ID;
        private byte ByteID => Convert.ToByte((int)ID);
        public short SoulLevel;
        public short Vigor;
        public short Endurance;
        public short Vitality;
        public short Attunement;
        public short Strength;
        public short Dexterity;
        public short Adaptability;
        public short Intelligence;
        public short Faith;

        public Dictionary<ATTR, short> ClassMinLevels = new();

        public DS2SClass() { }
        

        public void BuildMinLevelsDict()
        {
            ClassMinLevels.Add(ATTR.VGR, Vigor);
            ClassMinLevels.Add(ATTR.END, Endurance);
            ClassMinLevels.Add(ATTR.VIT, Vitality);
            ClassMinLevels.Add(ATTR.ATN, Attunement);
            ClassMinLevels.Add(ATTR.STR, Strength);
            ClassMinLevels.Add(ATTR.DEX, Dexterity);
            ClassMinLevels.Add(ATTR.ADP, Adaptability);
            ClassMinLevels.Add(ATTR.INT, Intelligence);
            ClassMinLevels.Add(ATTR.FTH, Faith);
            
        }
        public override string ToString() => Name;

        
    }
}
