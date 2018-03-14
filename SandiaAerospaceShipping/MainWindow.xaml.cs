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


}
