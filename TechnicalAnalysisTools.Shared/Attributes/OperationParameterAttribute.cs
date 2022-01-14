using System;

namespace TechnicalAnalysisTools.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OperationParameterAttribute : Attribute
    {
        public OperationParameterAttribute(int ordinal, string name, Type type)
        {
            Ordinal = ordinal;
            Name = name;
            Type = type;
        }

        public int Ordinal { get; private set; }

        public string Name { get; private set; }

        public Type Type { get; private set; }
    }
}
