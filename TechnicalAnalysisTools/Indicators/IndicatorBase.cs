using System;
using Ecng.Common;
using Ecng.ComponentModel;
using Ecng.Serialization;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public abstract class IndicatorBase : Cloneable<IIndicator>, IIndicator
    {
        protected IndicatorBase()
        {
            var type = GetType();

            _name = type.GetDisplayName();
            InputType = type.GetValueType(true);
            ResultType = type.GetValueType(false);
        }

        public Guid Id { get; private set; } = Guid.NewGuid();

        private string _name;

        public virtual string Name
        {
            get => _name;
            set
            {
                if (value.IsEmpty())
                    throw new ArgumentNullException(nameof(value));

                _name = value;
            }
        }

        public virtual void Reset()
        {
            IsFormed = false;
            Container.ClearValues();
            Reseted?.Invoke();
        }

        public virtual void Save(SettingsStorage storage)
        {
            storage.SetValue(nameof(Id), Id);
            storage.SetValue(nameof(Name), Name);
        }

        public virtual void Load(SettingsStorage storage)
        {
            Id = storage.GetValue<Guid>(nameof(Id));
            Name = storage.GetValue<string>(nameof(Name));
        }

        public virtual bool IsFormed { get; protected set; }

        public IIndicatorContainer Container { get; } = new IndicatorContainer();

        public virtual Type InputType { get; }

        public virtual Type ResultType { get; }

        public event Action<IIndicatorValue, IIndicatorValue> Changed;

        public event Action Reseted;

        public virtual IIndicatorValue Process(IIndicatorValue input)
        {
            var result = OnProcess(input);

            result.InputValue = input;

            if (input.IsFinal)
            {
                result.IsFinal = input.IsFinal;
            }

            Container.ClearValues();
            Container.AddValue(input, result);

            if (IsFormed && !result.IsEmpty)
                RaiseChangedEvent(input, result);

            return result;
        }

        protected abstract IIndicatorValue OnProcess(IIndicatorValue input);

        protected void RaiseChangedEvent(IIndicatorValue input, IIndicatorValue result)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Changed?.Invoke(input, result);
        }

        public override IIndicator Clone()
        {
            return PersistableHelper.Clone(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
