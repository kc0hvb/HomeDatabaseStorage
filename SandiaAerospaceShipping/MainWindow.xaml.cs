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
using System.Reflection;
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
        #region Static Members
        private List<ComponentsList> MyCollectionList { get; set; }
        private static ObservableCollection<ComponentsList> MyCollection { get; set; }
        private static DatabaseProcedure dbProc = new DatabaseProcedure();
        private static int iLogID { get; set; }
        private static string sCompany { get; set; }
        #endregion
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
            FillingMainDataGrid(false);
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
                FillingMainDataGrid(true));
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
                DataTable dtComponents = DatabaseProcedure.GettingInfoFromDatabase("SELECT Components FROM Components ORDER BY Components;");
                foreach(DataRow row in dtComponents.Rows)
                {
                    items.Add(new ComponentsList() { sComponent = row["Components"].ToString().Replace("_", " "), iQuantity = 0 });
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
            string[] lsShipping = { "UPS", "FedEx", "USPS", "DHL" };
            foreach (string item in lsShipping)
            {
                DropList.Add(item);
            }
            this.cbShippingCompany.ItemsSource = DropList;
        }
        public static List<string> Components()
        {
            List<string> lComponentList = new List<string> { "Safe_128", "Safe_328", "Safe_528", "ACF_314", "ACF_328", "ACF_528",
                                                            "SRU_1", "SRU_5","SRU_5_Mod","SRU_10", "ST_26", "ST_32_00", "ST_32_01",
                                                            "AIS_200B_35", "AIS_240B_35", "SA_3", "SA_3_L", "SA_3_NVG", "SA_15", "SA_24",
                                                            "SR_34_1", "SR_54_1", "SR_64_1", "SR_263", "SR_623", "GI_205", "STX_165",
                                                            "STX_165_Remote", "SAE_5_35", "360_PM", "360_Remote", "KI_300", "SAI_340",
                                                            "305477_00_Kit", "ADC_00", "ADC_01", "Panel_Punch", "ST_32_Kit", "Oat_Probe", };
            return lComponentList;
        }
        #endregion


        
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
            FillingMainDataGrid(true);
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
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

        private void FillingMainDataGrid(bool pRefresh)
        {
            try
            {
                DataTable dtInfo = new DataTable();
                string sQuery = DatabaseProcedure.OrderingColumns();
                dtInfo = DatabaseProcedure.GettingInfoFromDatabase(sQuery);
                dataGrid.ItemsSource = dtInfo.DefaultView;
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message.ToString());
            }
        }

        #region Textbox Events
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
        #endregion

        #region Button Functions
        private void bttnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FillingMainDataGrid(true);
        }
        private void bttnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            Window2 AddDevice = new Window2();
            AddDevice.ShowDialog();
            AddingComponentsToGrid();
            FillingMainDataGrid(true);
        }
        private void bttnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("This will delete LogID: {0} for Company: {1}?", iLogID, sCompany),  "Delete Shipment Log", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DatabaseProcedure.DeletingFromLog(iLogID);
                FillingMainDataGrid(true);
            }
            else
            {
                //No Stuff
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Window1 ServerConn = new Window1();
            ServerConn.Show();
        }
        #endregion

        #region Selection Change Events
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            iLogID = Int32.Parse((drv["Log_ID"]).ToString());
            sCompany = (drv["Company"]).ToString();
        }
        private void lbComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion


    }



    public class ComponentsList
    {
        public string sComponent { get; set; }

        public int iQuantity { get; set; }
    }
}

    
