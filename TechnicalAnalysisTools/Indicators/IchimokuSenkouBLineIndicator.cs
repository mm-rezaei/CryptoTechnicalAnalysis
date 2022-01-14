using System;
using System.Collections.Generic;
using System.Linq;
using Ecng.Serialization;
using MoreLinq;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuSenkouBLineIndicator : LengthIndicatorBase<decimal>
    {
        private List<Candle> _buffer = new List<Candle>();

        public IchimokuSenkouBLineIndicator(IchimokuLineIndicator kijun)
        {
            Kijun = kijun ?? throw new ArgumentNullException(nameof(kijun));
        }

        public override void Reset()
        {
            base.Reset();
            _buffer.Clear();
        }

        public override bool IsFormed => _buffer.Count >= Length && Buffer.Count >= Kijun.Length;

        public IchimokuLineIndicator Kijun { get; }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var candle = input.GetValue<Candle>();

            decimal? result = null;
            var buff = _buffer;

            if (input.IsFinal)
            {
                _buffer.Add(candle);

                if (_buffer.Count > Length)
                {
                    _buffer.RemoveAt(0);
                }
            }
            else
            {
                buff = _buffer.Skip(1).Concat(candle).ToList();
            }

            if (buff.Count >= Length)
            {
                var max = buff.Max(t => t.HighPrice);
                var min = buff.Min(t => t.LowPrice);

                if (Kijun.IsFormed && input.IsFinal)
                {
                    Buffer.Add((max + min) / 2);
                }

                if (Buffer.Count > Kijun.Length)
                {
                    Buffer.RemoveAt(0);
                }

                if (Buffer.Count >= Kijun.Length)
                {
                    result = Buffer[0];
                }
            }

            return result == null ? new DecimalIndicatorValue(this) : new DecimalIndicatorValue(this, result.Value);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            settings.SetValue(nameof(_buffer), TimeFrameCandleHelper.CandleArrayToByteArray(_buffer.Select(p => (TimeFrameCandle)p).ToArray()));

            SettingsStorage kijunSettings = new SettingsStorage();

            Kijun.Save(kijunSettings);

            settings.SetValue(nameof(Kijun), SettingsStorageHelper.ToByteArray(kijunSettings));
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            _buffer = ((Candle[])TimeFrameCandleHelper.CandleArrayFromByteArray(settings.GetValue<byte[]>(nameof(_buffer)))).ToList();

            SettingsStorage kijunSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(Kijun)));

            Kijun.Load(kijunSettings);
        }
    }
}
