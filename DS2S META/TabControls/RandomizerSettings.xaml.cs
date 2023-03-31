using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DS2S_META.Randomizer;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DS2S_META.Properties;
using System.Xml.Serialization;
using static DS2S_META.RandomizerSettings;
using System.Runtime.CompilerServices;
using DS2S_META.ViewModels;

namespace DS2S_META
{   
    using IPRSList = ObservableCollection<ItemRestriction>;
    
    public partial class RandomizerSettings : METAControl
    {
        // Constructor:
        public RandomizerSettings()
        {
            InitializeComponent();
        }
    }
}