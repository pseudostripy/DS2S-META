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
        private DS2SItem SelDs2item;
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
            lbxItems.ItemsSource = Weapons;
        }
        private void InventoryTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
        }
        internal override void ReloadCtrl()
        {
            lbxItems.SelectedIndex = -1;
            lbxItems.SelectedIndex = 0;
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
        private void GetWeaponProperties()
        {
            if (SelDs2item == null)
                throw new Exception("Null weapon selected");


        }

        // Main interactions:
        private void FilterItems()
        {
            //Clear items and add the ones that match text in search box

            // Update listbox
            lbxItems.ItemsSource = Weapons.Where(wp => wp.NameContains(txtSearch.Text));
            
            if (lbxItems.Items.Count > 0)
                lbxItems.SelectedIndex = 0;

            HandleSearchLabel();
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
            var item = lbxItems.SelectedItem as DS2SItem;
            if (item == null) return;

            var inf = cmbInfusion.SelectedItem as DS2SInfusion;
            if (item == null) return;

            var upgr = nudUpgrade.Value;
            if (upgr == null) return;

            // Success: Store, Show, Enable:
            lblSelectedWeapon.Content = $"{inf} {item} +{upgr}";
            SelDs2item = item;
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
            var rapierrow = Hook.WeaponParam?.Rows.Where(row => row.ID == 1500000).First(); // Rapier
            if (rapierrow == null)
                throw new NullReferenceException("Pretty sure the rapier should be there!");
            var F = rapierrow.Param.Fields[34];
            byte[] dmgbytes = BitConverter.GetBytes(dmgmod);
            Array.Copy(dmgbytes, 0, rapierrow.RowBytes, F.FieldOffset, F.FieldLength);
            rapierrow.WriteRow();
        }
        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            GetWeaponProperties();
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
                upgradeManualOverride = true;
        }

        private bool TryGetSelectedItem(out DS2SItem? item)
        {
            item = default;
            if (lbxItems == null)
                return false;

            if (lbxItems.SelectedIndex == -1)
                return false;

            item = (DS2SItem)lbxItems.SelectedItem;
            if (item == null)
                return false;

            return true;
        }
        private void cmbInfusion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var infusion = cmbInfusion.SelectedItem as DS2SInfusion;
            //Checks if cbxMaxUpgrade is checked and sets the value to max value
            HandleMaxItemCheckbox();
        }
        private void lbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard clauses
            if (!Hook.Hooked)
            {
                MessageBox.Show("Please open Dark Souls 2 first.");
                return;
            };
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null)
                return;

            // Update infusion/upgrade ..?
            var selid = cmbInfusion.SelectedIndex;
            cmbInfusion.Items.Clear();
            if (item.Type != DS2SItem.ItemType.Weapon)
                cmbInfusion.Items.Add(DS2SInfusion.Infusions[0]);
            else
                foreach (var infusion in Hook.GetWeaponInfusions(item.ID))
                    cmbInfusion.Items.Add(infusion);

            if (selid <= cmbInfusion.Items.Count)
                cmbInfusion.SelectedIndex = selid; // keep previous selection
            else
                cmbInfusion.SelectedIndex = 0; 
            cmbInfusion.IsEnabled = cmbInfusion.Items.Count > 1;


            nudUpgrade.Maximum = Hook.GetMaxUpgrade(item);
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
                lbxItems.SelectedIndex = ((lbxItems.SelectedIndex - 1) + lbxItems.Items.Count) % lbxItems.Items.Count;
                lbxItems.ScrollIntoView(lbxItems.SelectedItem);
                return;
            }

            //Scroll down through Items listbox and go back to top at end
            if (e.Key == Key.Down)
            {
                e.Handled = true;//Do not pass keypress along

                //One liner meme that does the exact same thing as the code above
                lbxItems.SelectedIndex = (lbxItems.SelectedIndex + 1) % lbxItems.Items.Count;
                lbxItems.ScrollIntoView(lbxItems.SelectedItem);
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
            if (lbxItems.Items.Count == 0)
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
        private void nudUpgrade_Click(object sender, EventArgs e)
        {
            nudUpgrade.Focus();
        }
    }
}
