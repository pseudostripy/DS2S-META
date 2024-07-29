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

namespace DS2S_META.TabControls.OtherControls
{
    /// <summary>
    /// Interaction logic for BonfireNudControl.xaml
    /// </summary>
    public partial class BonfireNudControl : UserControl
    {
        public DS2SBonfire AssociatedBonfire;

        public BonfireNudControl(DS2SBonfire associatedBonfire)
        {
            InitializeComponent();
            AssociatedBonfire = associatedBonfire;
        }
    }
}
