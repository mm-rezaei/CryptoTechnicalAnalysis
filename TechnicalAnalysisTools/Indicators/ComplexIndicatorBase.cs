using System;
using System.Collections.Generic;
using System.Linq;
using Ecng.Serialization;
using MoreLinq;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Indicators
{
    public abstract class ComplexIndicatorBase : IndicatorBase, IComplexIndicator
    {
        protected ComplexIndicatorBase(params IIndicator[] innerIndicators)
        {
            if (innerIndicators == null)
                throw new ArgumentNullException(nameof(innerIndicators));

            if (innerIndicators.Any(i => i == null))
                throw new ArgumentException(nameof(innerIndicators));

            InnerIndicators = new List<IIndicator>(innerIndicators);

            Mode = ComplexIndicatorModes.Parallel;
        }

        public ComplexIndicatorModes Mode { get; protected set; }

        protected IList<IIndicator> InnerIndicators { get; }

        IEnumerable<IIndicator> IComplexIndicator.InnerIndicators => InnerIndicators;

        public override bool IsFormed
        {
            get { return InnerIndicators.All(i => i.IsFormed); }
        }

        public override Type ResultType { get; } = typeof(ComplexIndicatorValue);

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var value = new ComplexIndicatorValue(this);

            foreach (var indicator in InnerIndicators)
            {
                var result = indicator.Process(input);

                value.InnerValues.Add(indicator, result);

                if (Mode == ComplexIndicatorModes.Sequence)
                {
                    if (!indicator.IsFormed)
                    {
                        break;
                    }

                    input = result;
                }
            }

            return value;
        }

        public override void Reset()
        {
            base.Reset();
            InnerIndicators.ForEach(i => i.Reset());
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);

            var index = 0;

            foreach (var indicator in InnerIndicators)
            {
                var innerSettings = new SettingsStorage();
                indicator.Save(innerSettings);
                settings.SetValue(indicator.Name + index, SettingsStorageHelper.ToByteArray(innerSettings));
                index++;
            }
        }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);

            var index = 0;

            foreach (var indicator in InnerIndicators)
            {
                indicator.Load(SettingsStorageHelper.FromByteArray(settings.GetValue<byte[]>(indicator.Name + index)));
                index++;
            }
        }
    }
}
