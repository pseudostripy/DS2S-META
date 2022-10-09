using DS2S_META.Utils;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS2S_META
{
    public partial class DmgCalcControl : METAControl
    {
        // Fields:
        private List<DS2SItem> Weapons => DS2SItemCategory.AllWeapons; // shorthand
        private DS2SItem? SelDs2item;
        //private WeaponRow Wep;
        internal ItemRow? Item;
        Timer InventoryTimer = new Timer();
        private bool upgradeManualOverride = false;
        
        // Constructor
        public DmgCalcControl()
        {
            InitializeComponent();
        }

        // Initialisation
        public override void InitTab()
        {
            InventoryTimer.Interval = 100;
            InventoryTimer.Elapsed += InventoryTimer_Elapsed;
            /*lbxItems.ItemsSource = Weapons*/;
        }
        private void InventoryTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
        }
        internal override void ReloadCtrl()
        {
            lbxWeapons.SelectedIndex = -1;
            lbxWeapons.SelectedIndex = 0;
            nudUpgrade.Value = 0;
        }
        internal override void EnableCtrls(bool enable)
        {
            InventoryTimer.Enabled = enable;
            btnCalculate.IsEnabled = enable;

            nudUpgrade.Value = 0;
            nudUpgrade.Maximum = 5;
            upgradeManualOverride = false;
        }

        // Core
        //private void GetWeaponProperties()
        //{
        //    if (SelDs2item == null)
        //        throw new Exception("Null weapon selected");

        //    var wep = ParamMan.GetWeaponFromID(SelDs2item.itemID);
        //    if (wep != null)
        //        Wep = wep; // save to class for easier access


        //    lblHmodValue.Content = $"lMod = {Wep.WTypeRow?.lMod}, rMod = {Wep.WTypeRow?.rMod}";
        //}
        //private void SetHmodValue()
        //{
        //    if (Wep == null)
        //    {
        //        lblHmodValue.Content = naempty;
        //        return;
        //    }

        //    string lmod = $"{Wep.WTypeRow?.lMod:F2}";
        //    string rmod = $"{Wep.WTypeRow?.rMod:F2}";
        //    lblHmodValue.Content = cbxLeftHand.IsChecked == true ? lmod : rmod;
        //}

        // Main interactions:
        private void FilterItems()
        {
            ////Clear items and add the ones that match text in search box

            //// Update listbox
            //lbxWeapons.ItemsSource = Weapons.Where(wp => wp.NameContains(txtSearch.Text));
            
            //if (lbxWeapons.Items.Count > 0)
            //    lbxWeapons.SelectedIndex = 0;

            //HandleSearchLabel();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterItems();
        }
        private void HandleSearchLabel()
        {
            //Handles the "Searching..." label on the text box
            if (txtSearch.Text == "")
                lblSearch.Visibility = Visibility.Visible;
            else
                lblSearch.Visibility = Visibility.Hidden;

        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            // Build up the item string:
            var item = lbxWeapons.SelectedItem as DS2SItem;
            if (item == null) return;

            var inf = cmbInfusion.SelectedItem as DS2SInfusion;
            if (item == null) return;

            var upgr = nudUpgrade.Value;
            if (upgr == null) return;

            // Success: Store, Show, Enable:
            lblSelectedWeapon.Content = $"{inf} {item} +{upgr}";
            SelDs2item = item;

            var wep = ParamMan.GetWeaponFromID(SelDs2item.itemID);
            
            btnCalculate.IsEnabled = true;
        }
        private void cbxOHKO_Checked(object sender, RoutedEventArgs e)
        {
            if (Hook == null)
                return; // first call

            float dmgmod;
            if (cbxOHKO.IsChecked == true)
                dmgmod = 1000;
            else
                dmgmod = 1;

            // Write to memory
            var rapierrow = ParamMan.WeaponParam?.Rows.FirstOrDefault(r => r.ID == 1500000) as WeaponRow; // Rapier
            if (rapierrow == null)
                throw new NullReferenceException("Pretty sure the rapier should be there!");
            var F = rapierrow.Param.Fields[34];
            byte[] dmgbytes = BitConverter.GetBytes(dmgmod);
            Array.Copy(dmgbytes, 0, rapierrow.RowBytes, F.FieldOffset, F.FieldLength);
            rapierrow.WriteRow();
        }
        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            int test = 1;
            //GetWeaponProperties();
        }
        private void cbxMaxUpgrade_Checked(object sender, RoutedEventArgs e)
        {
            if (cbxMax.IsChecked == true)
            {
                upgradeManualOverride = false;
                nudUpgrade.Value = nudUpgrade.Maximum;
                return;
            }
        }
        private int? HandleMaxItemCheckbox()
        {
            // Max checkbox is false
            if (cbxMax.IsChecked != true)
                return upgradeManualOverride ? nudUpgrade.Value : 0;

            // Max checkbox is true:
            if (upgradeManualOverride)
                return nudUpgrade.Value <= nudUpgrade.Maximum ? nudUpgrade.Value : nudUpgrade.Maximum;

            // Max checkbox is true && noManualOverride yet
            return nudUpgrade.Maximum;
        }
        private void nudUpgrade_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (nudUpgrade.Value != nudUpgrade.Maximum)
            {
                cbxMax.IsChecked = false;
                upgradeManualOverride = true;
            }
                
        }

        private bool TryGetSelectedWeapon(out WeaponRow? wep)
        {
            // Get weapon selected in listbox
            wep = default;
            if (lbxWeapons == null)
                return false;

            if (lbxWeapons.SelectedIndex == -1)
                return false;

            var item = (DS2SItem)lbxWeapons.SelectedItem;
            if (item == null)
                return false;

            if (TryGetWeapon(item.ID, out wep))
                return true;
            return false;
        }
        private void cmbInfusion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var infusion = cmbInfusion.SelectedItem as DS2SInfusion;
            //Checks if cbxMaxUpgrade is checked and sets the value to max value
            HandleMaxItemCheckbox();
        }
        private bool TryGetWeapon(int id, out WeaponRow? wep)
        {
            wep = default;
            // Guard clauses
            if (!Hook.Hooked)
            {
                MessageBox.Show("Please open Dark Souls 2 first.");
                return false;
            };
            
            wep = ParamMan.GetWeaponFromID(id); // get weapon
            return wep != null;
        }
        private void lbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!TryGetSelectedWeapon(out var wep))
                return;
            if (wep == null) return;

            // Update infusion/upgrade ..?
            var selid = cmbInfusion.SelectedIndex;
            cmbInfusion.ItemsSource = wep.GetInfusionList();

            if (selid <= cmbInfusion.Items.Count)
                cmbInfusion.SelectedIndex = selid; // keep previous selection
            else
                cmbInfusion.SelectedIndex = 0; 
            cmbInfusion.IsEnabled = cmbInfusion.Items.Count > 1;

            nudUpgrade.Maximum = wep.MaxUpgrade;
            nudUpgrade.IsEnabled = nudUpgrade.Maximum > 0;
            nudUpgrade.Value = HandleMaxItemCheckbox();
        }

        // Events handling etc:
        private void ScrollListbox(KeyEventArgs e)
        {
            //Scroll down through Items listbox and go back to bottom at end
            if (e.Key == Key.Up)
            {
                e.Handled = true;//Do not pass keypress along

                //One liner meme that does the exact same thing as the code above
                lbxWeapons.SelectedIndex = ((lbxWeapons.SelectedIndex - 1) + lbxWeapons.Items.Count) % lbxWeapons.Items.Count;
                lbxWeapons.ScrollIntoView(lbxWeapons.SelectedItem);
                return;
            }

            //Scroll down through Items listbox and go back to top at end
            if (e.Key == Key.Down)
            {
                e.Handled = true;//Do not pass keypress along

                //One liner meme that does the exact same thing as the code above
                lbxWeapons.SelectedIndex = (lbxWeapons.SelectedIndex + 1) % lbxWeapons.Items.Count;
                lbxWeapons.ScrollIntoView(lbxWeapons.SelectedItem);
                return;
            }
        }
        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                txtSearch.Clear();
                return;
            }

            //Create selected index as item
            if (e.Key == Key.Enter)
            {
                e.Handled = true; //Do not pass keypress along
                //CreateItem();
                return;
            }

            //Return if sender is cmbInfusion so that arrow Key are handled correctly
            if (sender == cmbInfusion)
                return;
            //Prevents up and down Key from moving the cursor left and right when nothing in item box
            if (lbxWeapons.Items.Count == 0)
            {
                if (e.Key == Key.Up)
                    e.Handled = true; //Do not pass keypress along
                if (e.Key == Key.Down)
                    e.Handled = true; //Do not pass keypress along
                return;
            }

            ScrollListbox(e);
        }
        private void txtSearch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.SelectAll();
            txtSearch.Focus();
            e.Handled = true;
        }
        private void cmbInfusion_KeyDown(object sender, KeyEventArgs e)
        {
            //Create selected index as item
            if (e.Key == Key.Enter)
            {
                e.Handled = true; //Do not pass keypress along
                return;
            }
        }

        
    }
}
