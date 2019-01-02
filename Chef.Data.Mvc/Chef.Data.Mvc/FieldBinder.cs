using System;
using System.Linq;
using System.Web.Mvc;

namespace Chef.Data.Mvc
{
    public class FieldBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = Activator.CreateInstance(bindingContext.ModelType);

            foreach (var property in result.GetType().GetProperties())
            {
                var propertyType = property.PropertyType;

                var value = bindingContext.ValueProvider.GetValue(property.Name);

                if (value == null) continue;

                object parameter;

                if (propertyType.IsSubclassOf(typeof(Field)))
                {
                    var implicitAssignment = propertyType.GetMethods()
                        .Single(x => x.Name.Equals("op_Implicit") && x.ReturnType == propertyType);

                    parameter = implicitAssignment.Invoke(
                        null,
                        new[] { GetParameter(propertyType.GenericTypeArguments[0], value.RawValue) });
                }
                else
                {
                    parameter = GetParameter(propertyType, value.RawValue);
                }

                property.SetValue(result, parameter);
            }

            return result;
        }

        private static object GetParameter(Type propertyType, object rawValue)
        {
            var propertyTypeFullName = propertyType.FullName ?? string.Empty;
            var rawValueTypeFullName = rawValue != null ? rawValue.GetType().FullName ?? string.Empty : string.Empty;

            if (rawValueTypeFullName.Contains("System.Decimal"))
            {
                return Convert.ToDouble(rawValue);
            }
            else if (propertyTypeFullName.Contains("System.DateTime"))
            {
                return DateTime.TryParse((string)rawValue, out var datetime)
                           ? datetime
                           : Activator.CreateInstance(propertyType);
            }
            else if (propertyTypeFullName.Contains("System.Int") && rawValueTypeFullName.Contains("System.String"))
            {
                var tryParser = propertyType.GetMethods()
                    .Single(x => x.Name.Equals("TryParse") && x.GetParameters().Length == 2);

                var parameters = new[] { rawValue, null };
                var result = tryParser.Invoke(null, parameters);

                return (bool)result ? parameters[1] : Activator.CreateInstance(propertyType);
            }

            return rawValue;
        }
    }
}