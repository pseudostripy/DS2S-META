using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    /// <summary>
    /// This is the master list of META functionality interfaces with 
    /// DS2. Is it subclassed by all versions to override the offsets
    /// as appropriate
    /// </summary>
    public abstract class DS2HookOffsets
    {
        // Misc/Global
        public const int UNSET = -1;
        public const string AOB_UNSET = "AB CD EF FF FF FF FF BA DC FE"; // some unlikely string I guess.

        // BaseA
        public string? BaseAAob;
        public string? BaseABabyJumpAoB;
        public int? BasePtrOffset1;
        public int? BasePtrOffset2;

        // Section 1:
        public PlayerCtrlOffsets PlayerCtrl { get; init; }
        public PlayerName PlayerName { get; init; }
        public ForceQuit ForceQuit { get; init; }
        //public DisableAI
        public PlayerBaseMisc PlayerBaseMisc { get; init; }
        public PlayerEquipment PlayerEquipment { get; init; }
        public PlayerParam PlayerParam { get; init; }
        public Attributes Attributes { get; init; }
        public Covenants Covenants { get; init; }
        public Gravity Gravity { get; init; } 
        public PlayerMapData PlayerMapData { get; init; }
        public Bonfire Bonfire { get; init; }
        public BonfireLevels BonfireLevels { get; init; }
        public Connection Connection { get; init; }
        public Camera Camera { get; init; }
        public Core? Core { get; init; }
        public Func? Func { get; set; }
        
        // Towards better version-specific functionality:
        public int[]? DisableAI;
        public int[]? LoadedEnemiesTable;

        public PlayerType PlayerType = new() 
        {
            ChrNetworkPhantomId = 0x3C,
            TeamType = 0x3D, 
            CharType = 0x48, 
        };
        public NetSvrBloodstainManager NetSvrBloodstainManager = new(0x38, 0x3C, 0x40);
        public PlayerPosition PlayerPosition = new(0x20, 0x24, 0x28, 0x34, 0x38, 0x3C);

        // Misc
        public int[]? PlayerStatsOffsets;
        public int[]? LoadingState;
        public int[]? BIKP1Skip_Val1;
        public int[]? BIKP1Skip_Val2;
        public int GameState;
    }
}
