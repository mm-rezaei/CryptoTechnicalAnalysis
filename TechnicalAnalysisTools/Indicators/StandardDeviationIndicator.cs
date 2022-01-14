using System;
using System.Collections.Generic;
using System.Linq;
using Ecng.Collections;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class StandardDeviationIndicator : LengthIndicatorBase<decimal>
    {
        private SmaIndicator _sma;

        public StandardDeviationIndicator()
        {
            _sma = new SmaIndicator();
            Length = 10;
        }

        public override bool IsFormed => _sma.IsFormed;

        public override void Reset()
        {
            _sma.Length = Length;
            base.Reset();
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var newValue = input.GetValue<decimal>();
            var smaValue = _sma.Process(input).GetValue<decimal>();

            if (input.IsFinal)
            {
                Buffer.Add(newValue);

                if (Buffer.Count > Length)
                    Buffer.RemoveAt(0);
            }

            var buff = Buffer;
            if (!input.IsFinal)
            {
                buff = new List<decimal>();
                buff.AddRange(Buffer.Skip(1));
                buff.Add(newValue);
            }

            var std = buff.Select(t1 => t1 - smaValue).Select(t => t * t).Sum();

            return new DecimalIndicatorValue(this, (decimal)Math.Sqrt((double)(std / Length)));
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            SettingsStorage smaSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(_sma)));

            _sma.Load(smaSettings);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            SettingsStorage smaSettings = new SettingsStorage();

            _sma.Save(smaSettings);

            settings.SetValue(nameof(_sma), SettingsStorageHelper.ToByteArray(smaSettings));
        }
    }
}
