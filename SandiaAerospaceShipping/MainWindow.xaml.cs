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
using System.Data;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Timers;
using System.Text.RegularExpressions;

namespace SandiaAerospaceShipping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ComponentsList> MyCollectionList { get; set; }
        private static ObservableCollection<ComponentsList> MyCollection { get; set; }
        private static DatabaseProcedure dbProc = new DatabaseProcedure();
        public MainWindow()
        {
            InitializeComponent();
            GettingSettings.SettingValuesFromConfig();
            if (GettingSettings._sServer != null && GettingSettings._sServer != "")
            {
                try
                {
                    string sqlConn = DatabaseProcedure.BuildingConnectionString();
                    if (sqlConn != "")
                    {
                        DatabaseProcedure.BuildingDatabase(sqlConn);
                        DatabaseProcedure.BuildingTable(sqlConn);
                        DatabaseProcedure.BuildingColumns(sqlConn);
                    }
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message.ToString()); }
            }
            dgComponent.CanUserAddRows = false;
            dataGrid.CanUserAddRows = false;
            dtShipDate.Text = DateTime.Now.ToString();
            AddingItemsToDropDownList();
            AddingComponentsToGrid();
            FillingMainDataGrid();
            //StartingTimertoRefresh();
        }

        private void StartingTimertoRefresh()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 3600000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                FillingMainDataGrid());
                this.Dispatcher.Invoke(() =>
                dtShipDate.Text = DateTime.Now.ToString());
            }
            catch { }
        }

        #region Adding Items to the UI
        public void AddingComponentsToGrid()
        {
            MyCollection = null;
            List<ComponentsList> items = new List<ComponentsList>();
            try
            {
                foreach (string sCom in Components())
                {
                    items.Add(new ComponentsList() { sComponent = sCom.Replace("_", " "), iQuantity = 0 });
                }
                MyCollectionList = items;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            MyCollection = new ObservableCollection<ComponentsList>(MyCollectionList);

            dgComponent.ItemsSource = MyCollection;
        }

        private void AddingItemsToDropDownList()
        {
            ObservableCollection<string> DropList = new ObservableCollection<string>();
            string[] lsShipping = { "UPS", "FedEx", "USPS" };
            foreach (string item in lsShipping)
            {
                DropList.Add(item);
            }
            this.cbShippingCompany.ItemsSource = DropList;
        }

        public static List<string> Components()
        {
            List<string> lComponentList = new List<string> { "Safe_128", "Safe_328", "Safe_5285", "ACF_314", "ACF_328", "ACF_528",
                                                            "SRU_1", "SRU_5","SRU_5_Mod","SRU_10", "ST_26", "ST_32_00", "ST_32_01",
                                                            "AIS_200B_35", "AIS_240B_35", "SA_3", "SA_3_L", "SA_3_NVG", "SA_15", "SA_24",
                                                            "SR_34_1", "SR_54_1", "SR_64_1", "SR_263", "SR_623", "GI_205", "STX_165",
                                                            "STX_165_Remote", "SAE_5_35", "360_PM", "360_Remote", "KI_300", "SAI_340",
                                                            "305477_00_Kit"};
            return lComponentList;
        }
        #endregion

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void bttnSave_Click(object sender, RoutedEventArgs e)
        {
            dtShipDate.Text = DateTime.Now.ToString();
            DatabaseProcedure.InsertingIntoDB(InsertQuery());
            MyCollection = null;
            txtCompany.Text = "";
            cbShippingCompany.Text = "";
            chckbRepair.IsChecked = false;
            txtCost.Text = "0";
            AddingComponentsToGrid();
            FillingMainDataGrid();
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            Window1 ServerConn = new Window1();
            ServerConn.Show();
        }
        public string InsertQuery()
        {
            string sRet = string.Empty;
            string Columns = string.Empty;
            string Values = string.Empty;
            int isRepair = 0;
            if (chckbRepair.IsChecked == true) { isRepair = 1; }
            try
            {
                Columns = "Company, Shipping_Date, Shipping_Company, Cost, Repair, ";
                Values = "'" + txtCompany.Text.ToString() + "', '" + dtShipDate.ToString() + "', '" + cbShippingCompany.Text.ToString() + "', " + txtCost.Text.ToString() + ", " + isRepair + ", ";

                foreach (var item in MyCollection)
                {
                    if (Regex.IsMatch(item.sComponent, @"^\d"))
                        item.sComponent = '"' + item.sComponent + '"';
                    Columns += item.sComponent.Replace(" ", "_") + ", ";
                    Values += item.iQuantity + ", ";
                }
                Columns = (Columns.Trim()).TrimEnd(',');
                Values = (Values.Trim()).TrimEnd(',');

                sRet = string.Format("INSERT INTO Shipping_Log({0}) VALUES ({1});", Columns, Values);

            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message.ToString()); }
            return sRet;
        }

        private void FillingMainDataGrid()
        {
            try
            {
                string sQuery = "SELECT * FROM Shipping_Log";
                DataTable dtInfo = DatabaseProcedure.GettingInfoFromDatabase(sQuery);
                dataGrid.ItemsSource = dtInfo.DefaultView;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }

        private void txtCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(txtCost.Text, "[^0-9]+");
        }

        private void txtCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void bttnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FillingMainDataGrid();
        }
    }

    public class DatabaseProcedure
    {
        private static List<ComponentsList> MyCollectionList { get; set; }
        private static void FillingOutList()
        {
            List<ComponentsList> items = new List<ComponentsList>();
            try
            {
                foreach (string sCom in MainWindow.Components())
                {
                    items.Add(new ComponentsList() { sComponent = sCom.Replace("_", " "), iQuantity = 0 });
                }
                MyCollectionList = items;
            }
            catch { }
        }

        public static string BuildingConnectionString()
        {
            string sqlConn = string.Empty;
            if ((GettingSettings._sServer != "" && GettingSettings._sServer != null) &&
                    (GettingSettings._sDatabaseName != "" && GettingSettings._sDatabaseName != null) &&
                    (GettingSettings._sUserName != "" && GettingSettings._sUserName != null) &&
                    (GettingSettings._sPassword != null))
            {
                GettingSettings Settings = new GettingSettings();
                GettingSettings.SettingValuesFromConfig();
                string sDecryptedPword = Password.ToInsecureString(GettingSettings._sPassword);
                sqlConn = $"Server = {GettingSettings._sServer}; Database = {GettingSettings._sDatabaseName}; User Id = {GettingSettings._sUserName}; Password = {sDecryptedPword};";
            }
            return sqlConn;
        }

        public static void BuildingDatabase(string pSQLConn)

        {

            string sSqlConnString = BuildingConnectionString();
            string sSQLQuery = string.Format("DECLARE @dbname nvarchar(128) " +
                                             "SET @dbname = N'{0}' " +
                                             "IF(NOT EXISTS(SELECT name " +
                                             "FROM master.dbo.sysdatabases " +
                                             "WHERE('[' + name + ']' = @dbname " +
                                             "OR name = @dbname))) CREATE DATABASE {0}", GettingSettings._sDatabaseName);
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString.Replace(GettingSettings._sDatabaseName, "master"));
                try
                {
                    SqlCon.Open();
                    SqlCommand SQLCom = new SqlCommand(sSQLQuery, SqlCon);
                    SQLCom.ExecuteNonQuery();
                    SqlCon.Close();

                }

                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
        }
        public static void BuildingTable(string pSQLConn)
        {
            try
            {
                string sSQLQuery = "IF (NOT EXISTS (SELECT * " +
                                     "FROM INFORMATION_SCHEMA.TABLES " +
                                     "WHERE TABLE_SCHEMA = 'dbo' " +
                                     "AND TABLE_NAME = 'Shipping_Log')) " +
                                     "BEGIN " +
                                     "CREATE TABLE[dbo].[Shipping_Log]([Log_ID][int] IDENTITY(1, 1) NOT NULL PRIMARY KEY NONCLUSTERED " +
                                     "([Log_ID] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, " +
                                     "IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] END";
                InsertingIntoDB(sSQLQuery);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public static void BuildingColumns(string pSQLConn)
        {
            FillingOutList();
            string SQLQuery = string.Empty;
            InsertingColumns(pSQLConn, "Company", "varchar(255)");
            InsertingColumns(pSQLConn, "Shipping_Date", "datetime");
            InsertingColumns(pSQLConn, "Cost", "int");
            InsertingColumns(pSQLConn, "Shipping_Company", "varchar(255)");
            InsertingColumns(pSQLConn, "Repair", "bit");
            foreach (var item in MyCollectionList)
            {
                InsertingColumns(pSQLConn, item.sComponent.Replace(" ", "_"), "int");
            }
        }
        public static void InsertingColumns(string pSQLConn, string pColumn, string pFType)
        {
            try
            {
                string SQLQuery = "";
                if (Regex.IsMatch(pColumn, @"^\d"))
                {
                    string sAddingColumn = '"' + pColumn + '"';
                    SQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = '{0}') " +
                                     "BEGIN ALTER TABLE Shipping_Log ADD {1} {2}; END", pColumn, sAddingColumn, pFType);
                }
                else
                    SQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = '{0}') " +
                                         "BEGIN ALTER TABLE Shipping_Log ADD {0} {1}; END", pColumn, pFType);
                InsertingIntoDB(SQLQuery);
            }
            catch
            {

            }
            }
        public static void InsertingIntoDB(string pQuery)
        {
            string sSqlConnString = BuildingConnectionString();
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString);
                try
                {
                    SqlCon.Open();
                    SqlCommand SQLCom = new SqlCommand(pQuery, SqlCon);
                    SQLCom.ExecuteNonQuery();
                    SqlCon.Close();

                }

                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
        }
        public static DataTable GettingInfoFromDatabase(string pQuery)
        {
            DataTable dtInfo = new DataTable();
            string sSqlConnString = BuildingConnectionString();
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString);
                try
                {
                    SqlCommand SQLCom = new SqlCommand(pQuery, SqlCon);
                    SqlDataAdapter sda = new SqlDataAdapter(SQLCom);
                    sda.Fill(dtInfo);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
            return dtInfo;
        }

    }

    public class ComponentsList
    {
        public string sComponent { get; set; }

        public int iQuantity { get; set; }
    }
}

    
