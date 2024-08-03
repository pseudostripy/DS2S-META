using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.OffsetClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class PlayerDataHGO : HGO
    {
        // Pointers
        private readonly PHLeaf? PHSoulLevel;
        private readonly PHLeaf? PHSoulMemory;
        private readonly PHLeaf? PHSoulMemory2;
        private readonly PHLeaf? PHMaxEquipLoad;
        private readonly PHLeaf? PHSouls;
        private readonly PHLeaf? PHTotalDeaths;
        private readonly PHLeaf? PHHollowLevel;
        private readonly PHLeaf? PHSinnerLevel;
        private readonly PHLeaf? PHSinnerPoints;
        private readonly PHLeaf? PHPlayerName;
        private readonly PHLeaf? PHClass;

        // Properties
        public int SoulLevel
        {
            get => PHSoulLevel?.ReadInt32() ?? 0;
            set => PHSoulLevel?.WriteInt32(value);
        }
        public string CharacterName
        {
            get => PHPlayerName?.ReadString(Encoding.Unicode, 0x22) ?? "";
            set
            {
                PHPlayerName?.WriteString(Encoding.Unicode, 0x22, value);
                OnPropertyChanged(nameof(CharacterName));
            }
        }
        public PLAYERCLASS? Class
        {
            get
            {
                var hookVal = PHClass?.ReadByte();
                if (hookVal == null || hookVal == 0) return null;
                return (PLAYERCLASS)hookVal;
            }
            set
            {
                var toWrite = value == null ? (byte)0 : (byte)value;
                PHClass?.WriteByte(toWrite);
            }
        }
        public int SoulMemory
        {
            get => PHSoulMemory?.ReadInt32() ?? 0;
            set => PHSoulMemory?.WriteInt32(value);
        }
        public int SoulMemory2
        {
            get => PHSoulMemory2?.ReadInt32() ?? 0;
            set => PHSoulMemory2?.WriteInt32(value);
        }
        public int MaxEquipLoad
        {
            get => PHMaxEquipLoad?.ReadInt32() ?? 0;
            set => PHMaxEquipLoad?.WriteInt32(value);
        }
        public int TotalDeaths
        {
            get => PHTotalDeaths?.ReadInt32() ?? 0;
            set => PHTotalDeaths?.WriteInt32(value);
        }
        public int HollowLevel
        {
            get => PHHollowLevel?.ReadByte() ?? 0;
            set
            {
                byte b = Convert.ToByte(value); // crash here if bad input
                PHHollowLevel?.WriteByte((byte)value);
            }
        }
        public int SinnerLevel
        {
            get => PHSinnerLevel?.ReadByte() ?? 0;
            set
            {
                byte b = Convert.ToByte(value); // crash here if bad input
                PHHollowLevel?.WriteByte((byte)value);
            }
        }
        public int SinnerPoints
        {
            get => PHSinnerPoints?.ReadByte() ?? 0;
            set
            {
                byte b = Convert.ToByte(value); // crash here if bad input
                PHHollowLevel?.WriteByte((byte)value);
            }
        }
        public int Souls => PHSouls?.ReadInt32() ?? -1;
        
        // Property Groups
        public Dictionary<EQUIP, string> Equipment { get; set; } = new();
        public Dictionary<ATTR, int> AttributeLevels { get; set; } = new();
        
        private readonly Dictionary<string, EQUIP> PHEquipInterface = new()
        {
            { "Legs", EQUIP.LEGS },
            { "Arms", EQUIP.ARMS },
            { "Chest", EQUIP.CHEST },
            { "Head", EQUIP.HEAD },
            { "RightHand1", EQUIP.RIGHTHAND1 },
            { "RightHand2", EQUIP.RIGHTHAND2 },
            { "RightHand3", EQUIP.RIGHTHAND3 },
            { "LeftHand1", EQUIP.LEFTHAND1 },
            { "LeftHand2", EQUIP.LEFTHAND2 },
            { "LeftHand3", EQUIP.LEFTHAND3 },
        };
        private Dictionary<EQUIP, PHLeaf?> PHEquipment { get; set; } = new();
        private Dictionary<ATTR, PHLeaf?> PHAttributes { get; set; } = new();


        public string GetEquipmentName(EQUIP eqpslot) => PHEquipment[eqpslot]?.ReadInt32().AsMetaName() ?? "";
        public int GetAttributeLevel(ATTR attr) => PHAttributes[attr]?.ReadInt16() ?? 0;
        public void SetAttributeLevel(ATTR attr, int lvl) => PHAttributes[attr]?.WriteInt16((short)lvl);

        // Soul helper functions
        public void UpdateSoulLevel()
        {
            var charClass = DS2Resource.Classes.FirstOrDefault(c => c.ID == Class);
            if (charClass == null) return;

            var soulLevel = GetSoulLevel(charClass);
            SoulLevel = soulLevel;
            var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);
            if (reqSoulMemory > SoulMemory)
            {
                SoulMemory = reqSoulMemory;
                SoulMemory2 = reqSoulMemory;
            }
        }
        private int GetSoulLevel(DS2SClass charClass)
        {
            int sl = charClass.SoulLevel;
            sl += GetAttributeLevel(ATTR.VGR) - charClass.Vigor;
            sl += GetAttributeLevel(ATTR.ATN) - charClass.Attunement;
            sl += GetAttributeLevel(ATTR.VIT) - charClass.Vitality;
            sl += GetAttributeLevel(ATTR.END) - charClass.Endurance;
            sl += GetAttributeLevel(ATTR.STR) - charClass.Strength;
            sl += GetAttributeLevel(ATTR.DEX) - charClass.Dexterity;
            sl += GetAttributeLevel(ATTR.ADP) - charClass.Adaptability;
            sl += GetAttributeLevel(ATTR.INT) - charClass.Intelligence;
            sl += GetAttributeLevel(ATTR.FTH) - charClass.Faith;
            return sl;
        }
        public void ResetSoulMemory()
        {
            var charClass = DS2Resource.Classes.FirstOrDefault(c => c.ID == Class);
            if (charClass == null) return;

            var soulLevel = GetSoulLevel(charClass);
            var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);

            SoulMemory = reqSoulMemory;
            SoulMemory2 = reqSoulMemory;
        }
        private static int GetRequiredSoulMemory(int SL, int baseSL)
        {
            int soulMemory = 0;
            var levelCosts = GetLevelRequirements();
            for (int i = baseSL; i < SL; i++)
            {
                var index = i <= 850 ? i : 850;
                soulMemory += levelCosts[index];
            }
            return soulMemory;
        }
        private static readonly List<int> Levels = new();
        private static List<int> GetLevelRequirements()
        {
            if (Levels.Count > 0) return Levels;

            // build Levels object
            if (ParamMan.PlayerLevelUpSoulsParam == null)
                throw new NullReferenceException("Level up cost param not found");

            foreach (var row in ParamMan.PlayerLevelUpSoulsParam.Rows.Cast<PlayerLevelUpSoulsRow>())
                Levels.Add(row.LevelCost);
            return Levels;
        }


        // Constructor
        public PlayerDataHGO(DS2SHook hook, Dictionary<string, PHLeaf?> equipGrp,
                            Dictionary<string, PHLeaf?> attrGroup,
                            Dictionary<string, PHLeaf?> playerParamGrp,
                            Dictionary<string, PHLeaf?> otherLeaves) 
                            : base(hook)
        {
            foreach (var kvp in equipGrp)
                PHEquipment.Add(PHEquipInterface[kvp.Key], kvp.Value);

            foreach (var kvp in attrGroup)
            {
                _ = Enum.TryParse(kvp.Key, out ATTR attr);
                PHAttributes.Add(attr, kvp.Value);
            }

            PHSoulLevel = otherLeaves["SoulLevel"];
            PHPlayerName = otherLeaves["PlayerName"];
            PHClass = otherLeaves["Class"];

            PHSoulMemory = playerParamGrp["SoulMemory"];
            PHSoulMemory2 = playerParamGrp["SoulMemory2"];
            PHMaxEquipLoad = playerParamGrp["MaxEquipLoad"];
            PHSouls = playerParamGrp["Souls"];
            PHTotalDeaths = playerParamGrp["TotalDeaths"];
            PHHollowLevel = playerParamGrp["HollowLevel"];
            PHSinnerLevel = playerParamGrp["SinnerLevel"];
            PHSinnerPoints = playerParamGrp["SinnerPoints"];
        }

        public override void UpdateProperties()
        {
            Equipment = PHEquipment.ToDictionary(kvp => kvp.Key, kvp => GetEquipmentName(kvp.Key));
            AttributeLevels = PHAttributes.ToDictionary(kvp => kvp.Key, kvp => GetAttributeLevel(kvp.Key));

            OnPropertyChanged(nameof(CharacterName));
            OnPropertyChanged(nameof(Class));
            OnPropertyChanged(nameof(SoulLevel));
            
            OnPropertyChanged(nameof(SoulMemory));
            OnPropertyChanged(nameof(MaxEquipLoad));
            OnPropertyChanged(nameof(TotalDeaths));
            OnPropertyChanged(nameof(HollowLevel));
            OnPropertyChanged(nameof(SinnerLevel));
            OnPropertyChanged(nameof(SinnerPoints));
        }
    }
}
