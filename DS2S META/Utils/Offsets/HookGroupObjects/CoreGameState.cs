using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.OffsetClasses;
using mrousavy;
using PropertyHook;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class CoreGameState : HGO
    {
        // Direct Hook properties
        public bool DisableAI
        {
            get
            {
                var isAIdisabled = PHDisableAI?.ReadByte() ?? 0;
                return isAIdisabled == 1;
            }
            set
            {
                if (!InGame) return;
                PHDisableAI?.WriteByte(value.AsByte());
                OnPropertyChanged();
            }
        }
        private bool _collision;
        private readonly List<byte?> _noCollisionStates = new() { 18, 19 };
        public bool Collision
        {
            get => _noCollisionStates.Contains(PHNetworkPhantomID?.ReadByte());
            set
            {
                if (!InGame) return;
                var phantomId = (byte)(value ? 0 : 18);
                PHNetworkPhantomID?.WriteByte(phantomId);
                OnPropertyChanged();
            }
        }
        private void SetNoGravity(bool noGravity) => PHNoGravity?.WriteBoolean(noGravity);
        private bool ActiveGravity() => PHNoGravity?.ReadBoolean() == false; // "nograv = false"
        public bool Gravity
        {
            get => InGame && ActiveGravity();
            set
            {
                SetNoGravity(!value);
                OnPropertyChanged();
            }
        }
        public bool LoadingState => PHLoadingState?.ReadBoolean() ?? false;
        private int _gamestate;
        public int GameState
        {
            get => _gamestate;
            set
            {
                if (value == _gamestate) return;
                var oldstate = _gamestate;
                _gamestate = value; // needs to be read during event
                Hook.RaiseGameStateChange(oldstate, value);
            }
        }
        public bool Online => ConnectionType > 0;
        public int ConnectionType => PHConnectionType?.ReadInt32() ?? 0;


        // utility shorthand wrappers
        public bool InGame => GameState == (int)GAMESTATE.LOADEDINGAME;
        public bool InMainMenu => GameState == (int)GAMESTATE.MAINMENU;
        public bool IsLoading => LoadingState;
        private const byte FORCEQUIT = 6;
        public void FastQuit() => PHForceQuit?.WriteByte(FORCEQUIT);
        public bool Multiplayer => !InGame || ConnectionType > 1;

        // REDataDefinitionInterfaces
        private PHLeaf? PHGameState;
        private PHLeaf? PHNoGravity;
        private PHLeaf? PHLoadingState;
        private PHLeaf? PHForceQuit;
        private PHLeaf? PHDisableAI;
        private PHLeaf? PHNetworkPhantomID;
        private PHLeaf? PHConnectionType;

        public CoreGameState(DS2SHook hook, Dictionary<string, PHLeaf?> leafdict) : base(hook)
        {
            Hook = hook;
            PHGameState = leafdict["GameState"];
            PHLoadingState = leafdict["LoadingState"];
            PHForceQuit = leafdict["ForceQuit"];
            PHDisableAI = leafdict["DisableAI"];
            PHNoGravity = leafdict["Gravity"];
            PHNetworkPhantomID = leafdict["NetworkPhantomID"];
            PHConnectionType = leafdict["ConnectionType"];
        }

        public override void UpdateProperties()
        {
            RefreshGameState();
            OnPropertyChanged(nameof(LoadingState));
            OnPropertyChanged(nameof(InGame));
            OnPropertyChanged(nameof(InMainMenu));
            OnPropertyChanged(nameof(Gravity));
            OnPropertyChanged(nameof(Collision));
            OnPropertyChanged(nameof(Online));
        }
        private void RefreshGameState()
        {
            GameState = PHGameState?.ReadInt32() ?? -1;
        }
    }
}
