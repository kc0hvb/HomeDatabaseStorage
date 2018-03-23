using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SandiaAerospaceShipping
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
        }

        private void bttnOk_Click(object sender, RoutedEventArgs e)
        {
            InsertingDevice();
        }

        private void CloseOnEscape(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.Return)
                InsertingDevice();

        }

        private void InsertingDevice()
        {
            if (txtDevice.Text.ToString() != "" && txtDevice.Text.ToString() != null)
            {
                
                DatabaseProcedure.InsertingIntoComponents(txtDevice.Text.ToString().Replace(" ", "_"));
                this.Close();
            }
            else
                this.Close();
        }

        private void bttnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
