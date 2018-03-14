using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Security;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace SandiaAerospaceShipping
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        
        public Window1()
        {
            GettingSettings Settings = new GettingSettings();
            InitializeComponent();
            Settings.SettingValuesFromConfig();
            Settings.FillingOutTxtBox();
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            GettingSettings Settings = new GettingSettings();
            Settings.SavingSettings();
            Settings.SettingValuesFromConfig();
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
