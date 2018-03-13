using System;
using System.Collections.Generic;
using System.IO;
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
using System.Configuration;
using System.Security.Cryptography;
using System.Security;

namespace SandiaAerospaceShipping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dgComponent.CanUserAddRows = false;
            List<ComponentsList> items = new List<ComponentsList>();
            foreach (string sCom in Components())
            { 
                items.Add(new ComponentsList() { sComponent = sCom, iQuantity = 0 });
            }
            dgComponent.ItemsSource = items;
        }

        private List<string> Components()
        {
            List<string> lComponentList = new List<string> { "SR23", "SR24", "SR25"};
            return lComponentList;
        }

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void bttnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Window1 ServerConn = new Window1();
            ServerConn.Show();
        }
    }

    public class ComponentsList
    {
        public string sComponent { get; set; }

        public int iQuantity { get; set; }
    }

    public class GettingSettings
    {

        public string sServer { get; set; }
        public string sDatabaseName { get; set; }
        public string sUserName { get; set; }
        public SecureString sPassword { get; set; }


        public Configuration ConfigurationLocation()
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "SandiaAerospaceShipping.exe.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            return config;
        }

        public void SettingValuesFromConfig()
        {
            Configuration config = ConfigurationLocation();
            try
            {
                sServer = config.AppSettings.Settings["Server"].Value.ToString();
                sDatabaseName = config.AppSettings.Settings["Database"].Value.ToString();
                sUserName = config.AppSettings.Settings["UserName"].Value.ToString();
                sPassword = DecryptString(config.AppSettings.Settings["Password"].Value.ToString());
            }
            catch
            {
            }
        }


        #region Password Protecting
        static byte[] entropy = Encoding.Unicode.GetBytes("Salt Is TOO A Password");

        public static string EncryptString(SecureString input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    DataProtectionScope.CurrentUser);
                return ToSecureString(Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
        #endregion

    }
}
