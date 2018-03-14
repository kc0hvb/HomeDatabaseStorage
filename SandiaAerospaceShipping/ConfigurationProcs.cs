using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;

namespace SandiaAerospaceShipping
{
    public class GettingSettings
    {

        Window1 ServerConnForm = new Window1();
        public string _sServer { get; set; }
        public string _sDatabaseName { get; set; }
        public string _sUserName { get; set; }
        public SecureString _sPassword { get; set; }
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
                _sServer = config.AppSettings.Settings["Server"].Value.ToString();
                _sDatabaseName = config.AppSettings.Settings["Database"].Value.ToString();
                _sUserName = config.AppSettings.Settings["UserName"].Value.ToString();
                _sPassword = Password.DecryptString(config.AppSettings.Settings["Password"].Value.ToString());
            }
            catch
            {
            }
        }
        public void SavingSettings()
        {
            Configuration config = ConfigurationLocation();

            if (ServerConnForm.txtServer.Text != "") config.AppSettings.Settings["Server"].Value = ServerConnForm.txtServer.Text.ToString();
            if (ServerConnForm.txtDatabase.Text != "") config.AppSettings.Settings["Database"].Value = ServerConnForm.txtDatabase.Text.ToString();
            if (ServerConnForm.txtUsername.Text != "") config.AppSettings.Settings["UserName"].Value = ServerConnForm.txtUsername.Text.ToString();
            if (ServerConnForm.passwordBox.Password.ToString() != "")
            {
                var secure = new SecureString();
                foreach (char c in ServerConnForm.passwordBox.Password.ToString())
                {
                    secure.AppendChar(c);
                }
                config.AppSettings.Settings["Password"].Value = Password.EncryptString(secure);
            }

            config.Save();
        }

        public void FillingOutTxtBox()
        {
            ServerConnForm.txtServer.Text = _sServer.ToString();
            ServerConnForm.txtDatabase.Text = _sDatabaseName.ToString(); ;
            ServerConnForm.txtUsername.Text = _sUserName.ToString();
        }
    }
    public class Password
    {
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
