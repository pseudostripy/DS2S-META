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

        
        private State.PlayerState PlayerState;
        PlayerViewModel VM; // todo setup command objects to the ViewModel in xaml

        private List<SavedPos> Positions = new();

        public override void InitTab()
        {
            PlayerState.Set = false;
            //foreach (var bonfire in DS2Resource.Bonfires)
            //    cmbBonfire.Items.Add(bonfire);
            LastSetBonfire = new DS2SBonfire(0, 0, "Last Set: _Game Start"); //last set bonfire (default values) // TODO cleaner.
            //cmbBonfire.Items.Add(LastSetBonfire); //add to end of filter
            Positions = SavedPos.GetSavedPositions()?? new();
            cmbStoredPositions.Items.Add(new SavedPos());
            UpdatePositions();
            VM = (PlayerViewModel)DataContext; // todo setup command objects to the ViewModel in xaml
        }
        
        public void StorePosition()
        {
            //if (VM.Hook == null)
            //    return;
            //var pos = new SavedPos{ Name = cmbStoredPositions.Text };
            //nudPosStoredX.Value = (decimal)VM.Hook.StableX;
            //nudPosStoredY.Value = (decimal)VM.Hook.StableY;
            //nudPosStoredZ.Value = (decimal)VM.Hook.StableZ;
            //PlayerState.AngX = VM.Hook.AngX;
            //PlayerState.AngY = VM.Hook.AngY;
            //PlayerState.AngZ = VM.Hook.AngZ;
            //PlayerState.HP = nudHealth.Value?? 0; // default to 0 if null
            //PlayerState.Stamina = nudStamina.Value?? 0;
            //PlayerState.FollowCam = VM.Hook.CameraData;
            //PlayerState.FollowCam2 = VM.Hook.CameraData2;
            //PlayerState.Set = true;
            //pos.X = VM.Hook.StableX;
            //pos.Y = VM.Hook.StableY;
            //pos.Z = VM.Hook.StableZ;
            //pos.PlayerState = PlayerState;
            //ProcessSavedPos(pos);
            //UpdatePositions();
            //SavedPos.Save(Positions);

            //txtAngX.Text = PlayerState.AngX.ToString("N2");
            //txtAngY.Text = PlayerState.AngY.ToString("N2");
            //txtAngZ.Text = PlayerState.AngZ.ToString("N2");
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
        private void StoredPositions_KeyDown(object sender, KeyEventArgs e)
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
                blank ??= new SavedPos();

                cmbStoredPositions.Items.Clear();
                cmbStoredPositions.Items.Add(blank);
                foreach (var item in Positions)
                {
                    cmbStoredPositions.Items.Add(item);
                }
            }

        }
        //public void RestorePosition()
        //{
        //    if (!btnPosRestore.IsEnabled)
        //        return;

        //    if (!nudPosStoredX.Value.HasValue || !nudPosStoredY.Value.HasValue || !nudPosStoredZ.Value.HasValue)
        //        return;

        //    if (VM.Hook == null)
        //        return;

        //    VM.Hook.StableX = (float)nudPosStoredX.Value;
        //    VM.Hook.StableY = (float)nudPosStoredY.Value;
        //    VM.Hook.StableZ = (float)nudPosStoredZ.Value;
        //    VM.Hook.AngX = PlayerState.AngX;
        //    VM.Hook.AngY = PlayerState.AngY;
        //    VM.Hook.AngZ = PlayerState.AngZ;
        //    //Hook.CameraData = PlayerState.FollowCam;
        //    //Hook.CamX = CamX;
        //    //Hook.CamY = CamY;
        //    //Hook.CamZ = CamZ;
        //    if (cbxRestoreState.IsChecked == true)
        //    {
        //        nudHealth.Value = PlayerState.HP;
        //        nudStamina.Value = PlayerState.Stamina;
        //    }
        //}

        
        //private void cmbBonfirHub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (Hook == null)
        //        return;

        //    spBonfires.Children.Clear();
        //    var bonfireHub = cmbBonfirHub.SelectedItem as DS2SBonfireHub;
        //    if (bonfireHub == null)
        //        return;

        //    foreach (var bonfire in bonfireHub.Bonfires)
        //    {
        //        var bonfireControl = new LabelNudControl();
        //        Binding binding = new Binding("Value")
        //        {
        //            Source = Hook,
        //            Path = new PropertyPath(bonfire.Replace(" ", "").Replace("'", "").Replace("(", "").Replace(")", ""))
        //        };
        //        bonfireControl.nudValue.SetBinding(Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty, binding);
        //        bonfireControl.nudValue.Minimum = 0;
        //        bonfireControl.nudValue.Maximum = 99;
        //        bonfireControl.Label = bonfire;
        //        bonfireControl.nudValue.Margin = new Thickness(0, 5, 0, 0);
        //        spBonfires.Children.Add(bonfireControl);
        //    }
        //}
        
        

        internal override void UpdateCtrl() 
        {
            ////manage unknown warps and current warps that are not in filter
            //if (VM.Hook == null) return;
            //var bonfireID = VM.Hook.LastBonfireID;

            //if (LastSetBonfire == null)
            //    return;

            //if (LastSetBonfire.ID != bonfireID) // lastSetBonfire does not match game LastBonfire
            //{
            //    //target warp is not in filter
            //    var result = DS2Resource.Bonfires.FirstOrDefault(b => b.ID == bonfireID); //check if warp is in bonfire resource
            //    if (result == null)
            //    {
            //        //bonfire not in filter. Add to filter as unknown
            //        result = new DS2SBonfire(VM.Hook.LastBonfireAreaID ,bonfireID, $"Unknown {VM.Hook.LastBonfireAreaID}: {bonfireID}");
            //        DS2Resource.Bonfires.Add(result);
            //        FilterBonfires();
            //    }

            //    //manage lastSetBonfire
            //    cmbBonfire.Items.Remove(LastSetBonfire); //remove from filter (if there)

            //    LastSetBonfire.AreaID = result.AreaID;
            //    LastSetBonfire.ID = result.ID;
            //    LastSetBonfire.Name = "Last Set: " + result.Name;

            //    cmbBonfire.Items.Add(LastSetBonfire); //add to end of filter
            //    cmbBonfire.SelectedItem = LastSetBonfire;
            //    //AddLastSetBonfire();
            //}
        }
        //private void FilterBonfires()
        //{
        //    //warp filter management

        //    cmbBonfire.Items.Clear();
        //    cmbBonfire.SelectedItem = null;

        //    //go through bonfire resource and add to filter
        //    foreach (var bonfire in DS2Resource.Bonfires)
        //    {
        //        if (bonfire.ToString().ToLower().Contains(txtSearch.Text.ToLower()))
        //        {
        //            cmbBonfire.Items.Add(bonfire);
        //        }
        //    }

        //    cmbBonfire.Items.Add(LastSetBonfire); //add lastSetBonfire to end of filter

        //    cmbBonfire.SelectedIndex = 0;

        //    if (txtSearch.Text == "")
        //        lblSearch.Visibility = Visibility.Visible;
        //    else
        //        lblSearch.Visibility = Visibility.Hidden;
        //}
        
        
        // UI events
        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            StorePosition();
        }
        
        //private void cbxSpeed_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (VM.Hook == null) return;
        //    nudSpeed.IsEnabled = cbxSpeed.IsChecked == true;
        //    VM.Hook.Speedhack(cbxSpeed.IsChecked == true);
        //}
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //RemoveSavedPos();
        }
        
        private void StoredPositions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStoredPositions.SelectedItem is not SavedPos savedPos)
                return;

            nudPosStoredX.Value = (decimal)savedPos.X;
            nudPosStoredY.Value = (decimal)savedPos.Y;
            nudPosStoredZ.Value = (decimal)savedPos.Z;
            PlayerState = savedPos.PlayerState;
            txtAngX.Text = PlayerState.AngX.ToString("N2");
            txtAngY.Text = PlayerState.AngY.ToString("N2");
            txtAngZ.Text = PlayerState.AngZ.ToString("N2");
        }


        public void DeltaHeight(float delta)
        {
            if (VM.Hook == null) return;
            VM.Hook.PosZ += delta;

            // QOL: AutoTurn off gravity
            if (cbxGravity.IsChecked == true)
                cbxGravity.IsChecked = false;
        }
        public void FastQuit()
        {
            if (VM.Hook == null || !VM.Hook.Hooked)
                return;
            VM.Hook.FastQuit = 6;
        }
        
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            //FilterBonfires();
        }
        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                TxtSearch.Clear();

            KeyDownListbox(e);
        }
        private void KeyDownListbox(KeyEventArgs e)
        {
            //if (e.Key == Key.Down)
            //{
            //    e.Handled = true;

            //    if (cmbBonfire.SelectedIndex < cmbBonfire.Items.Count - 1)
            //    {
            //        cmbBonfire.SelectedIndex += 1;
            //        return;
            //    }
            //}

            //if (e.Key == Key.Up)
            //{
            //    e.Handled = true;

            //    if (cmbBonfire.SelectedIndex != 0)
            //    {
            //        cmbBonfire.SelectedIndex -= 1;
            //        return;
            //    }
            //}

            //if (e.Key == Key.Enter)
            //{
            //    e.Handled = true;
            //    Warp();
            //    return;
            //}
        }
        private void txtSearch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TxtSearch.SelectAll();
            TxtSearch.Focus();
            e.Handled=true;
        }
    }
}
