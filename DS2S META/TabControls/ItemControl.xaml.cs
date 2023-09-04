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
    public partial class ItemControl : METAControl
    {
        Timer InventoryTimer = new Timer();

        public ItemControl()
        {
            InitializeComponent();
        }

        // Setup stuff:
        private Dictionary<ITEMCATEGORY, string> CategoryNames = new()
        {
            { ITEMCATEGORY.Ammo, "Ammo" },
            { ITEMCATEGORY.Shields, "Shields" },
            { ITEMCATEGORY.Spells, "Spells" },
            { ITEMCATEGORY.RangedWeapons, "Ranged Weapons" },
            { ITEMCATEGORY.RemovedItems, "Removed Content" },
            { ITEMCATEGORY.Gesture, "Gestures" },
            { ITEMCATEGORY.KeyItems, "Key Items" },
            { ITEMCATEGORY.Rings, "Rings" },
            { ITEMCATEGORY.StaffChimes, "Staves/Chimes" },
            { ITEMCATEGORY.MeleeWeapon, "Melee Weapons" },
            { ITEMCATEGORY.Armor, "Armor" },
            { ITEMCATEGORY.Item, "Item" },
        };
        public override void InitTab()
        {
            cmbCategory.ItemsSource = DS2Resource.ItemCategories.Select(ic => CategoryNames[ic.Type]);
            cmbCategory.SelectedIndex = 0;
            FilterItems();
            InventoryTimer.Interval = 100;
            InventoryTimer.Elapsed += InventoryTimer_Elapsed;
        }
        internal override void ReloadCtrl()
        {
            lbxItems.SelectedIndex = -1;
            lbxItems.SelectedIndex = 0;
        }
        internal override void EnableCtrls(bool enable)
        {
            InventoryTimer.Enabled = enable;
            btnCreate.IsEnabled = enable;

            if (enable)
                UpdateCreateEnabled();
        }
        private void InventoryTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //Dispatcher.Invoke(new Action(() =>
            //{
            //    if (Properties.Settings.Default.UpdateMaxLive)
            //        HandleMaxAvailable();
            //}));
        }

        private void HandleMaxAvailable()
        {
            if (cbxQuantityRestrict.IsChecked != true)
                return;

            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null) return;

            var diff = MaxMinusHeld(item);
            if (diff != nudQuantity.Maximum)
            {
                nudQuantity.Maximum = diff;
                if (cbxMax.IsChecked == true)
                    nudQuantity.Value = nudQuantity.Maximum;

                nudQuantity.IsEnabled = nudQuantity.Maximum > 1;
                txtMaxHeld.Visibility = nudQuantity.Maximum > 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }


        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterItems();
        }
        private void cmbInfusion_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var infusion = cmbInfusion.SelectedItem as DS2SInfusion;
            //Checks if cbxMaxUpgrade is checked and sets the value to max value
            HandleMaxItemCheckbox();
        }
        private void lbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ParamMan.IsLoaded) return;
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null) return;

            // update quantities based on newly selected item
            UpdateQuantityAndTextVis();

            // Update infusion/upgrade ..?
            var selid = cmbInfusion.SelectedIndex;
            cmbInfusion.ItemsSource = item.GetInfusionList();

            if (selid <= cmbInfusion.Items.Count)
                cmbInfusion.SelectedIndex = selid; // keep previous selection
            else
                cmbInfusion.SelectedIndex = 0;
            cmbInfusion.IsEnabled = cmbInfusion.Items.Count > 1;


            nudUpgrade.Maximum = Hook.GetMaxUpgrade(item);
            nudUpgrade.IsEnabled = nudUpgrade.Maximum > 0;

            btnCreate.IsEnabled = Hook.GetIsDroppable(item) || Properties.Settings.Default.SpawnUndroppable;
            if (!Properties.Settings.Default.UpdateMaxLive)
                HandleMaxAvailable();
            HandleMaxItemCheckbox();
        }

        //Clear items and add the ones that match text in search box
        private void FilterItems()
        {
            lbxItems.Items.Clear();

            if (SearchAllCheckbox.IsChecked == true && txtSearch.Text != "")
            {
                //search every item category
                foreach (DS2SItemCategory category in DS2Resource.ItemCategories)
                {
                    foreach (DS2SItem item in category.Items)
                    {
                        if (item.ToString().ToLower().Contains(txtSearch.Text.ToLower()))
                            lbxItems.Items.Add(item);
                    }
                }
            }
            else
            {
                // Note this adds all items if search is empty string
                var cat = DS2Resource.ItemCategories.ElementAt(cmbCategory.SelectedIndex);
                foreach (DS2SItem item in cat.Items)
                {
                    if (item.ToString().ToLower().Contains(txtSearch.Text.ToLower()))
                        lbxItems.Items.Add(item);
                }
            }

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
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null) return;

            // Update maximum based on cbx value
            setQuantityMaximum(item);

            // Update UI
            if (cbxQuantityRestrict.IsChecked == true)
            {
                // restricted
                nudQuantity.IsEnabled = nudQuantity.Maximum > 1;
            }
            else
            {
                // unrestricted
                nudQuantity.IsEnabled = true;
            }
            txtMaxHeld.Visibility = MaxMinusHeld(item) > 0 ? Visibility.Hidden : Visibility.Visible;
        }
        private bool TryGetSelectedItem(out ItemRow? item)
        {
            item = default;
            if (lbxItems == null)
                return false;

            if (lbxItems.SelectedIndex == -1)
                return false;

            var ds2item = lbxItems.SelectedItem as DS2SItem;
            if (ds2item == null)
                return false;

            if (TryGetItem(ds2item.ItemId, out item))
                return true;
            return false;
        }
        private bool TryGetItem(int id, out ItemRow? item)
        {
            item = default;
            // Guard clauses
            if (!Hook.Hooked)
            {
                MessageBox.Show("Please open Dark Souls 2 first.");
                return false;
            };

            if (ParamMan.IsLoaded == true)
                item = id.TryAsItemRow(); // get weapon
            return item != null;
        }

        private void setQuantityMaximum(ItemRow item)
        {
            nudQuantity.Maximum = cbxQuantityRestrict.IsChecked == true ? MaxMinusHeld(item) : 99;
        }

        private int MaxMinusHeld(ItemRow item)
        {
            var max = item.MaxHeld;
            var held = Hook.GetHeld(item);
            return max - held;
        }

        

        

        public void UpdateCreateEnabled()
        {
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null) return;

            btnCreate.IsEnabled = Hook.GetIsDroppable(item) || Properties.Settings.Default.SpawnUndroppable;
        }

        internal void EnableStats(bool enable)
        {
            btnCreate.IsEnabled = enable;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateItem();
        }

        //Apply hair to currently loaded character
        public void CreateItem()
        {
            if (!Properties.Settings.Default.UpdateMaxLive)
                HandleMaxAvailable();

            //Check if the button is enabled and the selected item isn't null
            if (btnCreate.IsEnabled)
            {
                _ = ChangeColor(Brushes.DarkGray);
                if (lbxItems.SelectedItem is not DS2SItem ds2item)
                    return;
                
                // Get values:
                short quanval = (short)(nudQuantity.Value?? 1);
                byte upgrval = (byte)(nudUpgrade.Value ?? 1);
                var infusion = cmbInfusion.SelectedItem as DS2SInfusion;
                if (infusion == null)
                    infusion = DS2SInfusion.Infusions[0];
                var infuidval = infusion.AsByte();
                 

                Hook.GiveItem(ds2item.ItemId, quanval, upgrval, infuidval);
                if (!Properties.Settings.Default.UpdateMaxLive)
                    HandleMaxAvailable();
            }
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
            btnCreate.Background = new_color;

            await Task.Delay(TimeSpan.FromSeconds(.25));

            btnCreate.Background = default(Brush);
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
                CreateItem();
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
            if (!TryGetSelectedItem(out var item))
                return;
            if (item == null) return;

            setQuantityMaximum(item);
            HandleMaxItemCheckbox();
            
        }

        private void HandleMaxItemCheckbox()
        {
            // Set maximum values
            // Assumes that setQuantityMaximum(item) has already been called!
            if (cbxMax.IsChecked == true)
            {
                nudQuantity.Value = nudQuantity.Maximum;
                nudUpgrade.Value = nudUpgrade.Maximum;
            }
            else
            {
                nudQuantity.Value = nudQuantity.Maximum == 0 ? 0 : 1;
                nudUpgrade.Value = 0;
            }
        }

        private void cmbInfusion_KeyDown(object sender, KeyEventArgs e)
        {
            //Create selected index as item
            if (e.Key == Key.Enter)
            {
                e.Handled = true; //Do not pass keypress along
                CreateItem();
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
