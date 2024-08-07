using DS2S_META.Randomizer;
using DS2S_META.Utils;
using DS2S_META.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl : METAControl
    {
        private DS2SBonfire? LastSetBonfire;

        public PlayerControl()
        {
            InitializeComponent();
        }

        
        public PlayerViewModel VM;

        public override void InitTab()
        {
            VM = (PlayerViewModel)DataContext; // todo setup command objects to the ViewModel in xaml
        }
        
        
        internal override void UpdateCtrl() 
        {
        }

        private void CbxDisableSkirtDamage_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
