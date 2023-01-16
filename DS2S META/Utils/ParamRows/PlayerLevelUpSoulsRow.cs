using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class PlayerLevelUpSoulsRow : Param.Row
    {
        // Behind-fields:
        private int indLevel { get; set; }
        private int indLevelCost { get; set; }
        public int _Level;
        public int _LevelCost;
        
        // Properties:
        public int Level
        {
            get => _Level;
            set
            {
                _Level = value;
                WriteAt(indLevel, BitConverter.GetBytes(value));
            }
        }
        public int LevelCost
        {
            get => _LevelCost;
            set
            {
                _LevelCost = value;
                WriteAt(indLevelCost, BitConverter.GetBytes(value));
            }
        }
        
        
        // Constructor:
        public PlayerLevelUpSoulsRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();

            // Initialise Values:
            Level = (int)ReadAt(indLevel);
            LevelCost = (int)ReadAt(indLevelCost);
        }
        private void SetupIndices()
        {
            indLevel = GetFieldIndex("Level");
            indLevelCost = GetFieldIndex("Level Up Cost");
        }

    }
}
