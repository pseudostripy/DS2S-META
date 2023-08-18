using DS2S_META.Randomizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS2S_META.ViewModels
{
    public class ItemRestriction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Fields/Properties:
        public string Name { get; set; }
        public int ItemID { get; set; }        // chosen randomly
        public List<int> ItemIDs { get; set; } // all options
        public ITEMGROUP GroupType { get; set; }
        private const int LIMDISTMIN = 1;
        private const int LIMDISTMAX = 20;


        private RestrType _restrType = RestrType.Anywhere; // combo box
        public RestrType RestrType
        {
            get => _restrType;
            set
            {
                _restrType = value;
                OnPropertyChanged(nameof(RestrType));
                OnPropertyChanged(nameof(VisDistSettings));
            }
        }
        public Visibility VisDistSettings => RestrType == RestrType.Distance ? Visibility.Visible : Visibility.Collapsed;
        private int _distMin = LIMDISTMIN;
        public int DistMin
        {
            get => _distMin;
            set
            {
                var limval = Math.Max(value, LIMDISTMIN);
                _distMin = limval < DistMax ? limval : DistMax;
                OnPropertyChanged(nameof(DistMin));
            }
        }
        private int _distMax = LIMDISTMAX;
        public int DistMax
        {
            get => _distMax;
            set
            {
                var limval = Math.Min(value, LIMDISTMAX);
                _distMax = limval > DistMin ? limval : DistMin;
                OnPropertyChanged(nameof(DistMax));
            }
        }


        public static Dictionary<RestrType, string> TypeComboItems { get; set; } = new()
        {
            [RestrType.Anywhere] = "Anywhere",
            [RestrType.Vanilla] = "Vanilla",
            [RestrType.Distance] = "Distance"
        };

        // Constructors:
        public ItemRestriction()
        {
            // Necessary for deserialization
            ItemIDs = new();
            Name = string.Empty;
        }
        public ItemRestriction(string name, ITEMID itemID, 
                                RestrType restrType = RestrType.Anywhere, 
                                int distmin = LIMDISTMIN, 
                                int distmax = LIMDISTMAX)
        {
            Name = name;
            ItemIDs = new List<int>() { (int)itemID };
            GroupType = ITEMGROUP.Specified;
            RestrType = restrType;
            DistMin = distmin;
            DistMax = distmax;
        }

        public ItemRestriction(string name, ITEMGROUP grp, 
                                RestrType restrType = RestrType.Anywhere, 
                                int distmin = LIMDISTMIN, 
                                int distmax = LIMDISTMAX)
        {
            Name = name;
            ItemIDs = DS2Data.ItemGroups[grp];
            GroupType = grp;
            RestrType = restrType;
            DistMin = distmin;
            DistMax = distmax;
        }

    }
}
