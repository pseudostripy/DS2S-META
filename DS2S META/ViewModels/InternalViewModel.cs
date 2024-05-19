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
using System.Diagnostics.Metrics;
using Xceed.Wpf.Toolkit;
using DS2S_META.Utils.Offsets.HookGroupObjects;

namespace DS2S_META.ViewModels
{
    // Note: CheatsControl has CheatsViewModel data context set in MainWindow.xaml
    public class InternalViewModel : ViewModelBase
    {
        // Constants
        private const int EXTRAROWMINHEIGHT = 25;
        private const int EXTRAROWMAXHEIGHT = 30;
        
        // Enables:
        public bool EnCovCombo => MetaFeature.FtCovenantInfo;

        // Commands:
        public ICommand SetCovenantCommand { get; set; }
        private bool CanExecInGame(object? parameter) => Hook?.InGame == true;
        private void SetCovenantExecute(object? parameter)
        {
            CovDiscovered = true;
            SetCurrentCovenant(SelCovId);
        } 
        private void SetCurrentCovenant(COV id)
        {
            if (Hook == null)
                return;
            Hook.CurrentCovenant = (byte)id;
        }

        // Utility:
        private CovenantHGO? CovHook => Hook?.DS2P?.CovenantHGO;
        private COV SelCovId => CovSelected.ID;
        private bool ShowExtraRows => SelCovId != COV.NONE;
        public int CovExtraRowMinHeight => ShowExtraRows ? EXTRAROWMINHEIGHT : 0;
        public int RowMaxH => EXTRAROWMAXHEIGHT;
        public Visibility CovExtraRowVis => ShowExtraRows ? Visibility.Visible : Visibility.Hidden;
        public string CurrentCovenantName
        {
            get
            {
                var currCovId = Hook?.CurrentCovenant;
                if (currCovId == null)
                    return string.Empty;
                return DS2Resource.GetCovById((COV)currCovId).Name;
            }
        }
        public string SelCovDiscovString => $"{SelCovName} Discovered";
        public string SelCovRankString => $"{SelCovName} Rank";
        public string SelCovProgressString => $"{SelCovName} Progress";
        
        private Covenant? SelCovData => GetCovData();
        private Covenant? GetCovData()
        {
            // convert to enum
            if (CovHook?.GameCovenantData.TryGetValue(SelCovId, out var covenant) == true)
                return covenant;
            return null;
        }
        private string SelCovName => DS2Resource.GetCovById(SelCovId).Name;


        // Binding Properties:
        public static ObservableCollection<DS2SCovenant> CovenantsList => new(DS2Resource.Covenants);
        public DS2SCovenant CovSelected
        {
            get => _covSelected;
            set
            {
                _covSelected = value;
                OnPropertyChanged();
                UpdateCovData();
            }
        }
        private DS2SCovenant _covSelected = DS2Resource.Covenants.First(); // default NONE
        public bool CovDiscovered
        {
            get => SelCovData?.Discovered == true;
            set
            {
                if (SelCovData == null) return;
                CovHook?.SetCovenantDiscov(SelCovId, value);
                if (value == false && Hook != null && (COV)Hook.CurrentCovenant == SelCovId)
                    SetCurrentCovenant(COV.NONE);

                OnPropertyChanged();
            }
        }
        public int CovRank
        {
            get => SelCovData?.Rank ?? 0;
            set
            {
                if (SelCovData == null) return;
                FixProgressOnRankChange(value);
                CovHook?.SetCovenantRank(SelCovId, value);
                OnPropertyChanged();
            }
        }
        public int CovProgress
        {
            get => SelCovData?.Progress ?? 0;
            set
            {
                if (SelCovData == null) return;
                FixRankOnProgressChange(value);
                CovHook?.SetCovenantProgress(SelCovId, value);
                OnPropertyChanged();
            }
        }

        private void FixRankOnProgressChange(int newprog)
        {
            var rankLvls = DS2Resource.GetCovById(SelCovId).RankLevels.Values.ToList();
            var newrankSearch = rankLvls.FindIndex(lvl => newprog < lvl);
            int newrank;
            if (newrankSearch < 0)
                newrank = 3;
            else
                newrank = newrankSearch - 1;
            CovHook?.SetCovenantRank(SelCovId, newrank);
        }
        private void FixProgressOnRankChange(int newrank)
        {
            // fix progress:
            var lvls = DS2Resource.GetCovById(SelCovId).RankLevels;
            var currProg = CovProgress;
            var newProg = currProg;
            var rankLB = lvls[newrank];
            var rankUB = newrank < 3 ? lvls[newrank + 1] -1 : 999;
            if (currProg > rankUB || currProg < rankLB)
                newProg = rankLB;
            CovProgress = newProg;
        }

        private void UpdateCovData()
        {
            OnPropertyChanged(nameof(CovExtraRowMinHeight));
            OnPropertyChanged(nameof(CovExtraRowVis));
            OnPropertyChanged(nameof(CovDiscovered));
            OnPropertyChanged(nameof(CovRank));
            OnPropertyChanged(nameof(CovProgress));
            OnPropertyChanged(nameof(SelCovDiscovString));
            OnPropertyChanged(nameof(SelCovRankString));
            OnPropertyChanged(nameof(SelCovProgressString));
        }


        // Constructor
        public InternalViewModel()
        {
            SetCovenantCommand = new RelayCommand(SetCovenantExecute, CanExecInGame);
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
        internal void OnInGame()
        {
            EnableElements(); // refresh UI element enables
        }
        internal void OnMainMenu()
        {
            EnableElements(); // disable stuff that requires InGame
        }
        private void EnableElements() 
        {
            OnPropertyChanged(nameof(EnCovCombo));
        }
        public override void UpdateViewModel()
        {
            // Update (called on mainwindow update interval)
            OnPropertyChanged(nameof(CurrentCovenantName));
            OnPropertyChanged(nameof(CovDiscovered));
            OnPropertyChanged(nameof(CovRank));
            OnPropertyChanged(nameof(CovProgress));
        }
        public override void DoSlowUpdates()
        {
            // put things here if less concerned about fastest updates
        }
        public override void CleanupVM()
        {
        }
    }
}
