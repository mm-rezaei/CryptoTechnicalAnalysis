using System.Linq;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class SmoothedMovingAverageIndicator : LengthIndicatorBase<decimal>
    {
        private decimal _prevFinalValue;

        public SmoothedMovingAverageIndicator()
        {
            Length = 32;
        }

        public override void Reset()
        {
            _prevFinalValue = 0;
            base.Reset();
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var newValue = input.GetValue<decimal>();

            if (!IsFormed)
            {
                if (input.IsFinal)
                {
                    Buffer.Add(newValue);

                    _prevFinalValue = Buffer.Sum() / Length;

                    return new DecimalIndicatorValue(this, _prevFinalValue);
                }

                return new DecimalIndicatorValue(this, (Buffer.Skip(1).Sum() + newValue) / Length);
            }

            var curValue = (_prevFinalValue * (Length - 1) + newValue) / Length;

            if (input.IsFinal)
                _prevFinalValue = curValue;

            return new DecimalIndicatorValue(this, curValue);
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            _prevFinalValue = settings.GetValue<decimal>(nameof(_prevFinalValue));
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            settings.SetValue(nameof(_prevFinalValue), _prevFinalValue);
        }
    }
}
