using System.Linq;
using System.Reflection;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class ReflectionHelper
    {
        public static void CopyValuableFields<TEntity>(TEntity fromEntity, TEntity toEntity) where TEntity : class
        {
            var fields = typeof(TEntity).GetFields();

            foreach (var fieldInfo in fields)
            {
                if (!fieldInfo.FieldType.IsClass)
                {
                    var value = fieldInfo.GetValue(fromEntity);

                    fieldInfo.SetValue(toEntity, value);
                }
            }
        }

        public static void CopyValuableProperties<TEntity>(TEntity fromEntity, TEntity toEntity) where TEntity : class
        {
            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.CanRead);

            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.PropertyType.IsClass)
                {
                    if (propertyInfo.CanWrite)
                    {
                        var value = propertyInfo.GetValue(fromEntity);

                        propertyInfo.SetValue(toEntity, value);
                    }
                }
            }
        }

        public static object GetPropertyValue<TEntity>(TEntity entity, string propertyName) where TEntity : class
        {
            object result = null;

            var property = typeof(TEntity).GetProperty(propertyName);

            if (property != null)
            {
                result = property.GetValue(entity);
            }

            return result;
        }
    }
}
