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
        public int Class
        {
            get => PHClass?.ReadByte() ?? -1;
            set => PHClass?.WriteByte((byte)value);
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
            get => PHHollowLevel?.ReadInt32() ?? 0;
            set => PHHollowLevel?.WriteInt32(value);
        }
        public int SinnerLevel
        {
            get => PHSinnerLevel?.ReadInt32() ?? 0;
            set => PHSinnerLevel?.WriteInt32(value);
        }
        public int SinnerPoints
        {
            get => PHSinnerPoints?.ReadInt32() ?? 0;
            set => PHSinnerPoints?.WriteInt32(value);
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
        public Dictionary<EQUIP, PHLeaf?> PHEquipment { get; set; } = new();
        public Dictionary<ATTR, PHLeaf?> PHAttributes { get; set; } = new();


        public string GetEquipmentName(EQUIP eqpslot) => PHEquipment[eqpslot]?.ReadInt32().AsMetaName() ?? "";
        public int GetAttributeLevel(ATTR attr) => PHAttributes[attr]?.ReadInt16() ?? 0;
        public void SetAttributeLevel(ATTR attr, int lvl) => PHAttributes[attr]?.WriteInt16((short)lvl);

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
