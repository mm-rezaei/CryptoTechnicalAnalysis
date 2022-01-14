using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuSenkouA26LineIndicator : IndicatorBase
    {
        public IchimokuSenkouA26LineIndicator(IchimokuLineIndicator tenkan, IchimokuLineIndicator kijun)
        {
            Tenkan = tenkan ?? throw new ArgumentNullException(nameof(tenkan));
            Kijun = kijun ?? throw new ArgumentNullException(nameof(kijun));
        }

        public override bool IsFormed => Tenkan.IsFormed && Kijun.IsFormed;

        public IchimokuLineIndicator Tenkan { get; }

        public IchimokuLineIndicator Kijun { get; }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            decimal? result = null;

            if (IsFormed)
            {
                result = (Tenkan.GetCurrentValue() + Kijun.GetCurrentValue()) / 2;
            }

            return result == null ? new DecimalIndicatorValue(this) : new DecimalIndicatorValue(this, result.Value);
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            SettingsStorage tenkanSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(Tenkan)));
            SettingsStorage kijunSettings = SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(nameof(Kijun)));

            Tenkan.Load(tenkanSettings);
            Kijun.Load(kijunSettings);
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            SettingsStorage tenkanSettings = new SettingsStorage();
            SettingsStorage kijunSettings = new SettingsStorage();

            Tenkan.Save(tenkanSettings);
            Kijun.Save(kijunSettings);

            settings.SetValue(nameof(Tenkan), SettingsStorageHelper.ToByteArray(tenkanSettings));
            settings.SetValue(nameof(Kijun), SettingsStorageHelper.ToByteArray(kijunSettings));
        }
    }
}
