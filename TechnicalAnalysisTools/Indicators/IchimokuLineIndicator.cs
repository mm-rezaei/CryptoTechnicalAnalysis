using System.Collections.Generic;
using System.Linq;
using Ecng.Serialization;
using MoreLinq;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuLineIndicator : LengthIndicatorBase<decimal>
    {
        private List<Candle> _buffer = new List<Candle>();

        public IchimokuLineIndicator()
        {
        }

        public override void Reset()
        {
            base.Reset();
            _buffer.Clear();
        }

        public override bool IsFormed => _buffer.Count >= Length;

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var candle = input.GetValue<Candle>();
            var buff = _buffer;

            if (input.IsFinal)
            {
                _buffer.Add(candle);

                if (_buffer.Count > Length)
                    _buffer.RemoveAt(0);
            }
            else
                buff = _buffer.Skip(1).Concat(candle).ToList();

            if (IsFormed)
            {
                var max = buff.Max(t => t.HighPrice);
                var min = buff.Min(t => t.LowPrice);

                return new DecimalIndicatorValue(this, (max + min) / 2);
            }

            return new DecimalIndicatorValue(this);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            settings.SetValue(nameof(_buffer), TimeFrameCandleHelper.CandleArrayToByteArray(_buffer.Select(p=>(TimeFrameCandle)p).ToArray()));
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            _buffer = ((Candle[])TimeFrameCandleHelper.CandleArrayFromByteArray(settings.GetValue<byte[]>(nameof(_buffer)))).ToList();
        }
    }
}
