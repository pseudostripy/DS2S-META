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

        public DmgCalcControl()
        {
            InitializeComponent();
        }

        public override void InitTab()
        {
            FilterItems();
            InventoryTimer.Interval = 100;
            InventoryTimer.Elapsed += InventoryTimer_Elapsed;
        }

        private void InventoryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (Properties.Settings.Default.UpdateMaxLive)
                    HandleMaxAvailable();
            }));
        }

        private void HandleMaxAvailable()
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
            btnCalculate.IsEnabled= enable;

            if (enable)
                UpdateCreateEnabled();
        }

        Timer InventoryTimer = new Timer();
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

        private void cbxQuantityRestrict_Checked(object sender, RoutedEventArgs e)
        {
            UpdateQuantityAndTextVis();
        }

        private void UpdateQuantityAndTextVis()
        {
            if (!TryGetSelectedItem(out DS2SItem item))
                return;

            // Update maximum based on cbx value
            setQuantityMaximum(item);
            txtMaxHeld.Visibility = MaxMinusHeld(item) > 0 ? Visibility.Hidden : Visibility.Visible;
        }
        private bool TryGetSelectedItem(out DS2SItem item)
        {
            item = null;
            if (lbxItems == null)
                return false;

            if (lbxItems.SelectedIndex == -1)
                return false;

            item = lbxItems.SelectedItem as DS2SItem;
            if (item == null)
                return false;

            return true;
        }

        private void setQuantityMaximum(DS2SItem item)
        {
        }

        private int MaxMinusHeld(DS2SItem item)
        {
            var max = Hook.GetMaxQuantity(item);
            var held = Hook.GetHeld(item);
            return max - held;
        }

        private void cmbInfusion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var infusion = cmbInfusion.SelectedItem as DS2SInfusion;
            //Checks if cbxMaxUpgrade is checked and sets the value to max value
            HandleMaxItemCheckbox();
        }

        private void lbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Hook.Hooked) return;

            if (!TryGetSelectedItem(out DS2SItem item))
                return;

            // update quantities based on newly selected item
            UpdateQuantityAndTextVis();

            // Update infusion/upgrade ..?
            cmbInfusion.Items.Clear();
            if (item.Type == DS2SItem.ItemType.Weapon)
                foreach (var infusion in Hook.GetWeaponInfusions(item.ID))
                    cmbInfusion.Items.Add(infusion);
            else
                cmbInfusion.Items.Add(DS2SInfusion.Infusions[0]);
            
            cmbInfusion.SelectedIndex = 0;
            cmbInfusion.IsEnabled = cmbInfusion.Items.Count > 1;

            nudUpgrade.Maximum = Hook.GetMaxUpgrade(item);
            nudUpgrade.IsEnabled = nudUpgrade.Maximum > 0;

            btnCalculate.IsEnabled = Hook.GetIsDroppable(item.ID) || Properties.Settings.Default.SpawnUndroppable;
            if (!Properties.Settings.Default.UpdateMaxLive)
                HandleMaxAvailable();
            HandleMaxItemCheckbox();
        }

        public void UpdateCreateEnabled()
        {
            DS2SItem item = lbxItems.SelectedItem as DS2SItem;
            if (item == null)
                return;

            btnCalculate.IsEnabled = Hook.GetIsDroppable(item.ID) || Properties.Settings.Default.SpawnUndroppable;
        }

        internal void EnableStats(bool enable)
        {
            btnCalculate.IsEnabled = enable;
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
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

        //Changes the color of the Apply button
        private async Task ChangeColor(Brush new_color)
        {
            btnCalculate.Background = new_color;

            await Task.Delay(TimeSpan.FromSeconds(.25));

            btnCalculate.Background = default(Brush);
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
            if (!TryGetSelectedItem(out DS2SItem item))
                return;
            
            setQuantityMaximum(item);
            HandleMaxItemCheckbox();
            
        }

        private void HandleMaxItemCheckbox()
        {
            if (cbxMax.IsChecked.Value)
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
    }
}
