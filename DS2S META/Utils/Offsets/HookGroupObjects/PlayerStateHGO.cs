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
    public class PlayerStateHGO : HGO
    {
        // Direct Hook properties
        public PHLeaf? PHHP;
        public PHLeaf? PHHPMax;
        public PHLeaf? PHHPMin;
        public PHLeaf? PHHPCap;
        public PHLeaf? PHSP;
        public PHLeaf? PHSPMax;
        public PHLeaf? PHCurrPoise;

        public PHLeaf? PHPosX;
        public PHLeaf? PHPosY;
        public PHLeaf? PHPosZ;
        public PHLeaf? PHAngX;
        public PHLeaf? PHAngY;
        public PHLeaf? PHAngZ;
        public PHLeaf? PHStableX;
        public PHLeaf? PHStableY;
        public PHLeaf? PHStableZ;

        public float[] Pos
        {
            get => new float[3] { PosX, PosY, PosZ };
            set
            {
                PosX = value[0];
                PosY = value[1];
                PosZ = value[2];
            }
        }
        public float PosX
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosX) : 0;
            set
            {
                if (Reading || !InGame) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosX, value);
            }
        }
        public float PosY
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosY) : 0;
            set
            {
                if (Reading || !InGame) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosY, value);
            }
        }
        public float PosZ
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosZ) : 0;
            set
            {
                if (Reading || !InGame) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosZ, value);
            }
        }
        public float[] Ang
        {
            get => new float[3] { AngX, AngY, AngZ };
            set
            {
                AngX = value[0];
                AngY = value[1];
                AngZ = value[2];
            }
        }
        private float AngX
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngX) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngX, value);
        }
        private float AngY
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngY) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngY, value);
        }
        private float AngZ
        {
            get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngZ) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngZ, value);
        }
        public float[] StablePos
        {
            get => new float[3] { StableX, StableY, StableZ };
            set
            {
                StableX = value[0];
                StableY = value[1];
                StableZ = value[2];
            }
        }
        private float StableX
        {
            get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpX1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX3, value);
            }
        }
        private float StableY
        {
            get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpY1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY3, value);
            }
        }
        private float StableZ
        {
            get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpZ1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ3, value);
            }
        }


        public PlayerStateHGO(DS2SHook hook, Dictionary<string, PHLeaf?> playerGrp) : base(hook)
        {
            PHHP = playerGrp["HP"];
            PHHPMax = playerGrp["HPMax"];
            PHHPMin = playerGrp["HPMin"];
            PHHPCap = playerGrp["HPCap"];
            PHSP = playerGrp["SP"];
            PHSPMax = playerGrp["SPMax"];
            PHCurrPoise = playerGrp["CurrPoise"];


        }

        public int Health
        {
            get => PHHP?.ReadInt32() ?? 0;
            set => PHHP?.WriteInt32(value);
        }
        public int HealthMax
        {
            get => PHHPMax?.ReadInt32() ?? 0;
            set => PHHPMax?.WriteInt32(value);
        }
        public int HealthMin
        {
            get => PHHPMin?.ReadInt32() ?? 0;
            set => PHHPMin?.WriteInt32(value);
        }
        public int HealthCap
        {
            get
            {
                if (!Hook.DS2P.CGS.InGame) return 0;
                var cap = PHHPCap?.ReadInt32() ?? 0;
                return cap < HealthMax ? cap : HealthMax;
            }
            set => PHHPCap?.WriteInt32(value);
        }
        public float Stamina
        {
            get => PHSP?.ReadSingle() ?? 0f;
            set => PHSP?.WriteSingle(value);
        }
        public float MaxStamina
        {
            get => PHSPMax?.ReadSingle() ?? 0f;
            set => PHSPMax?.WriteSingle(value);
        }
        public float CurrPoise
        {
            get => PHCurrPoise?.ReadSingle() ?? 0f;
            set => PHCurrPoise?.WriteSingle(value);
        }

        public override void UpdateProperties()
        {
            OnPropertyChanged(nameof(Health));
            OnPropertyChanged(nameof(HealthMax));
            OnPropertyChanged(nameof(HealthMin));
            OnPropertyChanged(nameof(HealthCap));
            OnPropertyChanged(nameof(Stamina));
            OnPropertyChanged(nameof(MaxStamina));
            OnPropertyChanged(nameof(CurrPoise));
        }
    }
}
