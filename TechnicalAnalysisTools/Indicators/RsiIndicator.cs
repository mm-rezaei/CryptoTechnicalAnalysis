using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class RsiIndicator : LengthIndicatorBase<decimal>
    {
        private SmoothedMovingAverageIndicator _gain;
        private SmoothedMovingAverageIndicator _loss;
        private bool _isInitialized;
        private decimal _last;

        public RsiIndicator()
        {
            _gain = new SmoothedMovingAverageIndicator();
            _loss = new SmoothedMovingAverageIndicator();

            Length = 15;
        }

        public override bool IsFormed => _gain.IsFormed;

        public override void Reset()
        {
            _loss.Length = _gain.Length = Length;

            base.Reset();
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var newValue = input.GetValue<decimal>();

            if (!_isInitialized)
            {
                if (input.IsFinal)
                {
                    _last = newValue;
                    _isInitialized = true;
                }

                return new DecimalIndicatorValue(this);
            }

            var delta = newValue - _last;

            var gainValue = _gain.Process(input.SetValue(this, delta > 0 ? delta : 0m)).GetValue<decimal>();
            var lossValue = _loss.Process(input.SetValue(this, delta > 0 ? 0m : -delta)).GetValue<decimal>();

            if (input.IsFinal)
            {
                _last = newValue;
            }

            if (Math.Round(lossValue, 16) == 0)
            {
                return new DecimalIndicatorValue(this, 100m);
            }

            if (Math.Round(gainValue, 16) / Math.Round(lossValue, 16) == 1)
            {
                return new DecimalIndicatorValue(this, 0m);
            }

            return new DecimalIndicatorValue(this, 100m - 100m / (1m + gainValue / lossValue));
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            SettingsStorage gainSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_gain)));
            SettingsStorage lossSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_loss)));

            _gain.Load(gainSettings);
            _loss.Load(lossSettings);

            _isInitialized = settings.GetValue<bool>(nameof(_isInitialized));
            _last = settings.GetValue<decimal>(nameof(_last));
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            SettingsStorage gainSettings = new SettingsStorage();
            SettingsStorage lossSettings = new SettingsStorage();

            _gain.Save(gainSettings);
            _loss.Save(lossSettings);

            settings.SetValue(nameof(_gain), SettingsStorageHelper.ToByteArray(gainSettings));
            settings.SetValue(nameof(_loss), SettingsStorageHelper.ToByteArray(lossSettings));

            settings.SetValue(nameof(_isInitialized), _isInitialized);
            settings.SetValue(nameof(_last), _last);
        }
    }
}
