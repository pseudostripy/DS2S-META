using DS2S_META.Utils;
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
    /// <summary>
    /// Interaction logic for ItemControl.xaml
    /// </summary>
    public partial class DmgCalcControl : METAControl
    {
        // Fields:
        private List<DS2SItem> Weapons => DS2SItemCategory.AllWeapons; // shorthand
        internal ItemParam? Item;
        Timer InventoryTimer = new Timer();

        public DmgCalcControl()
        {
            InitializeComponent();
        }

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
        }

        internal override void EnableCtrls(bool enable)
        {
            InventoryTimer.Enabled = enable;
            btnCalculate.IsEnabled = enable;

            if (enable)
                UpdateCreateEnabled();
        }

        
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterItems();
        }

        //Clear items and add the ones that match text in search box
        private void FilterItems()
        {
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

        //Handles the "Searching..." label on the text box
        private void HandleSearchLabel()
        {
            if (txtSearch.Text == "")
                lblSearch.Visibility = Visibility.Visible;
            else
                lblSearch.Visibility = Visibility.Hidden;

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
                return;
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null)
                return;

            // Update infusion/upgrade ..?
            cmbInfusion.Items.Clear();
            if (item.Type != DS2SItem.ItemType.Weapon)
                cmbInfusion.Items.Add(DS2SInfusion.Infusions[0]);
            else
                foreach (var infusion in Hook.GetWeaponInfusions(item.ID))
                    cmbInfusion.Items.Add(infusion);

            cmbInfusion.SelectedIndex = 0;
            cmbInfusion.IsEnabled = cmbInfusion.Items.Count > 1;

            nudUpgrade.Maximum = Hook.GetMaxUpgrade(item);
            nudUpgrade.IsEnabled = nudUpgrade.Maximum > 0;

            HandleMaxItemCheckbox();
        }

        public void UpdateCreateEnabled()
        {
            //if (lbxItems.SelectedItem is not DS2SItem item)
            //    return;
        }

        internal void EnableStats(bool enable)
        {
            btnCalculate.IsEnabled = enable;
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        //handles up and down scrolling
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

        //handles escape
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

        //Select number in nud
        private void nudUpgrade_Click(object sender, EventArgs e)
        {
            nudUpgrade.Focus();
        }

        private void SearchAllCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            //checkbox changed, refresh search filter (if txtSearch is not empty)
            if (txtSearch.Text != "")
                FilterItems();
        }

        private void cbxMaxUpgrade_Checked(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedItem(out DS2SItem? item))
                return;
            
            HandleMaxItemCheckbox();
            
        }

        private void HandleMaxItemCheckbox()
        {
            if (cbxMax.IsChecked == true)
                nudUpgrade.Value = nudUpgrade.Maximum;
            else
                nudUpgrade.Value = 0;
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
        //Select all text in search box
        private void txtSearch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.SelectAll();
            txtSearch.Focus();
            e.Handled = true;
        }

        private void SearchAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text != "")
                FilterItems();
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
            lblSelectedWeapon.Content = $"{inf} {item} +{upgr}";

        }
    }
}
