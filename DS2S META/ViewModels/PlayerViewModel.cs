using DS2S_META.Utils;
using DS2S_META.ViewModels.Commands;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DS2S_META.Randomizer;
using System.Diagnostics;
using System.Windows.Controls;
using DS2S_META.Commands;
using static DS2S_META.State;
using DS2S_META.Utils.Offsets;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class PlayerViewModel : ViewModelBase
    {
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

        // Other properties
        private bool IsWarping = false;
        private bool _chkNoDeath = false;
        public bool ChkNoDeath {
            get => _chkNoDeath;
            set {
                _chkNoDeath = value;
                Hook?.SetNoDeath(value);
                OnPropertyChanged(nameof(ChkNoDeath));
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
                Hook?.Speedhack(value);
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
                UpdateBfMan(value);
                OnPropertyChanged();
            } 
        }
        
        // Bonfire Combobox Property Management
        private void ResetAutoBfUpdate()
        {
            // call on ChkLocked unticked and on DS2 loads for QoL
            if (ChkLockChoice) 
                return; // locked. leave it alone
            UserSelectedABonfireOrHub = false; // allow relief to auto
            BonfireList = AllBonfireList;
        }
        public DS2SBonfire? GameLastBonfire
        {
            get
            {
                var lastBonfireAreaId = Hook?.LastBonfireAreaID;
                var lastBonfireId = Hook?.LastBonfireID;
                if (lastBonfireAreaId == null || lastBonfireId == null)
                    return null;
                return DS2Resource.LookupBonfire((int)lastBonfireAreaId, (int)lastBonfireId);
            }
        }
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
                return GameLastBonfire; // auto
            }
            set
            {
                _selectedBf = value;
                UserSelectedABonfireOrHub = true;
                OnPropertyChanged();
            }
        }
        private DS2SBonfireHub? _selectedBfHub = null;
        public DS2SBonfireHub? SelectedBfHub
        {
            get => SelectedBf?.Hub;
            set
            {
                _selectedBfHub = value;
                UserSelectedABonfireOrHub = true;
                if (_selectedBfHub?.Bonfires == null)
                    BonfireList = AllBonfireList; // reset to all options
                else
                    BonfireList = new ObservableCollection<DS2SBonfire>(_selectedBfHub?.Bonfires!);

                //var testbf = DS2Resource.Bonfires[8];
                //var bonfireControl = new LabelNudControl();
                ////Binding binding = new Binding("Value")
                ////{
                ////    Source = Hook,
                ////    Path = new PropertyPath(bonfire.Replace(" ", "").Replace("'", "").Replace("(", "").Replace(")", ""))
                ////};
                ////bonfireControl.nudValue.SetBinding(Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty, binding);
                //bonfireControl.nudValue.Minimum = 0;
                //bonfireControl.nudValue.Maximum = 99;
                //bonfireControl.Label = testbf.Name;
                //bonfireControl.nudValue.Margin = new Thickness(0, 5, 0, 0);
                ////spBonfires.Children.Add(bonfireControl);
                //MyNewStuffTest.Add(bonfireControl);
                //List<string> testlist = new() { "hello", "world" };
                //MyNewStuffTest = new() { "hello", "world" };
                //OnPropertyChanged(nameof(MyNewStuffTest));
                //OnPropertyChanged(nameof(BonfireList));
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

        
        public List<string> MyNewStuffTest { get; set; }

        // Integer updown binding stuff:
        private readonly float[] ZEROVECFLOAT = new float[3] { 0.0f, 0.0f, 0.0f };
        public float[] Ang => Hook?.Ang ?? ZEROVECFLOAT;
        public float[] Pos => Hook?.Pos ?? ZEROVECFLOAT;
        public float[] StablePos => Hook?.StablePos ?? ZEROVECFLOAT;
        public float[] StoredPos => Hook?.StablePos ?? ZEROVECFLOAT;
    
        // Hook tasks:
        public void ToggleGravity() => ChkGravity = !ChkGravity;
        public void ToggleCollision() => ChkCollision = !ChkCollision;
        public void ToggleAI() => ChkDisableAi = !ChkDisableAi;
        public void ToggleSpeedhack() => ChkSpeedhack = !ChkSpeedhack;
        private void SetGameSpeed() => Hook?.SetSpeed((double)_speedHackFactor);
        public void ToggleNoDeath() => ChkNoDeath = !ChkNoDeath;
        //public void ToggleOhko() // TODO

        public void ToggleRapierOhko() => ChkRapierOHKO = !ChkRapierOHKO;
        public void ToggleFistOhko() => ChkFistOHKO = !ChkFistOHKO;


        private void UpdateBfMan(bool enable) { } // todo

        // Constructor
        public PlayerViewModel()
        {
            // initialize commands
            BtnWarpCommand = new RelayCommand(BtnWarpExecute, BtnWarpCanExecute);
            BtnUnlockBfsCommand = new RelayCommand(BtnUnlockBfsExecute, BtnUnlockBfsCanExec);
            BtnRestorePositionCommand = new RelayCommand(BtnRestorePositionExecute, BtnRestorePositionCanExec);
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            //OnPropertyChanged(nameof(Health));
            //OnPropertyChanged(nameof(HealthMax));
            //OnPropertyChanged(nameof(HealthCap));
            //OnPropertyChanged(nameof(Stamina));
            //OnPropertyChanged(nameof(MaxStamina));
            //OnPropertyChanged(nameof(TeamType));
            //OnPropertyChanged(nameof(CharType));
            OnPropertyChanged(nameof(Pos));
            OnPropertyChanged(nameof(StablePos));
            OnPropertyChanged(nameof(Ang));
            OnPropertyChanged(nameof(ChkCollision));
            OnPropertyChanged(nameof(ChkGravity));
            OnPropertyChanged(nameof(GameLastBonfire));
;       }
        public override void DoSlowUpdates()
        {
            // put things here if performance issues
            OnPropertyChanged(nameof(SelectedBf));
            OnPropertyChanged(nameof(SelectedBfHub));
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
        

        // Command Definitions
        public ICommand BtnWarpCommand { get; set; }
        public ICommand BtnUnlockBfsCommand { get; set; }
        public ICommand BtnRestorePositionCommand { get; set; }
        //
        private bool BtnWarpCanExecute(object? parameter) => MetaFeature.FtWarp; 
        private bool BtnUnlockBfsCanExec(object? parameter) => Hook?.Hooked == true; 
        private bool BtnRestorePositionCanExec(object? parameter) => Hook?.Hooked == true; 
        //
        private void BtnWarpExecute(object? parameter) => Warp();
        private void BtnUnlockBfsExecute(object? parameter) => Hook?.UnlockBonfires();
        public void BtnRestorePositionExecute(object? paramter)
        {
            //if (Hook == null) return;

            //if (!nudPosStoredX.Value.HasValue || !nudPosStoredY.Value.HasValue || !nudPosStoredZ.Value.HasValue)
            //    return;

            

            //VM.Hook.StableX = (float)nudPosStoredX.Value;
            //VM.Hook.StableY = (float)nudPosStoredY.Value;
            //VM.Hook.StableZ = (float)nudPosStoredZ.Value;
            //VM.Hook.AngX = PlayerState.AngX;
            //VM.Hook.AngY = PlayerState.AngY;
            //VM.Hook.AngZ = PlayerState.AngZ;
            ////Hook.CameraData = PlayerState.FollowCam;
            ////Hook.CamX = CamX;
            ////Hook.CamY = CamY;
            ////Hook.CamZ = CamZ;
            //if (cbxRestoreState.IsChecked == true)
            //{
            //    nudHealth.Value = PlayerState.HP;
            //    nudStamina.Value = PlayerState.Stamina;
            //}
        }

        // Event based updates
        internal void OnHooked()
        {
            EnableElements();
        }
        internal void OnUnHooked()
        {
            EnableElements();
        }
        internal void OnInGame()
        {
            // called upon transition from load-screen or main-menu to in-game
            if (Hook == null) return;
            EnableElements(); // refresh UI element enables

            // things that need to be reset on load:
            Hook?.SetNoDeath(ChkNoDeath);
            Hook?.SetDisableAI(ChkDisableAi);
            
            if (Properties.Settings.Default.NoGravThroughLoads)
            {
                Hook?.SetNoGravity(ChkNoGravity);
                Hook?.SetNoCollision(ChkNoCollision);
            }
            IsWarping = false;
            ResetAutoBfUpdate();
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
        }

        public void Warp() // todo
        {
            //IsWarping = true;
            OnPropertyChanged(nameof(EnWarp));

            //testing();
            MetaException.RaiseUserWarning($"current bonfire: {GameLastBonfire}");

            // no idea if multiplayer hook even implemented properly
            if (Hook?.Multiplayer == true)
            {
                MetaException.RaiseUserWarning("Cannot warp while engaging in Multiplayer");
                return;
            }

            //var bonfire = cmbBonfire.SelectedItem as DS2SBonfire;

            //// Handle betwixt start warps:
            //bool NoPrevBonfire = bonfire == null || bonfire.ID == 0 || bonfire.AreaID == 0;
            //if (NoPrevBonfire)
            //{
            //    int BETWIXTAREA = 167903232;
            //    ushort BETWIXTBFID = 2650;
            //    VM.Hook.LastBonfireAreaID = BETWIXTAREA;
            //    VM.Hook.Warp(BETWIXTBFID, true);
            //    return;
            //}


            //if (bonfire == null)
            //    throw new Exception("How do we get here intellisense??");

            //VM.Hook.LastBonfireID = bonfire.ID;
            //VM.Hook.LastBonfireAreaID = bonfire.AreaID;
            //var warped = VM.Hook.Warp(bonfire.ID);
            //if (warped && cbxWarpRest.IsChecked == true)
            //    WarpRest = true;
        }


        //public void RemoveSavedPos()
        //{
        //    //if (Positions.Any(n => n.Name == cmbStoredPositions.Text))
        //    //{
        //    //    //if (MessageBox.Show("Are you sure you want to delete this positon?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        //    //    //{
        //    //    //    var old = Positions.Single(n => n.Name == cmbStoredPositions.Text);
        //    //    //    Positions.Remove(old);
        //    //    //    cmbStoredPositions.SelectedIndex = 0;
        //    //    //    UpdatePositions();
        //    //    //    SavedPos.Save(Positions);
        //    //    //}

        //    //}
        //}
    }
}
