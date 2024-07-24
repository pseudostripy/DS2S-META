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
        private bool InGame => Hook.DS2P.CGS.InGame; // shorthand

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
        public Dictionary<string, PHLeaf?> PHWarpGroup;

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
            get => InGame ? PHPosX?.ReadSingle() ?? 0 : 0;
            set
            {
                if (!InGame) return;
                PHPosX?.WriteSingle(value);
            }
        }
        public float PosY
        {
            get => InGame ? PHPosY?.ReadSingle() ?? 0 : 0;
            set
            {
                if (!InGame) return;
                PHPosY?.WriteSingle(value);
            }
        }
        public float PosZ
        {
            get => InGame ? PHPosZ?.ReadSingle() ?? 0 : 0;
            set
            {
                if (!InGame) return;
                PHPosZ?.WriteSingle(value);
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
            get => InGame ? PHAngX?.ReadSingle() ?? 0 : 0;
            set => PHAngX?.WriteSingle(value);
        }
        private float AngY
        {
            get => InGame ? PHAngY?.ReadSingle() ?? 0 : 0;
            set => PHAngY?.WriteSingle(value);
        }
        private float AngZ
        {
            get => InGame ? PHAngZ?.ReadSingle() ?? 0 : 0;
            set => PHAngZ?.WriteSingle(value);
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
            get => InGame ? PHWarpGroup["WarpX1"]?.ReadSingle() ?? 0 : 0;
            set
            {
                PHWarpGroup["WarpX1"]?.WriteSingle(value);
                PHWarpGroup["WarpX2"]?.WriteSingle(value);
                PHWarpGroup["WarpX3"]?.WriteSingle(value);
            }
        }
        private float StableY
        {
            get => InGame ? PHWarpGroup["WarpY1"]?.ReadSingle() ?? 0 : 0;
            set
            {
                PHWarpGroup["WarpY1"]?.WriteSingle(value);
                PHWarpGroup["WarpY2"]?.WriteSingle(value);
                PHWarpGroup["WarpY3"]?.WriteSingle(value);
            }
        }
        private float StableZ
        {
            get => InGame ? PHWarpGroup["WarpZ1"]?.ReadSingle() ?? 0 : 0;
            set
            {
                PHWarpGroup["WarpZ1"]?.WriteSingle(value);
                PHWarpGroup["WarpZ2"]?.WriteSingle(value);
                PHWarpGroup["WarpZ3"]?.WriteSingle(value);
            }
        }


        public PlayerStateHGO(DS2SHook hook, Dictionary<string, PHLeaf?> playerGrp, 
                                Dictionary<string,PHLeaf?> warpGrp) : base(hook)
        {
            PHHP = playerGrp["HP"];
            PHHPMax = playerGrp["HPMax"];
            PHHPMin = playerGrp["HPMin"];
            PHHPCap = playerGrp["HPCap"];
            PHSP = playerGrp["SP"];
            PHSPMax = playerGrp["SPMax"];
            PHCurrPoise = playerGrp["CurrPoise"];

            PHWarpGroup = warpGrp;
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
