using DS2S_META.Randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils;

namespace DS2S_META.Utils
{
    public class ArmorReinforceRow : Param.Row
    {
        // Behind-fields:
        private int indSlashDef { get; set; }
        private int indThrustDef { get; set; }
        private int indStrikeDef { get; set; }
        private int indStandardDef { get; set; }
        private int indMagicAbsorb { get; set; }
        private int indLightningAbsorb { get; set; }
        private int indFireAbsorb { get; set; }
        private int indDarkAbsorb { get; set; }
        private int indPoisonResist { get; set; }
        private int indBleedResist { get; set; }
        private int indPetrifyResist { get; set; }
        private int indCurseResist { get; set; }
        private int indMaxReinforceLevel { get; set; }
        private int indUpgradeMaterial { get; set; }
        private int indReinforceCostID { get; set; }
        
        public float _SlashDef;
        public float _ThrustDef;
        public float _StrikeDef;
        public float _StandardDef;
        public float _MagicAbsorb;
        public float _LightningAbsorb;
        public float _FireAbsorb;
        public float _DarkAbsorb;
        public float _PoisonResist;
        private float _BleedResist;
        private float _PetrifyResist;
        private float _CurseResist;
        private int _MaxReinforceLevel;
        private int _UpgradeMaterial;
        private int _ReinforceCostID;

        // Properties:
        public float SlashDef
        {
            get => _SlashDef;
            set
            {
                _SlashDef = value;
                WriteAt(indSlashDef, BitConverter.GetBytes(value));
            }
        }
        public float ThrustDef
        {
            get => _ThrustDef;
            set
            {
                _ThrustDef = value;
                WriteAt(indThrustDef, BitConverter.GetBytes(value));
            }
        }
        public float StrikeDef
        {
            get => _StrikeDef;
            set
            {
                _StrikeDef = value;
                WriteAt(indSlashDef, BitConverter.GetBytes(value));
            }
        }
        public float StandardDef
        {
            get => _StandardDef;
            set
            {
                _StandardDef = value;
                WriteAt(indStandardDef, BitConverter.GetBytes(value));
            }
        }
        public float MagicAbsorb
        {
            get => _MagicAbsorb;
            set
            {
                _MagicAbsorb = value;
                WriteAt(indMagicAbsorb, BitConverter.GetBytes(value));
            }
        }
        public float LightningAbsorb
        {
            get => _LightningAbsorb;
            set
            {
                _LightningAbsorb = value;
                WriteAt(indLightningAbsorb, BitConverter.GetBytes(value));
            }
        }
        public float FireAbsorb
        {
            get => _FireAbsorb;
            set
            {
                _FireAbsorb = value;
                WriteAt(indFireAbsorb, BitConverter.GetBytes(value));
            }
        }
        public float DarkAbsorb
        {
            get => _DarkAbsorb;
            set
            {
                _DarkAbsorb = value;
                WriteAt(indDarkAbsorb, BitConverter.GetBytes(value));
            }
        }
        public float PoisonResist
        {
            get => _PoisonResist;
            set
            {
                _PoisonResist = value;
                WriteAt(indPoisonResist, BitConverter.GetBytes(value));
            }
        }
        public float BleedResist
        {
            get => _BleedResist;
            set
            {
                _BleedResist = value;
                WriteAt(indBleedResist, BitConverter.GetBytes(value));
            }
        }
        public float PetrifyResist
        {
            get => _PetrifyResist;
            set
            {
                _PetrifyResist = value;
                WriteAt(indPetrifyResist, BitConverter.GetBytes(value));
            }
        }
        public float CurseResist
        {
            get => _CurseResist;
            set
            {
                _CurseResist = value;
                WriteAt(indCurseResist, BitConverter.GetBytes(value));
            }
        }
        public int MaxReinforceLevel
        {
            get => _MaxReinforceLevel;
            set
            {
                _MaxReinforceLevel = value;
                WriteAt(indMaxReinforceLevel, BitConverter.GetBytes(value));
            }
        }
        public int UpgradeMaterial
        {
            get => _UpgradeMaterial;
            set
            {
                _UpgradeMaterial = value;
                WriteAt(indUpgradeMaterial, BitConverter.GetBytes(value));
            }
        }
        public int ReinforceCostID
        {
            get => _ReinforceCostID;
            set
            {
                _ReinforceCostID = value;
                WriteAt(indReinforceCostID, BitConverter.GetBytes(value));
            }
        }
        

        // Linked param:
        //internal ItemLotRow? ItemLot => ParamMan.GetLink<ItemLotRow>(ParamMan.ItemLotChrParam, ItemLotID);
        //internal GeneratorRegistRow? GeneratorRegist => ParamMan.GetGenRegistLink<GeneratorRegistRow>(GeneratorRegistID, Param.Name);

        // Constructor:
        public ArmorReinforceRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();

            // Initialise Values:
            SlashDef = (float)ReadAt(indSlashDef);
            ThrustDef = (float)ReadAt(indThrustDef);
            StrikeDef = (float)ReadAt(indStrikeDef);
            StandardDef = (float)ReadAt(indStandardDef);
            MagicAbsorb = (float)ReadAt(indMagicAbsorb);
            LightningAbsorb = (float)ReadAt(indLightningAbsorb);
            FireAbsorb = (float)ReadAt(indFireAbsorb);
            DarkAbsorb = (float)ReadAt(indDarkAbsorb);
            PoisonResist = (float)ReadAt(indPoisonResist);
            BleedResist = (float)ReadAt(indBleedResist);
            PetrifyResist = (float)ReadAt(indPetrifyResist);
            CurseResist = (float)ReadAt(indCurseResist);
            MaxReinforceLevel = (int)ReadAt(indMaxReinforceLevel);
            UpgradeMaterial = (int)ReadAt(indUpgradeMaterial);
            ReinforceCostID = (int)ReadAt(indReinforceCostID);
        }
        private void SetupIndices()
        {
            indSlashDef = GetFieldIndex("Slash Defence");
            indThrustDef = GetFieldIndex("Thrust Defence");
            indStrikeDef = GetFieldIndex("Strike Defence");
            indStandardDef = GetFieldIndex("Standard Defence");
            indMagicAbsorb = GetFieldIndex("Magic Absorption");
            indLightningAbsorb = GetFieldIndex("Lightning Absorption");
            indFireAbsorb = GetFieldIndex("Fire Absorption");
            indDarkAbsorb = GetFieldIndex("Dark Absorption");
            indPoisonResist = GetFieldIndex("Poison Resist");
            indBleedResist = GetFieldIndex("Bleed Resist");
            indPetrifyResist = GetFieldIndex("Petrify Resist");
            indCurseResist = GetFieldIndex("Curse Resist");
            indMaxReinforceLevel = GetFieldIndex("Max Reinforce Level");
            indUpgradeMaterial = GetFieldIndex("Upgrade Material");
            indReinforceCostID = GetFieldIndex("Reinforce Cost ID");
        }

    }
}
