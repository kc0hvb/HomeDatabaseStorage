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
                    string sqlConn = BuildingConnectionString();
                    DatabaseProcedure.BuildingTable(sqlConn);
                    DatabaseProcedure.BuildingColumns(sqlConn);
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message.ToString()); }
            }
            dgComponent.CanUserAddRows = false;
            AddingComponentsToGrid();
        }
        public void AddingComponentsToGrid()
        {
            List<ComponentsList> items = new List<ComponentsList>();
            try
            {
                foreach (string sCom in Components())
                {
                    items.Add(new ComponentsList() { sComponent = sCom, iQuantity = 0 });
                }
                MyCollectionList = items;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            MyCollection = new ObservableCollection<ComponentsList>(MyCollectionList);

            dgComponent.ItemsSource = MyCollection;
        }

        public static List<string> Components()
        {
            List<string> lComponentList = new List<string> { "SR23", "SR24", "SR25" };
            return lComponentList;
        }

        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void bttnSave_Click(object sender, RoutedEventArgs e)
        {
            InsertingIntoDB();
            MyCollection = null;
            AddingComponentsToGrid();
            dgComponent.Items.Refresh();
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
            try
            {
                Columns = "Company, Shipping_Date, ";
                Values = "'" + txtCompany.Text.ToString() + "', '" + dtShipDate.ToString() + "', ";

                foreach (var item in MyCollection)
                {
                    Columns += item.sComponent + ", ";
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
        public string BuildingConnectionString()
        {
            GettingSettings Settings = new GettingSettings();
            GettingSettings.SettingValuesFromConfig();
            string sDecryptedPword = Password.ToInsecureString(GettingSettings._sPassword);
            string sqlConn = $"Server = {GettingSettings._sServer}; Database = {GettingSettings._sDatabaseName}; User Id = {GettingSettings._sUserName}; Password = {sDecryptedPword};";
            return sqlConn;
        }

        public void InsertingIntoDB()
        {
            string sSqlConnString = BuildingConnectionString();
            SqlConnection SqlCon = new SqlConnection(sSqlConnString);
            try
            {
                SqlCon.Open();
                SqlCommand SQLCom = new SqlCommand(InsertQuery(), SqlCon);
                SQLCom.ExecuteNonQuery();
                SqlCon.Close();

            }
            catch(Exception ex) { MessageBox.Show(ex.Message.ToString()); }
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
                    items.Add(new ComponentsList() { sComponent = sCom, iQuantity = 0 });
                }
                MyCollectionList = items;
            }
            catch { }
        }

        public static void BuildingTable(string pSQLConn)
        {
            string CommandText = "IF (NOT EXISTS (SELECT * " +
                                 "FROM INFORMATION_SCHEMA.TABLES " +
                                 "WHERE TABLE_SCHEMA = 'dbo' " +
                                 "AND TABLE_NAME = 'Shipping_Log')) " +
                                 "BEGIN " +
                                 "CREATE TABLE[dbo].[Shipping_Log]([Log_ID][int] IDENTITY(1, 1) NOT NULL PRIMARY KEY NONCLUSTERED " +
                                 "([Log_ID] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, " +
                                 "IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] END";
            SqlConnection SQLConn = new SqlConnection(pSQLConn);
            SqlCommand SQLCom = new SqlCommand(CommandText, SQLConn);
            SQLConn.Open();
            SQLCom.ExecuteNonQuery();
            SQLConn.Close();

        }

        public static void BuildingColumns(string pSQLConn)
        {
            FillingOutList();
            string SQLQuery = string.Empty;
            InsertingColumns(pSQLConn, "Company", "varchar(255)");
            InsertingColumns(pSQLConn, "Shipping_Date", "datetime");
            foreach (var item in MyCollectionList)
            {
                InsertingColumns(pSQLConn, item.sComponent.ToString(), "int");
            }
        }
        public static void InsertingColumns(string pSQLConn, string pColumn, string pFType)
        {
            string SQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'{0}') " +
                                     "BEGIN ALTER TABLE Shipping_Log ADD {0} {1}; END", pColumn, pFType);
            SqlConnection SQLConn = new SqlConnection(pSQLConn);
            SqlCommand SQLCom = new SqlCommand(SQLQuery, SQLConn);
            SQLConn.Open();
            SQLCom.ExecuteNonQuery();
            SQLConn.Close();
        }

    }

    public class ComponentsList
    {
        public string sComponent { get; set; }

        public int iQuantity { get; set; }
    }
}

    
