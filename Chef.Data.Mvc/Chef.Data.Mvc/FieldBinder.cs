using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using Chef.Data.Mvc.Extensions;

namespace Chef.Data.Mvc
{
    public class FieldBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = Activator.CreateInstance(bindingContext.ModelType);

            if (typeof(IList).IsAssignableFrom(bindingContext.ModelType))
            {
                var prefix = bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName)
                                 ? bindingContext.ModelName
                                 : string.Empty;

                var index = 0;

                while (bindingContext.ValueProvider.ContainsPrefix($"{prefix}[{index}]"))
                {
                    var item = Activator.CreateInstance(bindingContext.ModelType.GenericTypeArguments[0]);

                    SetValues(bindingContext.ValueProvider, $"{prefix}[{index}].", ref item);

                    ((IList)result).Add(item);

                    index++;
                }
            }
            else
            {
                SetValues(bindingContext.ValueProvider, string.Empty, ref result);
            }

            return result;
        }

        private static void SetValues(IValueProvider valueProvider, string prefix, ref object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyType = property.PropertyType;

                var value = valueProvider.GetValue(string.Concat(prefix, property.Name));

                if (value == null) continue;

                object objValue;

                if (propertyType.IsSubclassOf(typeof(Field)))
                {
                    var implicitAssignment = propertyType.GetMethods()
                        .Single(x => x.Name.Equals("op_Implicit") && x.ReturnType == propertyType);

                    objValue = implicitAssignment.Invoke(
                        null,
                        new[] { value.TryConvertTo(propertyType.GenericTypeArguments[0]) });
                }
                else
                {
                    objValue = value.ConvertTo(propertyType);
                }

                property.SetValue(obj, objValue);
            }
        }
    }
}