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
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SavingSettings()
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "Conan Exiles Server Admin.exe.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            if (txtServer.Text != "") config.AppSettings.Settings["Server"].Value = txtServer.Text.ToString();
            if (txtDatabase.Text != "") config.AppSettings.Settings["Database"].Value = txtDatabase.Text.ToString();
            if (txtUsername.Text != "") config.AppSettings.Settings["UserName"].Value = txtUsername.Text.ToString();
            if (passwordBox.ToString() != "")
            {
                var secure = new SecureString();
                foreach (char c in passwordBox.ToString())
                {
                    secure.AppendChar(c);
                }
                config.AppSettings.Settings["Password"].Value = GettingSettings.EncryptString(secure);
            }

            config.Save();
            GettingSettings Settings = new GettingSettings();
            Settings.SettingValuesFromConfig();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
