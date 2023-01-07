using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for metaHotkey.xaml
    /// </summary>
    public partial class HotkeyBoxControl : UserControl
    {
        public string HotkeyName
        {
            get { return (string)GetValue(HotkeyNameProperty); }
            set { SetValue(HotkeyNameProperty, value); }
        }
        public string? SettingsName { get; set; }
        private readonly Brush DefaultColor;

        // Using a DependencyProperty as the backing store for HotkeyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HotkeyNameProperty =
            DependencyProperty.Register("HotkeyName", typeof(string), typeof(HotkeyBoxControl), new PropertyMetadata(default));

        public HotkeyBoxControl()
        {
            InitializeComponent();
            DefaultColor = tbxHotkey.Background;
            tbxHotkey.MouseEnter += HotkeyTextBox_MouseEnter;
            tbxHotkey.MouseLeave += HotkeyTextBox_MouseLeave;
        }

        private void HotkeyTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            tbxHotkey.Background = DefaultColor;
        }
        private void HotkeyTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            tbxHotkey.Background = Brushes.LightGreen;
        }
    }
}
