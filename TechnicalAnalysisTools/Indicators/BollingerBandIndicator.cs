using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class BollingerBandIndicator : IndicatorBase
    {
        private readonly LengthIndicatorBase<decimal> _ma;
        private readonly StandardDeviationIndicator _dev;

        public BollingerBandIndicator(LengthIndicatorBase<decimal> ma, StandardDeviationIndicator dev)
        {
            _ma = ma ?? throw new ArgumentNullException(nameof(ma));
            _dev = dev ?? throw new ArgumentNullException(nameof(dev));
        }

        public decimal Width { get; set; }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            return new DecimalIndicatorValue(this, _ma.GetCurrentValue() + (Width * _dev.GetCurrentValue()));
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);
            Width = settings.GetValue<decimal>(nameof(Width));

            SettingsStorage maSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_ma)));
            SettingsStorage devSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_dev)));

            _ma.Load(maSettings);
            _dev.Load(devSettings);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);
            settings.SetValue(nameof(Width), Width);

            SettingsStorage maSettings = new SettingsStorage();
            SettingsStorage devSettings = new SettingsStorage();

            _ma.Save(maSettings);
            _dev.Save(devSettings);

            settings.SetValue(nameof(_ma), SettingsStorageHelper.ToByteArray(maSettings));
            settings.SetValue(nameof(_dev), SettingsStorageHelper.ToByteArray(devSettings));
        }
    }
}
