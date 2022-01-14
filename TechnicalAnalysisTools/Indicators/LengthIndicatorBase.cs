using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ecng.Serialization;

namespace TechnicalAnalysisTools.Indicators
{
    public abstract class LengthIndicatorBase<TResult> : IndicatorBase
    {
        protected LengthIndicatorBase()
        {
            Buffer = new List<TResult>();
        }

        public override void Reset()
        {
            Buffer.Clear();
            base.Reset();
        }

        private int _length = 1;

        public int Length
        {
            get => _length;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Period length is incorrectly set.");

                _length = value;

                Reset();
            }
        }

        public override bool IsFormed => Buffer.Count >= Length;

        protected IList<TResult> Buffer { get; private set; }

        public override void Load(SettingsStorage settings)
        {
            base.Load(settings);
            Length = settings.GetValue<int>(nameof(Length));

            using (var memoryStream = new MemoryStream(settings.GetValue<byte[]>(nameof(Buffer))))
            {
                var formatter = new BinaryFormatter();

                Buffer = (List<TResult>)formatter.Deserialize(memoryStream);
            }
        }

        public override void Save(SettingsStorage settings)
        {
            base.Save(settings);
            settings.SetValue(nameof(Length), Length);

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, Buffer);

                settings.SetValue(nameof(Buffer), memoryStream.ToArray());
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " + Length;
        }
    }
}
