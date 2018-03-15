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
            
            InitializeComponent();
            GettingSettings.SettingValuesFromConfig();
            FillingOutTxtBox();
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SavingSettings();
            GettingSettings.SettingValuesFromConfig();
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SavingSettings()
        {
            Configuration config = GettingSettings.ConfigurationLocation();

            if (txtServer.Text != "") config.AppSettings.Settings["Server"].Value = txtServer.Text.ToString();
            if (txtDatabase.Text != "") config.AppSettings.Settings["Database"].Value = txtDatabase.Text.ToString();
            if (txtUsername.Text != "") config.AppSettings.Settings["UserName"].Value = txtUsername.Text.ToString();
            if (passwordBox.Password.ToString() != "")
            {
                var secure = new SecureString();
                foreach (char c in passwordBox.Password.ToString())
                {
                    secure.AppendChar(c);
                }
                config.AppSettings.Settings["Password"].Value = Password.EncryptString(secure);
            }

            config.Save();
        }

        public void FillingOutTxtBox()
        {
            GettingSettings.SettingValuesFromConfig();
            if (GettingSettings._sServer != "" && GettingSettings._sServer != null) txtServer.Text = GettingSettings._sServer.ToString();
            if (GettingSettings._sDatabaseName != "" && GettingSettings._sUserName != null) txtDatabase.Text = GettingSettings._sDatabaseName.ToString();
            if (GettingSettings._sUserName != "" && GettingSettings._sUserName != null) txtUsername.Text = GettingSettings._sUserName.ToString();
            if (GettingSettings._sPassword != null) passwordBox.Password = Password.ToInsecureString(GettingSettings._sPassword);
        }
    }
}
