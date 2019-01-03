using System;
using System.Web.Mvc;

namespace Chef.Data.Mvc.Extensions
{
    internal static class ValueProviderResultExtension
    {
        public static object TryConvertTo(this ValueProviderResult me, Type type)
        {
            try
            {
                return me.ConvertTo(type);
            }
            catch
            {
                // ignored
            }

            return Activator.CreateInstance(type);
        }
    }
}