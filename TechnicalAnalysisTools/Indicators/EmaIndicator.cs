using System.Linq;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class EmaIndicator : LengthIndicatorBase<decimal>
    {
        private decimal _prevFinalValue;
        private decimal _multiplier = 1;

        public EmaIndicator()
        {
            Length = 32;
        }

        public override void Reset()
        {
            base.Reset();
            _multiplier = 2m / (Length + 1);
            _prevFinalValue = 0;
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
                else
                {
                    return new DecimalIndicatorValue(this, (Buffer.Skip(1).Sum() + newValue) / Length);
                }
            }
            else
            {
                var curValue = (newValue - _prevFinalValue) * _multiplier + _prevFinalValue;

                if (input.IsFinal)
                    _prevFinalValue = curValue;

                return new DecimalIndicatorValue(this, curValue);
            }
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            settings.SetValue(nameof(_prevFinalValue), _prevFinalValue);
            settings.SetValue(nameof(_multiplier), _multiplier);
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            _prevFinalValue = settings.GetValue<decimal>(nameof(_prevFinalValue));
            _multiplier = settings.GetValue<decimal>(nameof(_multiplier));
        }
    }
}
