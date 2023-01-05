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
    /// Interaction logic for BonfireControl.xaml
    /// </summary>
    public partial class LabelNudControl : UserControl
    {

        public string Label
        {
            get { return (string)GetValue(BonfireNameProperty); }
            set { SetValue(BonfireNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BonfireName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BonfireNameProperty =
            DependencyProperty.Register("BonfireName", typeof(string), typeof(LabelNudControl), new PropertyMetadata(default));

        public byte BonfireLevel
        {
            get { return (byte)GetValue(BonfireLevelProperty); }
            set { SetValue(BonfireLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BonfireLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BonfireLevelProperty =
            DependencyProperty.Register("BonfireLevel", typeof(byte), typeof(LabelNudControl), new PropertyMetadata(default));

        public LabelNudControl()
        {
            InitializeComponent();
        }
    }
}
