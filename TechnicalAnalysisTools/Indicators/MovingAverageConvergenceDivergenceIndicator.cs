using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class MovingAverageConvergenceDivergenceIndicator : IndicatorBase
    {
        public MovingAverageConvergenceDivergenceIndicator() : this(new EmaIndicator { Length = 26 }, new EmaIndicator { Length = 12 })
        {
        }

        public MovingAverageConvergenceDivergenceIndicator(EmaIndicator longMa, EmaIndicator shortMa)
        {
            ShortMa = shortMa ?? throw new ArgumentNullException(nameof(shortMa));
            LongMa = longMa ?? throw new ArgumentNullException(nameof(longMa));
        }

        public EmaIndicator LongMa { get; }

        public EmaIndicator ShortMa { get; }

        public override bool IsFormed => LongMa.IsFormed;

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var shortValue = ShortMa.Process(input);
            var longValue = LongMa.Process(input);
            return new DecimalIndicatorValue(this, shortValue.GetValue<decimal>() - longValue.GetValue<decimal>());
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            SettingsStorage longMaSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(LongMa)));
            SettingsStorage shortMaSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(ShortMa)));

            LongMa.Load(longMaSettings);
            ShortMa.Load(shortMaSettings);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            SettingsStorage longMaSettings = new SettingsStorage();
            SettingsStorage shortMaSettings = new SettingsStorage();

            LongMa.Save(longMaSettings);
            ShortMa.Save(shortMaSettings);

            settings.SetValue(nameof(LongMa), SettingsStorageHelper.ToByteArray(longMaSettings));
            settings.SetValue(nameof(ShortMa), SettingsStorageHelper.ToByteArray(shortMaSettings));
        }
    }
}
