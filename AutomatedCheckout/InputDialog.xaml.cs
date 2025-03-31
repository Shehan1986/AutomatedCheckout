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

namespace AutomatedCheckout
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public double Value { get; private set; }

        public InputDialog(string promptText = "Enter Value")
        {
            InitializeComponent();
            txbPrompt.Text = promptText;
            txtValue.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = double.TryParse(txtValue.Text, out double value) && value > 0;
            if (isValid)
            {
                Value = value;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}