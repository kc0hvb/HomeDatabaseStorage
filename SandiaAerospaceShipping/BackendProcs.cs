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
        
        public static string _sServer { get; set; }
        public static string _sDatabaseName { get; set; }
        public static string _sUserName { get; set; }
        public static SecureString _sPassword { get; set; }
        public static Configuration ConfigurationLocation()
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "SandiaAerospaceShipping.exe.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            return config;
        }
        public static void SettingValuesFromConfig()
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
