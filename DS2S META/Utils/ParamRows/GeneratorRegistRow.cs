using DS2S_META.Randomizer;
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
    public class GeneratorRegistRow : Param.Row
    {
        // Behind-fields:
        private int indEnemyID { get; set; }
        public int _enemyID;
        
        // Properties:
        public int EnemyID
        {
            get => _enemyID;
            set
            {
                _enemyID = value;
                WriteAt(indEnemyID, BitConverter.GetBytes(value));
            }
        }

        // Linked param:
        internal ChrRow? Enemy => ParamMan.GetLink<ChrRow>(ParamMan.EnemyParam, EnemyID);

        // Constructor:
        public GeneratorRegistRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            SetupIndices();
            
            // Initialise Values:
            EnemyID = (int)ReadAt(indEnemyID);
        }
        private void SetupIndices()
        {
            indEnemyID = GetFieldIndex("EnemyParamID");
        }
    }
}
