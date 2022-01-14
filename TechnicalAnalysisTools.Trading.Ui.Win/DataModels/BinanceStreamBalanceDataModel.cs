using System.ComponentModel;

namespace TechnicalAnalysisTools.Trading.Ui.Win.DataModels
{
    internal class BinanceStreamBalanceDataModel : INotifyPropertyChanged
    {
#pragma warning disable CS0649
        private string asset;
        private decimal free;
        private decimal locked;
        private decimal total;
#pragma warning restore CS0649

        private void SetPropertyValue(string fieldName, string propertyName, object value)
        {
            typeof(BinanceStreamBalanceDataModel).GetField(fieldName).SetValue(this, value);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Asset { get => asset; set => SetPropertyValue(nameof(asset), nameof(Asset), value); }

        public decimal Free { get => free; set => SetPropertyValue(nameof(free), nameof(Free), value); }

        public decimal Locked { get => locked; set => SetPropertyValue(nameof(locked), nameof(Locked), value); }

        public decimal Total { get => total; set => SetPropertyValue(nameof(total), nameof(Total), value); }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
