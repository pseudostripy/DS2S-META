using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using DS2S_META.DataClassHelpers.Commands;
using static DS2S_META.DataClassHelpers.State;
using Xceed.Wpf.Toolkit;
using System.Threading;
using DS2S_META.Utils.Offsets.HookGroupObjects;
using DS2S_META.Utils;
using DS2S_META.Utils.DS2Hook;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class PlayerViewModel : ViewModelBase
    {
        // Shorthand to used structures
        private BonfiresHGO? BF => Hook?.DS2P?.BonfiresHGO;
        private PlayerStateHGO? PS => Hook?.DS2P?.PlayerState;
        private CameraHGO? Camera => Hook?.DS2P?.CameraHGO;
        private CoreGameState? Game => Hook?.DS2P?.CGS;

        // Binding Variables:
        public bool EnNoDeath => MetaFeature.FtNoDeath;
        public bool EnRapierOHKO => MetaFeature.FtRapierOHKO;
        public bool EnFistOHKO => MetaFeature.FtFistOHKO;
        public bool EnSpeedhack => MetaFeature.FtSpeedhack;
        public bool EnNoGravity => MetaFeature.FtNoGravity;
        public bool EnNoCollision => MetaFeature.FtNoCollision;
        public bool EnDisableAi => MetaFeature.FtDisableAi;
        public bool EnStorePosition => MetaFeature.FtStorePosition;
        public bool EnRestorePosition => MetaFeature.FtRestorePosition;
        public bool EnWarp => MetaFeature.FtWarp && !IsWarping;
        public bool EnManageBfs => MetaFeature.FtWarp && !IsWarping;
        public bool EnWarpRest => Hook?.Hooked == true && !Properties.Settings.Default.AlwaysRestAfterWarp;  
        public bool EnSpeedhackFactor => MetaFeature.FtSpeedhack && ChkSpeedhack; // allowed and enabled  
        public bool EnLockChoice => Hook?.Hooked == true;
        public bool EnDmgMod => MetaFeature.FtDmgMod;
        public bool EnMoneyBags => MetaFeature.FtGiveSouls;
        public bool EnRestoreHumanity => MetaFeature.FtRestoreHumanity;
        public bool EnNewTestCharacter => MetaFeature.FtNewTestCharacter;
        public bool EnDisablePoisonBuildup => MetaFeature.FtDisablePoisonBuildup;
        public bool EnDisableSkirtPoison => MetaFeature.FtDisableSkirtPoison;
        public bool EnInfiniteStamina => MetaFeature.FtInfiniteStamina;
        public bool EnInfiniteSpells => MetaFeature.FtInfiniteSpells;
        public bool EnDisablePartyWalkTimer => MetaFeature.FtDisablePartyWalkTimer;
        public bool EnInfiniteGoods => MetaFeature.FtInfiniteGoods;

        // Other properties
        private Visibility _lblSearchVisibility = Visibility.Visible;
        public Visibility LblSearchVisibility
        {
            get => _lblSearchVisibility;
            set
            {
                _lblSearchVisibility = value;
                OnPropertyChanged();
            }
        }
        private bool _chkNoDeath = false;
        public bool ChkNoDeath {
            get => _chkNoDeath;
            set {
                _chkNoDeath = value;
                Hook?.SetNoDeath(value);
                OnPropertyChanged(nameof(ChkNoDeath));
            }
        }

        private bool _chkInfiniteStamina = false;
        public bool ChkInfiniteStamina
        {
            get => _chkInfiniteStamina;
            set
            {
                _chkInfiniteStamina = value;
                Hook?.SetInfiniteStamina(value);
                OnPropertyChanged(nameof(ChkInfiniteStamina));
            }
        }

        private bool _chkDisableAi = false;
        public bool ChkDisableAi { 
            get => _chkDisableAi;
            set 
            {
                _chkDisableAi = value;
                // update game AI when we toggle checkbox but note that it can update itself too and desync from this flag (i.e. after load)
                Hook?.SetDisableAI(value); 
                OnPropertyChanged(nameof(ChkDisableAi));
            }
        }
        private bool ChkNoGravity => !ChkGravity;
        private bool _chkGravity = true;
        public bool ChkGravity
        {
            get => _chkGravity;
            set
            {
                _chkGravity = value;
                Hook?.SetNoGravity(!value);
                OnPropertyChanged(nameof(ChkGravity));
            }
        }
        private bool ChkNoCollision => !ChkCollision;
        private bool _chkCollision = true;
        public bool ChkCollision
        {
            get => _chkCollision;
            set
            {
                _chkCollision = value;
                Hook?.SetNoCollision(!value);
                OnPropertyChanged(nameof(_chkCollision));
            }
        }
        private bool _chkRapierOHKO = false;
        public bool ChkRapierOHKO
        {
            get => _chkRapierOHKO;
            set
            {
                _chkRapierOHKO = value;
                Hook?.SetRapierOHKO(value);
                OnPropertyChanged(nameof(ChkRapierOHKO));
            }
        }
        private bool _chkFistOHKO = false;
        public bool ChkFistOHKO
        {
            get => _chkFistOHKO;
            set
            {
                _chkFistOHKO = value;
                Hook?.SetFistOHKO(value);
                OnPropertyChanged(nameof(ChkFistOHKO));
            }
        }

        private bool _chkDisablePoisonBuildup = false;
        public bool ChkDisablePoisonBuildup
        {
            get => _chkDisablePoisonBuildup;
            set
            {
                _chkDisablePoisonBuildup = value;
                Hook?.SetDisablePoisonBuildup(value);
                OnPropertyChanged(nameof(ChkDisablePoisonBuildup));
            }
        }
        private bool _chkDisableSkirtPoison = false;
        public bool ChkDisableSkirtPoison
        {
            get => _chkDisableSkirtPoison;
            set
            {
                _chkDisableSkirtPoison = value;
                Hook?.SetDisableSkirtPoison(value);
                OnPropertyChanged(nameof(ChkDisableSkirtPoison));
            }
        }

        private bool _chkInfiniteSpells = false;
        public bool ChkInfiniteSpells
        {
            get => _chkInfiniteSpells;
            set
            {
                _chkInfiniteSpells = value;
                Hook?.SetInfiniteSpells(value);
                OnPropertyChanged(nameof(ChkInfiniteSpells));
            }
        }

        private bool _chkDisablePartyWalkTimer = false;
        public bool ChkDisablePartyWalkTimer
        {
           get => _chkDisablePartyWalkTimer;
           set
           {
                _chkDisablePartyWalkTimer = value;
                OnPropertyChanged(nameof(ChkDisablePartyWalkTimer));
           }
        }

        private bool _chkInfiniteGoods = false;
        public bool ChkInfiniteGoods
        {
            get => _chkInfiniteGoods;
            set
            {
                _chkInfiniteGoods = value;
                Hook?.SetInfiniteGoods(value);
                OnPropertyChanged(nameof(ChkInfiniteGoods));
            }
        }

        private bool _chkWarpRest = Properties.Settings.Default.RestAfterWarp; // load from previous opening
        public bool ChkWarpRest
        {
            get => _chkWarpRest;
            set
            {
                _chkWarpRest = value;
                Properties.Settings.Default.RestAfterWarp = value; // save preferences
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnWarpRest)); // possibly needs rechecking
            }
        }
        private bool _chkLockChoice = Properties.Settings.Default.LockBfChoice; // load from previous opening
        public bool ChkLockChoice
        {
            get => _chkLockChoice;
            set
            {
                _chkLockChoice = value;
                Properties.Settings.Default.LockBfChoice = value; // save preferences
                ResetAutoBfUpdate(); // reset to "auto"
                OnPropertyChanged();
            }
        }
        private bool _chkSpeedhack = false; // start off
        public bool ChkSpeedhack
        {
            get => _chkSpeedhack;
            set
            {
                _chkSpeedhack = value;
                Hook?.EnableSpeedhack(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnSpeedhackFactor));
            }
        }
        private decimal _speedHackFactor = Properties.Settings.Default.SpeedValue;
        public decimal SpeedhackFactor
        {
            get => _speedHackFactor;
            set
            {
                _speedHackFactor = value;
                Properties.Settings.Default.SpeedValue = value;
                SetGameSpeed();
                OnPropertyChanged();
            }
        }
        private bool _chkTogManageBfs = false;
        public bool ChkTogManageBfs 
        { 
            get => _chkTogManageBfs;
            set {
                _chkTogManageBfs = value;
                if (value)
                    SetManagedBonfireList();
                else
                    BonfireControlList = new(); // clear
                OnPropertyChanged();
            } 
        }

        // Status Properties [not entirely sure why these must have setters to work]
        public int? CurrentTypedHealth { get; set; }
        private int _healthCurr;
        public int HealthCurr
        {
            get => PS?.Health ?? 0;
            set
            {
                // There's a nasty race condition here because of the ClipToMinMax = true condition
                // in the View, which is bound to HealthCap, which will return 0 until initialized.
                // That clip then causes a HealthCurr SET value to (what is currently the max of 0),
                // killing the player :/. Not sure this is even a full fix tbh, it at least greatly
                // reduces the race condition active time...
                if (HealthCap == 0)
                    return;

                _healthCurr = value;
                if (PS?.Health == null || IsHealthTyping == true)
                    return;

                PS.Health = value;
                OnPropertyChanged();
            }
        }
        public int HealthCap
        {
            get => PS?.HealthCap ?? 0;
            set
            {
                if (PS?.HealthCap != null)
                    PS.HealthCap = value;
                OnPropertyChanged();
            }
        }
        public int HealthMax
        {
            get => PS?.HealthMax ?? 0;
            set
            {
                if (PS?.HealthMax != null)
                    PS.HealthMax = value;
                OnPropertyChanged();
            }
        }
        public int StaminaCurr
        {
            get => (int)(PS?.Stamina ?? 0);
            set
            {
                if (PS?.Stamina != null)
                    PS.Stamina = value;
                OnPropertyChanged();
                StaminaLock = false; // onLostFocus
            }
        }
        public int StaminaMax
        {
            get => (int)(PS?.MaxStamina ?? 0);
            set
            {
                if (PS?.MaxStamina != null)
                    PS.MaxStamina = value;
                OnPropertyChanged();
            }
        }


        public float PoiseCurr
        {
            get => PS?.CurrPoise ?? 0;
            set
            {
                if (PS != null)
                    PS.CurrPoise = value;
                OnPropertyChanged();
            }
        }

        private bool _chkRestoreState = false;
        public bool ChkRestoreState
        {
            get => _chkRestoreState;
            set
            {
                _chkRestoreState = value;
                OnPropertyChanged();
            }
        }
        private PlayerState? LastPlayerState = null;
        
        // Bonfire Combobox Property Management [StateHell]
        private bool IsWarping = false;
        private void ResetAutoBfUpdate()
        {
            // call on ChkLocked unticked and on DS2 loads for QoL
            if (ChkLockChoice || ActiveFilter) 
                return; // locked. leave it alone
            UserSelectedABonfireOrHub = false; // allow relief to auto
            BonfireList = AllBonfireList;
        }
        public DS2SBonfire? GameLastBonfire
        {
            get
            {
                var lastBonfireAreaId = BF?.LastBonfireAreaID;
                var lastBonfireId = BF?.LastBonfireID;
                if (lastBonfireAreaId == null || lastBonfireId == null)
                    return null;
                return DS2Resource.LookupBonfire((int)lastBonfireAreaId, (int)lastBonfireId);
            }
        }
        private bool autoHubUpdate = false;
        private bool simpleSetBf = false;
        private bool bfListUpdate = false;
        private bool bfseltrig = false; // required to break the inf recursion on doubly dependent events
        private DS2SBonfire? _selectedBf;
        public DS2SBonfire? SelectedBf
        {
            get
            {
                if (ChkLockChoice && UserSelectedABonfireOrHub)
                    return _selectedBf; // they chose something and want to use it
                if (ChkLockChoice && !UserSelectedABonfireOrHub)
                    return GameLastBonfire; // possible if only locked via saved pref.

                // Not ChkLocked
                if (UserSelectedABonfireOrHub) return _selectedBf; // recent selection

                // auto
                var autobf = GameLastBonfire;
                if (autobf?.Hub != SelectedBfHub)
                {
                    autoHubUpdate = true;
                    SelectedBfHub = autobf?.Hub;
                    autoHubUpdate = false;
                }
                return autobf;
            }
            set
            {
                if (bfListUpdate)
                    return;
                if (simpleSetBf)
                {
                    _selectedBf = value;
                    OnPropertyChanged();
                    return;
                }

                // actual user bonfire selection:
                _selectedBf = value;
                UserSelectedABonfireOrHub = true;
                bfseltrig = true;
                SelectedBfHub = value?.Hub;
                bfseltrig = false; // done
                OnPropertyChanged();
            }
        }
        private DS2SBonfireHub? _selectedBfHub = null;
        public DS2SBonfireHub? SelectedBfHub
        {
            get => _selectedBfHub;
            set
            {
                var origval = _selectedBfHub;
                _selectedBfHub = value;
                if (!autoHubUpdate)
                    UserSelectedABonfireOrHub = true;

                if (!autoHubUpdate && !bfseltrig)
                {
                    // Manual hub selection
                    // Update bonfire list but don't desync the SelectedBf Property inside an event
                    bfListUpdate = true;
                    if (_selectedBfHub?.Bonfires == null)
                        BonfireList = AllBonfireList; // reset to all options
                    else
                        BonfireList = new ObservableCollection<DS2SBonfire>(_selectedBfHub?.Bonfires!);
                    bfListUpdate = false;

                    simpleSetBf = true;
                    SelectedBf = BonfireList.FirstOrDefault(); // triggers only on user hub selection
                    simpleSetBf = false;
                }

                // Update managed bonfires
                if (ChkTogManageBfs && origval != _selectedBfHub)
                    SetManagedBonfireList();
            }
        }
        private bool UserSelectedABonfireOrHub = false;
        public static readonly ObservableCollection<DS2SBonfire> AllBonfireList = new(DS2Resource.Bonfires);
        private ObservableCollection<DS2SBonfire> _bonfireList = AllBonfireList;
        public ObservableCollection<DS2SBonfire> BonfireList {
            get => _bonfireList;
            set
            {
                _bonfireList = value;
                OnPropertyChanged();
            }
        }
        public static List<DS2SBonfireHub> BonfireHubList => DS2Resource.BonfireHubs;
        public void SetManagedBonfireList()
        {
            if (SelectedBfHub == null) return;
            if (Hook == null) return;
            BonfireControlList = SelectedBfHub.Bonfires.Select(bf => new BonfireControlData(Hook, bf)).ToList();
        }
        private bool ActiveFilter = false;

        // Utility Properties
        private bool _chkDealNoDmg = false;
        public bool ChkDealNoDmg
        {
            get => _chkDealNoDmg;
            set
            {
                // Success, update properties and FE
                _chkDealNoDmg = value;
                _chkOHKO = false; // overruled
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChkOHKO));
            }
        }
        private bool _chkOHKO = false;
        public bool ChkOHKO
        {
            get => _chkOHKO;
            set
            {
                _chkOHKO = value;
                _chkDealNoDmg = false; // overruled (OHKO and dealNoDmg are mutex)
                Hook?.GeneralizedDmgMod(_chkOHKO, _chkDealNoDmg, _chkTakeNoDmg);
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChkDealNoDmg));
            }
        }
        private bool _chkTakeNoDmg = false;
        public bool ChkTakeNoDmg
        {
            get => _chkTakeNoDmg;
            set
            {
                _chkTakeNoDmg = value;
                OnPropertyChanged();
            }
        }
        
        // Integer updown binding stuff:
        private static readonly float[] ZEROVECFLOAT = new float[3] { 0.0f, 0.0f, 0.0f };
        private float[] _ang = ZEROVECFLOAT;
        public float[] Ang
        {
            get => PS?.Ang ?? ZEROVECFLOAT;
            set
            {
                _ang = value;
                if (PS?.Ang != null)
                    PS.Ang = _ang;
                OnPropertyChanged();
            }
        }
        public float[] _currentPos = ZEROVECFLOAT;
        public float[] CurrentPos
        {
            get => PS?.Pos ?? ZEROVECFLOAT;
            set
            {
                _currentPos = value;
                if (PS?.Pos != null)
                    PS.Pos = _currentPos;
                OnPropertyChanged();
            }
        }
        public float[] _stablePos = ZEROVECFLOAT;
        public float[] StablePos
        {
            get => PS?.StablePos ?? ZEROVECFLOAT;
            set
            {
                _stablePos = value;
                if (PS?.StablePos != null)
                    PS.StablePos = _stablePos;
                OnPropertyChanged();
            }
        }
        private float[] _storedPos = ZEROVECFLOAT;
        public float[] StoredPos
        {
            get => PS?.StablePos ?? ZEROVECFLOAT;
            set
            {
                _storedPos = value;
                OnPropertyChanged();
            }
        }
    
        // Hook tasks:
        public void ToggleGravity() => ChkGravity = !ChkGravity;
        public void ToggleCollision() => ChkCollision = !ChkCollision;
        public void ToggleAI() => ChkDisableAi = !ChkDisableAi;
        public void ToggleSpeedhack() => ChkSpeedhack = !ChkSpeedhack;
        private void SetGameSpeed() => Hook?.SetSpeedhackSpeed((double)_speedHackFactor);
        public void ToggleNoDeath() => ChkNoDeath = !ChkNoDeath;
        public void ToggleOHKO() => ChkOHKO = !ChkOHKO;
        public void ToggleRapierOhko() => ChkRapierOHKO = !ChkRapierOHKO;
        public void ToggleFistOhko() => ChkFistOHKO = !ChkFistOHKO;

        // Programmatic ItemControl Management
        public class BonfireControlData : ViewModelBase
        {
            public string Name => Bonfire.Name;
            public readonly DS2SBonfire Bonfire;
            public int BfLevel
            {
                get => Hook?.DS2P.BonfiresHGO.GetBonfireLevelById(Bonfire.ID) ?? 0;
                set => Hook?.DS2P.BonfiresHGO.SetBonfireLevelById(Bonfire.ID, value);
            }
            public BonfireControlData(DS2SHook hook, DS2SBonfire bonfire)
            {
                Hook = hook;
                Bonfire = bonfire;
            }
            public override void UpdateViewModel()
            {
                OnPropertyChanged(nameof(BfLevel));
            }
        }
        private List<BonfireControlData> _bonfireControlList = new();
        public List<BonfireControlData> BonfireControlList
        {
            get => _bonfireControlList;
            set
            {
                _bonfireControlList = value;
                OnPropertyChanged();
            }
        }

        // Constructor
        public PlayerViewModel()
        {
            // initialize commands
            BtnWarpCommand = new RelayCommand(BtnWarpExecute, BtnWarpCanExec);
            BtnUnlockBfsCommand = new RelayCommand(BtnUnlockBfsExecute, BtnUnlockBfsCanExec);
            RestorePositionCommand = new RelayCommand(RestorePositionExecute, RestorePositionCanExec);
            BtnMoneybagsCommand = new RelayCommand(BtnMoneybagsExecute, BtnMoneybagsCanExec);
            BtnRestoreHumanityCommand = new RelayCommand(BtnRestoreHumanityExecute, BtnRestoreHumanityCanExec);
            BtnNewTestCharCommand = new RelayCommand(BtnNewTestCharExecute, BtnNewTestCharCanExec);
            HealthCurrLostFocusCommand = new RelayCommand(HealthCurrLostFocusExecute, AlwaysCanExec);
            StaminaCurrGotFocusCommand = new RelayCommand(StaminaCurrGotFocusExecute, AlwaysCanExec);
            StaminaPreviewKeyDownCommand = new RelayCommand(StaminaPreviewKeyDownExecute, AlwaysCanExec);
            HealthPreviewKeyDownCommand = new RelayCommand(HealthPreviewKeyDownExecute, AlwaysCanExec);
            StorePositionCommand = new RelayCommand(StorePositionExecute, StorePositionCanExec);
            BfSearchTextChangedCommand = new RelayCommand(BfSearchTextChangedCommandExecute, AlwaysCanExec);
        }

        // Command Definitions
        public ICommand BtnWarpCommand { get; set; }
        public ICommand BtnUnlockBfsCommand { get; set; }
        public ICommand StorePositionCommand { get; set; }
        public ICommand RestorePositionCommand { get; set; }
        public ICommand BtnMoneybagsCommand { get; set; }
        public ICommand BtnRestoreHumanityCommand { get; set; }
        public ICommand BtnNewTestCharCommand { get; set; }
        public ICommand HealthCurrLostFocusCommand { get; set; }
        public ICommand StaminaCurrGotFocusCommand { get; set; }
        public ICommand StaminaPreviewKeyDownCommand { get; set; }
        public ICommand HealthPreviewKeyDownCommand { get; set; }
        public ICommand BfSearchTextChangedCommand { get; set; }
        //
        private bool BtnWarpCanExec(object? parameter) => MetaFeature.FtWarp; 
        private bool BtnUnlockBfsCanExec(object? parameter) => Hook?.Hooked == true; 
        private bool StorePositionCanExec(object? parameter) => MetaFeature.FtStorePosition; 
        private bool RestorePositionCanExec(object? parameter) => Hook?.Hooked == true; 
        private bool BtnMoneybagsCanExec(object? parameter) => MetaFeature.FtGiveSouls; 
        private bool BtnRestoreHumanityCanExec(object? parameter) => MetaFeature.FtRestoreHumanity; 
        private bool BtnNewTestCharCanExec(object? parameter) => MetaFeature.FtNewTestCharacter; 
        private static bool AlwaysCanExec(object? parameter) => true;
        //
        private void BtnWarpExecute(object? parameter) => Warp();
        private void BtnUnlockBfsExecute(object? parameter) => Hook?.UnlockBonfires();
        private void StorePositionExecute(object? paramter)
        {
            if (Camera == null)
                return;
            var cam = new float[] {Camera.CamX, Camera.CamY, Camera.CamZ}; // unsure if implemented properly
            LastPlayerState = new PlayerState(HealthCurr, StaminaCurr, StablePos, Ang, cam);
            
            // update storedpos
            StoredPos = StablePos;
        }
        private void RestorePositionExecute(object? paramter)
        {
            if (Camera == null) return;
            if (LastPlayerState?.StablePos != null)
                StablePos = LastPlayerState.StablePos;
            if (LastPlayerState?.Ang != null)
                Ang = LastPlayerState.Ang;
            if (ChkRestoreState && LastPlayerState?.HP != null)
                HealthCurr = LastPlayerState.HP;
            if (ChkRestoreState && LastPlayerState?.Stamina != null)
                StaminaCurr = LastPlayerState.Stamina;

            if (LastPlayerState?.Cam != null)
            {
                Camera.CamX = LastPlayerState.Cam[0];
                Camera.CamY = LastPlayerState.Cam[1];
                Camera.CamZ = LastPlayerState.Cam[2];
            }
        }
        private void BtnMoneybagsExecute(object? parameter) => Hook?.AddSouls(9999999);
        private void BtnRestoreHumanityExecute(object? parameter) => Hook?.RestoreHumanity();
        private void BtnNewTestCharExecute(object? parameter) => Hook?.NewTestCharacter();
        private void HealthCurrLostFocusExecute(object? parameter)
        {
            IsHealthTyping = false;
            HealthCurr = _healthCurr;
        }
        private void StaminaCurrGotFocusExecute(object? parameter) => StaminaLock = true;
        
        // KeyDown Events:
        private void ResetStaminaCurr()
        {
            // reset to whatever the game thinks stamina is
            var hookstam = StaminaCurr;
            StaminaCurr = hookstam;
            Keyboard.ClearFocus();
            StaminaLock = false;
        }
        private void StaminaPreviewKeyDownExecute(object? evargs)
        {
            var keyargs = (KeyEventArgs?)evargs;
            if (keyargs?.Key == Key.Escape)
            {
                ResetStaminaCurr();
                return;
            }

            // Handle userIsTyping
            if (keyargs?.Key != Key.Enter)
            {
                StaminaLock = true;
                return; 
            }

            // Bad event origin
            if (keyargs.OriginalSource is not WatermarkTextBox textbox) return;

            // Handle Enter press
            if (textbox.Text != string.Empty)
                StaminaCurr = int.Parse(textbox.Text);

            // Done
            ResetStaminaCurr();
        }
        private void HealthPreviewKeyDownExecute(object? evargs)
        {
            var keyargs = (KeyEventArgs?)evargs;
            if (keyargs?.Key == Key.Escape)
            {
                var hookhealth = HealthCurr;
                HealthCurr = hookhealth; // reset to ds2 game value
                Keyboard.ClearFocus(); // this triggers a value update (blocked by below line)
                IsHealthTyping = false; // allow updates as usual again
                return;
            }

            if (keyargs?.Key != Key.Enter)
            {
                IsHealthTyping = true;
                return;
            }

            IsHealthTyping = false;
            HealthCurr = _healthCurr;
            Keyboard.ClearFocus();
        }

        private void BfSearchTextChangedCommandExecute(object? parameter)
        {
            var txtchangedevargs = (TextChangedEventArgs?)parameter;
            
            if (txtchangedevargs?.Source is not TextBox tbx) return;
            LblSearchVisibility = tbx.Text == string.Empty ? Visibility.Visible : Visibility.Hidden;
            ActiveFilter = tbx.Text != string.Empty;
            FilterBonfiresExecute(tbx.Text);
        }
        private void SelectFirstBonfireInList()
        {
            // wrapper to handle awkward state events
            simpleSetBf = true;
            if (BonfireList.Count == 0)
                SelectedBf = null;
            else    
                SelectedBf = BonfireList.FirstOrDefault(); // triggers only on user hub selection
            simpleSetBf = false;
        }
        private void FilterBonfiresExecute(string txt)
        {
            // when user clears their own text
            if (txt == string.Empty)
            {
                BonfireList = AllBonfireList;
                return;
            }

            var newbfs = DS2Resource.Bonfires.Where(bf => bf.Name.Contains(txt, StringComparison.OrdinalIgnoreCase)).ToList();
            BonfireList = new ObservableCollection<DS2SBonfire>(newbfs);
            SelectFirstBonfireInList();

            // no filter match
            if (SelectedBf == null)
            {
                autoHubUpdate = true;
                SelectedBfHub = null;
                autoHubUpdate = false;
                OnPropertyChanged(nameof(SelectedBfHub));
                return;
            }

            // Update to first selection
            autoHubUpdate = true;
            SelectedBfHub = SelectedBf.Hub;
            autoHubUpdate = false;
            return;
        }

        // used to allow user to type without updating things
        public bool IsHealthTyping { get; set; } = false;
        public bool StaminaLock { get; set; } = false;

        // Misc.
        public void DeltaHeight(float delta)
        {
            if (PS == null) return;
            PS.PosZ += delta;

            // QOL: AutoTurn off gravity
            ChkGravity = false;
        }
        public void FastQuit()
        {
            if (Hook?.Hooked != true)
                return;
            Hook.DS2P.CGS.FastQuit();
        }
        public void Warp()
        {
            // no idea if multiplayer hook even implemented properly
            if (Game?.Multiplayer == true)
            {
                MetaExceptionStaticHandler.RaiseUserWarning("Cannot warp while engaging in Multiplayer");
                return;
            }

            // Final checks
            var bfChosen = SelectedBf; // race condition avoidance
            SelectedBf = bfChosen; // just in case its still null
            if (bfChosen == null)
                return;

            // Do Warp
            // [neat fix for GameLastBonf undef after warp]
            UserSelectedABonfireOrHub = true; // keep the selection after warp
            var ww = bfChosen.Name == "_Game Start";
            Hook?.WarpBonfire(bfChosen, ww, ChkWarpRest);

            // save warp location to fix an interesting meme due to the thread area overwrite
            IsWarping = true; // fixed on next load screen
            OnPropertyChanged(nameof(EnWarp)); // disable until next load
        }

        // Event based updates
        public override void OnHooked()
        {
            EnableElements();
        }
        public override void OnUnHooked()
        {
            EnableElements();
        }
        public void OnInGame()
        {
            // called upon transition from load-screen or main-menu to in-game
            if (Hook == null) return;

            // things that need to be reset on load:
            Hook?.SetNoDeath(ChkNoDeath);
            Hook?.SetDisableAI(ChkDisableAi);
            Hook?.SetInfiniteStamina(ChkInfiniteStamina);
            Hook?.SetInfiniteSpells(ChkInfiniteSpells);
            Hook?.SetInfiniteGoods(ChkInfiniteGoods);
            
            if (Properties.Settings.Default.NoGravThroughLoads)
            {
                Hook?.SetNoGravity(ChkNoGravity);
                Hook?.SetNoCollision(ChkNoCollision);
            }

            // fix post-warp
            if (IsWarping)
                IsWarping = false; // complete. reenable warps
            else
                ResetAutoBfUpdate();
            EnableElements(); // refresh UI element enables
        }
        internal void OnMainMenu()
        {
            EnableElements(); // disable stuff that requires InGame
        }
        private void EnableElements() 
        {
            OnPropertyChanged(nameof(EnNoDeath));
            OnPropertyChanged(nameof(EnRapierOHKO));
            OnPropertyChanged(nameof(EnFistOHKO));
            OnPropertyChanged(nameof(EnSpeedhack));
            OnPropertyChanged(nameof(EnNoGravity));
            OnPropertyChanged(nameof(EnNoCollision));
            OnPropertyChanged(nameof(EnDisableAi));
            OnPropertyChanged(nameof(EnStorePosition));
            OnPropertyChanged(nameof(EnRestorePosition));
            OnPropertyChanged(nameof(EnWarp));
            OnPropertyChanged(nameof(EnManageBfs));
            OnPropertyChanged(nameof(EnWarpRest));
            OnPropertyChanged(nameof(EnLockChoice));
            OnPropertyChanged(nameof(EnMoneyBags));
            OnPropertyChanged(nameof(EnRestoreHumanity));
            OnPropertyChanged(nameof(EnNewTestCharacter));
            OnPropertyChanged(nameof(EnDisablePoisonBuildup));
            OnPropertyChanged(nameof(EnDisableSkirtPoison));
            OnPropertyChanged(nameof(EnInfiniteStamina));
            OnPropertyChanged(nameof(EnInfiniteSpells));
            OnPropertyChanged(nameof(EnDisablePartyWalkTimer));
            OnPropertyChanged(nameof(EnInfiniteGoods));
        }
        public override void UpdateViewModel()
        {
            //Update(called on mainwindow update interval)
            if (!IsHealthTyping)
                OnPropertyChanged(nameof(HealthCurr));
            OnPropertyChanged(nameof(HealthMax));
            OnPropertyChanged(nameof(HealthCap));
            if (!StaminaLock)
                OnPropertyChanged(nameof(StaminaCurr));
            OnPropertyChanged(nameof(StaminaMax));
            OnPropertyChanged(nameof(StaminaMax));
            OnPropertyChanged(nameof(PoiseCurr));
            OnPropertyChanged(nameof(StablePos));
            OnPropertyChanged(nameof(CurrentPos));
            OnPropertyChanged(nameof(Ang));
            OnPropertyChanged(nameof(ChkCollision));
            OnPropertyChanged(nameof(ChkGravity));
            OnPropertyChanged(nameof(GameLastBonfire));
            OnPropertyChanged(nameof(EnDmgMod));
        }
        public override void DoSlowUpdates()
        {
            // put things here if less concerned about fastest updates
            OnPropertyChanged(nameof(SelectedBf));
            OnPropertyChanged(nameof(SelectedBfHub));
            BonfireLevelSync();
            if (ChkDisablePartyWalkTimer)
                Hook?.ResetPartyWalkTimer();
        }
        
        private void BonfireLevelSync()
        {
            // Refresh the whole list
            foreach (var bfcl in BonfireControlList)
                bfcl.UpdateViewModel();
        }
        public override void CleanupVM()
        {
            Hook?.SetNoDeath(false);
            Hook?.SetDisableAI(false);
            Hook?.SetNoGravity(false);
            Hook?.SetNoCollision(false);
            Hook?.SetRapierOHKO(false);
            Hook?.SetFistOHKO(false);
        }
    }
}
