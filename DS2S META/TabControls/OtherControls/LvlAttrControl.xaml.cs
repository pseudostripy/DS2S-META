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
    /// Interaction logic for LvlAttrControl.xaml
    /// </summary>
    public partial class LvlAttrControl : METAControl
    {
        public LvlAttrControl()
        {
            InitializeComponent();
        }

        public int StamTest 
        {   get => (int)(Hook?.DS2P?.PlayerState.Stamina ?? 5);
            set { return; }
        }

        public string AttrName
        {
            get => (string)GetValue(AttrNameProperty);
            set { SetValue(AttrNameProperty, value); }
        }

        // Can I bind to this?
        public static readonly DependencyProperty AttrNameProperty =
            DependencyProperty.Register("AttrName", typeof(string), typeof(LvlAttrControl), new PropertyMetadata(default));

        public int AttrLvl
        {
            get => (int)GetValue(AttrLvlProperty);
            set { SetValue(AttrLvlProperty, value); }
        }

        // Can I bind to this?
        public static readonly DependencyProperty AttrLvlProperty =
            DependencyProperty.Register("AttrLvl", typeof(int), typeof(LvlAttrControl), new PropertyMetadata(default));

        public string AttrClassMinLvl
        {
            get => (string)GetValue(AttrClassMinLvlProperty);
            set { SetValue(AttrClassMinLvlProperty, value); }
        }

        // Can I bind to this?
        public static readonly DependencyProperty AttrClassMinLvlProperty =
            DependencyProperty.Register("AttrClassMinLvl", typeof(int), typeof(LvlAttrControl), new PropertyMetadata(default));
    }
}
