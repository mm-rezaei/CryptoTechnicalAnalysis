using Ecng.Serialization;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class StochasticKIndicator : LengthIndicatorBase<decimal>
    {
        private LowestIndicator _low = new LowestIndicator();

        private HighestIndicator _high = new HighestIndicator();

        public StochasticKIndicator()
        {
            Length = 14;
        }

        public override bool IsFormed => _high.IsFormed;

        public override void Reset()
        {
            _high.Length = _low.Length = Length;
            base.Reset();
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var candle = input.GetValue<Candle>();

            var highValue = _high.Process(input.SetValue(this, candle.HighPrice)).GetValue<decimal>();
            var lowValue = _low.Process(input.SetValue(this, candle.LowPrice)).GetValue<decimal>();

            var diff = highValue - lowValue;

            if (diff == 0)
                return new DecimalIndicatorValue(this, 0);

            return new DecimalIndicatorValue(this, 100 * (candle.ClosePrice - lowValue) / diff);
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
