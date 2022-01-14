using Ecng.Serialization;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class WilliamsRIndicator : LengthIndicatorBase<decimal>
    {
        private readonly LowestIndicator _low;

        private readonly HighestIndicator _high;

        public WilliamsRIndicator()
        {
            _low = new LowestIndicator();
            _high = new HighestIndicator();
        }

        public override bool IsFormed => _low.IsFormed;

        public override void Reset()
        {
            _high.Length = _low.Length = Length;
            base.Reset();
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var candle = input.GetValue<Candle>();

            var lowValue = _low.Process(input.SetValue(this, candle.LowPrice)).GetValue<decimal>();
            var highValue = _high.Process(input.SetValue(this, candle.HighPrice)).GetValue<decimal>();

            if ((highValue - lowValue) != 0)
                return new DecimalIndicatorValue(this, -100m * (highValue - candle.ClosePrice) / (highValue - lowValue));

            return new DecimalIndicatorValue(this);
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            SettingsStorage lowSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_low)));
            SettingsStorage highSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_high)));

            _low.Load(lowSettings);
            _high.Load(highSettings);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            SettingsStorage lowSettings = new SettingsStorage();
            SettingsStorage highSettings = new SettingsStorage();

            _low.Save(lowSettings);
            _high.Save(highSettings);

            settings.SetValue(nameof(_low), SettingsStorageHelper.ToByteArray(lowSettings));
            settings.SetValue(nameof(_high), SettingsStorageHelper.ToByteArray(highSettings));
        }
    }
}
