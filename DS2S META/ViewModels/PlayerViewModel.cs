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
        public bool EnWarp => MetaFeature.FtWarp;

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

        public bool EnWarpRest => Hook?.Hooked == true && !Properties.Settings.Default.AlwaysRestAfterWarp;  
        
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

        private bool _liveGravity = true;
        public bool LiveGravity
        {
            get => _liveGravity;
            set
            {
                _liveGravity = value;
                // only update in real time if we're not keeping nograv etc on throughout loads
                if (!Properties.Settings.Default.NoGravThroughLoads)
                {
                    ChkGravity = value;
                    OnPropertyChanged(nameof(ChkGravity));
                }
            }
        }
        private bool _liveCollision = true;
        public bool LiveCollision
        {
            get => _liveCollision;
            set
            {
                _liveCollision = value;
                // only update in real time if we're not keeping nograv etc on throughout loads
                if (!Properties.Settings.Default.NoGravThroughLoads)
                {
                    ChkCollision = value;
                    OnPropertyChanged(nameof(ChkCollision));
                }
            }
        }

        // Hotkey commands:
        public void ToggleGravity() => ChkGravity = !ChkGravity;
        public void ToggleCollision() => ChkCollision = !ChkCollision;
        //public void ToggleSpeed() => cbxSpeed.IsChecked = !cbxSpeed.IsChecked;
        
        public void ToggleAI() => ChkDisableAi = !ChkDisableAi;
        
        // TODO
        //public void ToggleRapierOhko()
        //{
        //    cbxRapierOHKO.IsChecked = !cbxRapierOHKO.IsChecked;
        //    cbxFistOHKO.IsChecked = !cbxFistOHKO.IsChecked;
        //}
        public void ToggleNoDeath() => ChkNoDeath = !ChkNoDeath;

        // Constructor
        public PlayerViewModel()
        {
            // initialize commands
            BtnWarpCommand = new RelayCommand(BtnWarpExecute, BtnWarpCanExecute);
            BtnUnlockBfsCommand = new RelayCommand(BtnUnlockBfsExecute, BtnUnlockBfsCanExec);
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            //EnableElements();
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
        //
        private bool BtnWarpCanExecute(object? parameter) => MetaFeature.FtWarp; 
        private bool BtnUnlockBfsCanExec(object? parameter) => Hook?.Hooked == true; 
        //
        private void BtnWarpExecute(object? parameter) => Warp();
        private void BtnUnlockBfsExecute(object? parameter) => Hook?.UnlockBonfires();


        // install special event handler for LiveGravity stuff
        internal void OnHookPropertyUpdate(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) return;
            var hook = sender as DS2SHook;

            switch (e.PropertyName)
            {
                case nameof(Hook.Gravity):
                    UpdateLiveGravity(hook?.Gravity);
                    break;

                case nameof(Hook.Collision):
                    UpdateLiveCollision(hook?.Collision);
                    break;
            }
        }
        private void UpdateLiveGravity(bool? grav) 
        {
            if (grav == null) return;
            LiveGravity = grav ?? true;
        }
        private void UpdateLiveCollision(bool? coll)
        {
            if (coll == null) return;
            LiveCollision = coll ?? true;
        }
        
        // Event based updates
        internal void OnHooked()
        {
            EnableElements();
            if (Hook == null) return;
            Hook.PropertyChanged += OnHookPropertyUpdate;
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
            OnPropertyChanged(nameof(EnWarpRest));
        }


        public void Warp()
        {
        //    if (VM.Hook == null)
        //        return;

        //    _ = ChangeColor(Brushes.DarkGray);
        //    if (VM.Hook.Multiplayer)
        //    {
        //        MessageBox.Show("Warning: Cannot warp while engaging in Multiplayer", "Multiplayer Warp Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    var bonfire = cmbBonfire.SelectedItem as DS2SBonfire;

        //    // Handle betwixt start warps:
        //    bool NoPrevBonfire = bonfire == null || bonfire.ID == 0 || bonfire.AreaID == 0;
        //    if (NoPrevBonfire)
        //    {
        //        int BETWIXTAREA = 167903232;
        //        ushort BETWIXTBFID = 2650;
        //        VM.Hook.LastBonfireAreaID = BETWIXTAREA;
        //        VM.Hook.Warp(BETWIXTBFID, true);
        //        return;
        //    }


        //    if (bonfire == null)
        //        throw new Exception("How do we get here intellisense??");

        //    VM.Hook.LastBonfireID = bonfire.ID;
        //    VM.Hook.LastBonfireAreaID = bonfire.AreaID;
        //    var warped = VM.Hook.Warp(bonfire.ID);
        //    if (warped && cbxWarpRest.IsChecked == true)
        //        WarpRest = true;
        }
    }
}
