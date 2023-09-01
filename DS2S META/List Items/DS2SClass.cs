using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    class DS2SClass
    {
        private static readonly Regex ClassEntryRx = new(@"^(?<id>\S+) (?<sl>\S+) (?<vig>\S+) (?<end>\S+) (?<vit>\S+) (?<att>\S+) (?<str>\S+) (?<dex>\S+) (?<adp>\S+) (?<int>\S+) (?<fth>\S+) (?<name>.+)$");

        public string Name;
        public byte ID;
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

        private DS2SClass() { }
        public static DS2SClass ParseNew(string line)
        {
            var cls = new DS2SClass();
            Match classEntry = ClassEntryRx.Match(line);

            cls.Name = classEntry.Groups["name"].Value;
            cls.ID = Convert.ToByte(classEntry.Groups["id"].Value);
            cls.SoulLevel = Convert.ToInt16(classEntry.Groups["sl"].Value);
            cls.Vigor = Convert.ToInt16(classEntry.Groups["vig"].Value);
            cls.Endurance = Convert.ToInt16(classEntry.Groups["end"].Value);
            cls.Vitality = Convert.ToInt16(classEntry.Groups["vit"].Value);
            cls.Attunement = Convert.ToInt16(classEntry.Groups["att"].Value);
            cls.Strength = Convert.ToInt16(classEntry.Groups["str"].Value);
            cls.Dexterity = Convert.ToInt16(classEntry.Groups["dex"].Value);
            cls.Adaptability = Convert.ToInt16(classEntry.Groups["adp"].Value);
            cls.Intelligence = Convert.ToInt16(classEntry.Groups["int"].Value);
            cls.Faith = Convert.ToInt16(classEntry.Groups["fth"].Value);
            return cls;
        }
        public override string ToString() => Name;
    }
}
