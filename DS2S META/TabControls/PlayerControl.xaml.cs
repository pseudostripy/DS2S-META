using DS2S_META.Utils;
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

        private State.PlayerState PlayerState;

        private List<SavedPos> Positions = new List<SavedPos>();

        public override void InitTab()
        {
            PlayerState.Set = false;
            foreach (var bonfire in DS2SBonfire.All)
                cmbBonfire.Items.Add(bonfire);
            LastSetBonfire = new DS2SBonfire(0, 0, "Last Set: _Game Start"); //last set bonfire (default values) // TODO cleaner.
            cmbBonfire.Items.Add(LastSetBonfire); //add to end of filter
            Positions = SavedPos.GetSavedPositions()?? new();
            cmbStoredPositions.Items.Add(new SavedPos());
            btnWarp.IsEnabled = true;
            UpdatePositions();
        }
        internal override void ReloadCtrl()
        {
            if (WarpRest)
                Hook.ApplySpecialEffect(110000010);
        }
        internal override void EnableCtrls(bool enable)
        {
            btnPosStore.IsEnabled = enable;
            btnPosRestore.IsEnabled = enable;
            nudPosStoredX.IsEnabled = enable;
            nudPosStoredY.IsEnabled = enable;
            nudPosStoredZ.IsEnabled = enable;
            nudHealth.IsEnabled = enable;
            nudStamina.IsEnabled = enable;
            cbxSpeed.IsEnabled = enable || Hook.Setup;
            cbxGravity.IsEnabled = enable;
            cbxCollision.IsEnabled = enable;
            btnWarp.IsEnabled = enable && !Hook.Multiplayer;
            cbxWarpRest.IsEnabled = enable;

            if (enable)
                cmbBonfire.SelectedIndex = cmbBonfire.Items.Count - 1;
        }
        public void StorePosition()
        {
            if (btnPosStore.IsEnabled)
            {
                var pos = new SavedPos();
                pos.Name = cmbStoredPositions.Text;
                nudPosStoredX.Value = (decimal)Hook.StableX;
                nudPosStoredY.Value = (decimal)Hook.StableY;
                nudPosStoredZ.Value = (decimal)Hook.StableZ;
                PlayerState.AngX = Hook.AngX;
                PlayerState.AngY = Hook.AngY;
                PlayerState.AngZ = Hook.AngZ;
                PlayerState.HP = nudHealth.Value?? 0; // default to 0 if null
                PlayerState.Stamina = nudStamina.Value?? 0;
                PlayerState.FollowCam = Hook.CameraData;
                PlayerState.FollowCam2 = Hook.CameraData2;
                PlayerState.Set = true;
                pos.X = Hook.StableX;
                pos.Y = Hook.StableY;
                pos.Z = Hook.StableZ;
                pos.PlayerState = PlayerState;
                ProcessSavedPos(pos);
                UpdatePositions();
                SavedPos.Save(Positions);

                txtAngX.Text = PlayerState.AngX.ToString("N2");
                txtAngY.Text = PlayerState.AngY.ToString("N2");
                txtAngZ.Text = PlayerState.AngZ.ToString("N2");
            }
        }
        public void ProcessSavedPos(SavedPos pos)
        {
            if (!string.IsNullOrWhiteSpace(cmbStoredPositions.Text))
            {
                if (Positions.Any(n => n.Name == cmbStoredPositions.Text))
                {
                    var old = Positions.Single(n => n.Name == cmbStoredPositions.Text);
                    Positions.Remove(old);
                    Positions.Add(pos);
                    return;
                }

                Positions.Add(pos);
            }
            else
            {
                cmbStoredPositions.Items[0] = pos;
            }

        }
        private void storedPositions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StorePosition();
            }

            var shift = (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift));

            if (e.Key == Key.Delete && shift)
            {
                DeleteButton_Click(sender, e);
            }
        }
        private void UpdatePositions()
        {
            if (cmbStoredPositions.SelectedItem != new SavedPos())
            {
                var blank = cmbStoredPositions.Items[0] as SavedPos;
                if (blank == null)
                    blank = new SavedPos();

                cmbStoredPositions.Items.Clear();
                cmbStoredPositions.Items.Add(blank);
                foreach (var item in Positions)
                {
                    cmbStoredPositions.Items.Add(item);
                }
            }

        }
        public void RestorePosition()
        {
            if (btnPosRestore.IsEnabled)
            {
                if (!nudPosStoredX.Value.HasValue || !nudPosStoredY.Value.HasValue || !nudPosStoredZ.Value.HasValue)
                    return;

                Hook.StableX = (float)nudPosStoredX.Value;
                Hook.StableY = (float)nudPosStoredY.Value;
                Hook.StableZ = (float)nudPosStoredZ.Value;
                Hook.AngX = PlayerState.AngX;
                Hook.AngY = PlayerState.AngY;
                Hook.AngZ = PlayerState.AngZ;
                //Hook.CameraData = PlayerState.FollowCam;
                //Hook.CamX = CamX;
                //Hook.CamY = CamY;
                //Hook.CamZ = CamZ;
                if (cbxRestoreState.IsChecked == true)
                {
                    nudHealth.Value = PlayerState.HP;
                    nudStamina.Value = PlayerState.Stamina;
                }
            }
        }

        public void RemoveSavedPos()
        {
            if (Positions.Any(n => n.Name == cmbStoredPositions.Text))
            {
                //if (MessageBox.Show("Are you sure you want to delete this positon?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                //{
                //    var old = Positions.Single(n => n.Name == cmbStoredPositions.Text);
                //    Positions.Remove(old);
                //    cmbStoredPositions.SelectedIndex = 0;
                //    UpdatePositions();
                //    SavedPos.Save(Positions);
                //}

            }

        }
        

        public bool WarpRest { get; private set; }

        internal override void UpdateCtrl() 
        {
            //manage unknown warps and current warps that are not in filter
            var bonfireID = Hook.LastBonfireID;

            if (LastSetBonfire == null)
                return;

            if (LastSetBonfire.ID != bonfireID) // lastSetBonfire does not match game LastBonfire
            {
                //target warp is not in filter
                var result = DS2SBonfire.All.FirstOrDefault(b => b.ID == bonfireID); //check if warp is in bonfire resource
                if (result == null)
                {
                    //bonfire not in filter. Add to filter as unknown
                    result = new DS2SBonfire(Hook.LastBonfireAreaID ,bonfireID, $"Unknown {Hook.LastBonfireAreaID}: {bonfireID}");
                    DS2SBonfire.All.Add(result);
                    FilterBonfires();
                }

                //manage lastSetBonfire
                cmbBonfire.Items.Remove(LastSetBonfire); //remove from filter (if there)

                LastSetBonfire.AreaID = result.AreaID;
                LastSetBonfire.ID = result.ID;
                LastSetBonfire.Name = "Last Set: " + result.Name;

                cmbBonfire.Items.Add(LastSetBonfire); //add to end of filter
                cmbBonfire.SelectedItem = LastSetBonfire;
                //AddLastSetBonfire();
            }
        }
        private void FilterBonfires()
        {
            //warp filter management

            cmbBonfire.Items.Clear();
            cmbBonfire.SelectedItem = null;

            //go through bonfire resource and add to filter
            foreach (var bonfire in DS2SBonfire.All)
            {
                if (bonfire.ToString().ToLower().Contains(txtSearch.Text.ToLower()))
                {
                    cmbBonfire.Items.Add(bonfire);
                }
            }

            cmbBonfire.Items.Add(LastSetBonfire); //add lastSetBonfire to end of filter

            cmbBonfire.SelectedIndex = 0;

            if (txtSearch.Text == "")
                lblSearch.Visibility = Visibility.Visible;
            else
                lblSearch.Visibility = Visibility.Hidden;
        }
        
        public void ToggleGravity()
        {
            cbxGravity.IsChecked = !cbxGravity.IsChecked;
        }
        private void cbxOHKO_Checked(object sender, RoutedEventArgs e)
        {
            if (!Hook.Hooked)
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


        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            StorePosition();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            RestorePosition();
        }

        private void cbxSpeed_Checked(object sender, RoutedEventArgs e)
        {
            nudSpeed.IsEnabled = cbxSpeed.IsChecked == true;
            Hook.Speedhack(cbxSpeed.IsChecked == true);
        }

        private void SetGameSpeed()
        {
            if (GameLoaded && Hook.Hooked)
                Hook.SetSpeed((float)(nudSpeed.Value ?? 1));
        }
        private void nudSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetGameSpeed();
        }
        private void nudSpeed_LostFocus(object sender, RoutedEventArgs e)
        {
            SetGameSpeed();
        }
        private void cmbBonfire_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard clauses
            if (!Hook.Loaded)
                return;
            if (cbxQuickSelectBonfire.IsChecked != true)
                return;
            if (cmbBonfire.SelectedItem is not DS2SBonfire bonfire)
                throw new NullReferenceException("Unexpected bonfire");
            
            // Do stuff
            Hook.LastBonfireID = bonfire.ID;
            Hook.LastBonfireAreaID = bonfire.AreaID;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSavedPos();
        }

        private void storedPositions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var savedPos = cmbStoredPositions.SelectedItem as SavedPos;
            if (savedPos == null)
                return;

            nudPosStoredX.Value = (decimal)savedPos.X;
            nudPosStoredY.Value = (decimal)savedPos.Y;
            nudPosStoredZ.Value = (decimal)savedPos.Z;
            PlayerState = savedPos.PlayerState;
            txtAngX.Text = PlayerState.AngX.ToString("N2");
            txtAngY.Text = PlayerState.AngY.ToString("N2");
            txtAngZ.Text = PlayerState.AngZ.ToString("N2");
        }

        private void WarpButton_Click(object sender, RoutedEventArgs e)
        {
            Warp();
        }

        public void Warp()
        {
            _ = ChangeColor(Brushes.DarkGray);
            if (Hook.Multiplayer)
            {
                MessageBox.Show("Warning: Cannot warp while engaging in Multiplayer", "Multiplayer Warp Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var bonfire = cmbBonfire.SelectedItem as DS2SBonfire;

            // Handle betwixt start warps:
            bool NoPrevBonfire = bonfire == null || bonfire.ID == 0 || bonfire.AreaID == 0;
            if (NoPrevBonfire)
            {
                int BETWIXTAREA = 167903232;
                ushort BETWIXTBFID = 2650;
                Hook.LastBonfireAreaID = BETWIXTAREA;
                Hook.Warp(BETWIXTBFID, true);
                return;
            }


            if (bonfire == null)
                throw new Exception("How do we get here intellisense??");

            Hook.LastBonfireID = bonfire.ID;
            Hook.LastBonfireAreaID = bonfire.AreaID;
            var warped = Hook.Warp(bonfire.ID);
            if (warped && cbxWarpRest.IsChecked == true)
                WarpRest = true; 
        }

        private async Task ChangeColor(Brush new_color)
        {
            btnWarp.Background = new_color;

            await Task.Delay(TimeSpan.FromSeconds(.25));

            btnWarp.Background = default(Brush);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterBonfires();
        }
        private void KeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                txtSearch.Clear();

            KeyDownListbox(e);
        }
        private void KeyDownListbox(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;

                if (cmbBonfire.SelectedIndex < cmbBonfire.Items.Count - 1)
                {
                    cmbBonfire.SelectedIndex += 1;
                    return;
                }
            }

            if (e.Key == Key.Up)
            {
                e.Handled = true;

                if (cmbBonfire.SelectedIndex != 0)
                {
                    cmbBonfire.SelectedIndex -= 1;
                    return;
                }
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Warp();
                return;
            }
        }
        private void txtSearch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.SelectAll();
            txtSearch.Focus();
            e.Handled=true;
        }
    }
}
