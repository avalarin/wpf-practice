using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Payments.xaml.windows {
    /// <summary>
    /// Interaction logic for CreatePaymentWindow.xaml
    /// </summary>
    public partial class CreatePaymentWindow : Window {
        public CreatePaymentWindow() {
            InitializeComponent();
        }

        private void OnAmountInput(object sender, TextCompositionEventArgs e) {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
