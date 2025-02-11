﻿using System;
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
    public partial class MetaWarningWindow : Window
    {
        public MetaWarningWindow(string warnstr)
        {
            InitializeComponent();
            tbError.Text = warnstr;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        public static void ShowMetaWarning(string warnstr)
        {
            Application.Current.Dispatcher.Invoke(() => ShowMetaWarnWindow(warnstr));
        }
        private static void ShowMetaWarnWindow(string warnstr)
        {
            var mwWindow = new MetaWarningWindow(warnstr);
            mwWindow.ShowDialog();
        }
    }
}
