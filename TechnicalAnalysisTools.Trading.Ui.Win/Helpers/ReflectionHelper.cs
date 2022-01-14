using System.Linq;
using System.Reflection;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Helpers
{
    internal static class ReflectionHelper
    {
        internal static void CopyValuableFields(object fromEntity, object toEntity)
        {
            var fromEntityFields = fromEntity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var toEntityFields = toEntity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var fieldName in toEntityFields.Select(p => p.Name))
            {
                var fromEntityField = fromEntityFields.FirstOrDefault(p => p.Name == fieldName);
                var toEntityField = toEntityFields.FirstOrDefault(p => p.Name == fieldName);

                if (fromEntityField != null && toEntityField != null)
                {
                    if (fromEntityField.FieldType.IsValueType && toEntityField.FieldType.IsValueType)
                    {
                        if (fromEntityField.FieldType == toEntityField.FieldType)
                        {
                            var value = fromEntityField.GetValue(fromEntity);

                            toEntityField.SetValue(toEntity, value);
                        }
                    }
                }
            }
        }

        internal static void CopyValuableProperties(object fromEntity, object toEntity)
        {
            var fromEntityProperties = fromEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.CanRead);
            var toEntityProperties = toEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.CanRead);

            foreach (var propertyName in toEntityProperties.Select(p => p.Name))
            {
                var fromEntityProperty = fromEntityProperties.FirstOrDefault(p => p.Name == propertyName);
                var toEntityProperty = toEntityProperties.FirstOrDefault(p => p.Name == propertyName);

                if (fromEntityProperty != null && toEntityProperty != null)
                {
                    if (fromEntityProperty.PropertyType.IsValueType && toEntityProperty.PropertyType.IsValueType)
                    {
                        if (fromEntityProperty.PropertyType == toEntityProperty.PropertyType)
                        {
                            if (fromEntityProperty.CanRead && toEntityProperty.CanWrite)
                            {
                                var value = fromEntityProperty.GetValue(fromEntity);

                                toEntityProperty.SetValue(toEntity, value);
                            }
                        }
                    }
                }
            }
        }
    }
}
