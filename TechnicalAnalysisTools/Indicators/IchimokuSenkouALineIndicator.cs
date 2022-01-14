using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuSenkouALineIndicator : LengthIndicatorBase<decimal>
    {
        public IchimokuSenkouALineIndicator(IchimokuLineIndicator tenkan, IchimokuLineIndicator kijun)
        {
            Tenkan = tenkan ?? throw new ArgumentNullException(nameof(tenkan));
            Kijun = kijun ?? throw new ArgumentNullException(nameof(kijun));
        }

        public override bool IsFormed => Buffer.Count >= Kijun.Length;

        public IchimokuLineIndicator Tenkan { get; }

        public IchimokuLineIndicator Kijun { get; }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            decimal? result = null;

            if (Tenkan.IsFormed && Kijun.IsFormed)
            {
                if (input.IsFinal)
                {
                    Buffer.Add((Tenkan.GetCurrentValue() + Kijun.GetCurrentValue()) / 2);

                    if (Buffer.Count > Kijun.Length)
                    {
                        Buffer.RemoveAt(0);
                    }
                }

                if (IsFormed)
                {
                    result = Buffer[0];
                }
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
