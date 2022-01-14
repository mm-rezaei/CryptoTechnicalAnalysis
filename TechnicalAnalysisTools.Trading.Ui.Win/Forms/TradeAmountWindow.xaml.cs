using System;
using System.Windows;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Forms
{
    internal partial class TradeAmountWindow : Window
    {
        public TradeAmountWindow()
        {
            InitializeComponent();

            ComboBoxSymbol.ItemsSource = Enum.GetValues(typeof(SymbolTypes));

            ComboBoxSymbol.SelectedIndex = 0;
        }

        private bool Result { get; set; } = false;

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            decimal amount;

            if (decimal.TryParse(TextBoxAmount.Text, out amount))
            {
                if (amount >= 0)
                {
                    Result = true;

                    Close();
                }
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;

            Close();
        }

        public Tuple<SymbolTypes, decimal> ShowDialogWindow()
        {
            Tuple<SymbolTypes, decimal> result = null;

            ShowDialog();

            if (Result)
            {
                result = new Tuple<SymbolTypes, decimal>((SymbolTypes)ComboBoxSymbol.SelectedItem, decimal.Parse(TextBoxAmount.Text));
            }

            return result;
        }
    }
}
