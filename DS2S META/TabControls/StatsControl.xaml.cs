using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for StatsControl.xaml
    /// </summary>
    public partial class StatsControl : METAControl
    {
        public List<IntegerUpDown> nudLevels => new() { nudVig, nudEnd, nudVit, nudAtt, nudStr, nudDex, nudAdp, nudInt, nudFth };

        public StatsControl()
        {
            InitializeComponent();
            foreach (DS2SClass charClass in DS2Resource.Classes)
                cmbClass.Items.Add(charClass);
            cmbClass.SelectedIndex = -1;
        }
        public void ReloadTab()
        {

        }
        private void cbmClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Hook.InGame)
            {
                if (cmbClass.SelectedItem is not DS2SClass charClass)
                    throw new NullReferenceException("Null character class");

                Hook.Class = charClass.ID;
                nudVig.Minimum = charClass.Vigor;
                nudEnd.Minimum = charClass.Endurance;
                nudVit.Minimum = charClass.Vitality;
                nudAtt.Minimum = charClass.Attunement;
                nudStr.Minimum = charClass.Strength;
                nudDex.Minimum = charClass.Dexterity;
                nudAdp.Minimum = charClass.Adaptability;
                nudInt.Minimum = charClass.Intelligence;
                nudFth.Minimum = charClass.Faith;
            }
        }

        internal override void UpdateCtrl()
        {
        }

        internal override void ReloadCtrl()
        {
            cmbClass.SelectedItem = cmbClass.Items.Cast<DS2SClass>().FirstOrDefault(c => c.ID == Hook.Class);
            txtName.Text = Hook.CharacterName;
        }

        internal override void EnableCtrls(bool enable)
        {
            cmbClass.IsEnabled = enable;
            txtName.IsEnabled = enable;
            btnGive.IsEnabled = enable;
            btnResetSoulMemory.IsEnabled = enable;
            nudGiveSouls.IsEnabled = enable;
            nudVig.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudEnd.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudVit.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudAtt.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudStr.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudDex.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudAdp.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudInt.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudFth.IsEnabled = enable && Properties.Settings.Default.EditStats;
            nudHollowLevel.IsEnabled = enable;
            btnReset.IsEnabled = enable && Properties.Settings.Default.EditStats;
            btnMax.IsEnabled = enable && Properties.Settings.Default.EditStats;
            btnRestoreHumanity.IsEnabled = enable;

            if (!enable)
                cmbClass.SelectedIndex = -1;
        }
        private void GiveSouls_Click(object sender, RoutedEventArgs e)
        {
            if (nudGiveSouls.Value.HasValue)
                Hook.AddSouls(nudGiveSouls.Value.Value);
        }
        private void ResetSoulMemory_Click(object sender, RoutedEventArgs e)
        {
            Hook.ResetSoulMemory();
        }
        private void Name_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Hook.CharacterName = txtName.Text;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            foreach (IntegerUpDown nudLev in nudLevels)
                nudLev.Value = nudLev.Minimum;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            Hook.SetMaxLevels();
        }

        private void RestoreHumanity_Click(object sender, RoutedEventArgs e)
        {
            // These both work apparently:
            //var RestoreHumanityEffect = 60151000;   // Using Effigy effect
            //var RestoreHumanityEffect = 60355000;   // Warp effect
            Hook.RestoreHumanity();
        }

        private void NewTestCharacter_Click(object sender, RoutedEventArgs e)
        {
            Hook.NewTestCharacter();
        }

    }
}
