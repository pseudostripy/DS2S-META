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
    /// Interaction logic for RandoWarpWarning.xaml
    /// </summary>
    public partial class MetaExceptionWindow : Window
    {
        private readonly string ExceptionStr = "TEMP";
        public MetaExceptionWindow(string exceptionStr)
        {
            InitializeComponent();
            ExceptionStr = $"{exceptionStr}{Environment.NewLine}{Environment.NewLine}Dumped to <METADIR>\\log.txt";
            tbError.Text = ExceptionStr;
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
