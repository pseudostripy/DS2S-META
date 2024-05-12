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
        private COV SelCovId => (COV) CovSelected.ID;
        private bool ShowExtraRows => SelCovId != COV.NONE;
        public int CovExtraRowMinHeight => ShowExtraRows ? EXTRAROWMINHEIGHT : 0;
        public int RowMaxH => EXTRAROWMAXHEIGHT;
        public Visibility CovExtraRowVis => ShowExtraRows ? Visibility.Visible : Visibility.Hidden;
        public string CurrentCovenant => Hook?.CurrentCovenantName ?? string.Empty;
        public string SelCovDiscovString => $"{SelCovName} Discovered";
        public string SelCovRankString => $"{SelCovName} Rank";
        public string SelCovProgressString => $"{SelCovName} Progress";


        private Covenant? SelCovData => GetCovData();
        private Covenant? GetCovData()
        {
            // convert to enum
            if (Hook?.GameCovenantData?.TryGetValue(SelCovId, out var covenant) == true)
                return covenant;
            return null;
        }
        private string SelCovName => DS2SCovenant.All.First(x => (COV)x.ID == SelCovId).Name;


        // Binding Properties:
        public static ObservableCollection<DS2SCovenant> CovenantsList => new(DS2SCovenant.All);
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
        private DS2SCovenant _covSelected = DS2SCovenant.All.First(); // default NONE
        public bool CovDiscovered
        {
            get => SelCovData?.Discovered == true;
            set
            {
                if (SelCovData == null) return;
                Hook?.SetCovenantDiscov(SelCovId, value);
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
                Hook?.SetCovenantRank(SelCovId, value);
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
                Hook?.SetCovenantProgress(SelCovId, value);
                OnPropertyChanged();
            }
        }

        private void FixRankOnProgressChange(int newprog)
        {
            var lvls = DS2SCovenant.All.Where(x => (COV)x.ID == SelCovId).First().Levels.Split('/').ToList();
            var newrank = lvls.FindIndex(lvl => newprog < int.Parse(lvl));
            if (newrank < 0)
                newrank = 3;
            Hook?.SetCovenantRank(SelCovId, newrank);
        }
        private void FixProgressOnRankChange(int newrank)
        {
            // fix progress:
            var lvls = DS2SCovenant.All.Where(x => (COV)x.ID == SelCovId).First().Levels.Split('/');
            var currProg = CovProgress;
            var newProg = currProg;
            var rankLB = newrank > 0 ? int.Parse(lvls[newrank - 1]) : 0;
            var rankUB = newrank < 3 ? int.Parse(lvls[newrank - 1 + 1]) - 1 : 999;
            if (currProg > rankUB || currProg < rankLB)
                currProg = rankLB;
            CovProgress = currProg;
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
            OnPropertyChanged(nameof(CurrentCovenant));
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
