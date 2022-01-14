using System;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Ui.Win.Auxiliaries
{
    internal class FakeIndicatorAuxiliary : IIndicator
    {
        public Guid Id => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsFormed => true;

        public IIndicatorContainer Container => throw new NotImplementedException();

        public Type InputType => throw new NotImplementedException();

        public Type ResultType => throw new NotImplementedException();

#pragma warning disable CS0067

        public event Action<IIndicatorValue, IIndicatorValue> Changed;

        public event Action Reseted;

#pragma warning restore CS0067

        public IIndicator Clone()
        {
            throw new NotImplementedException();
        }

        public void Load(SettingsStorage storage)
        {
            throw new NotImplementedException();
        }

        public IIndicatorValue Process(IIndicatorValue input)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Save(SettingsStorage storage)
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }
}
