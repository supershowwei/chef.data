using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Chef.Data
{
    public static class Extension
    {
        public static string ToUpdateCommand(this IFieldSet me, out IDictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();

            return ToUpdateCommand(me, string.Empty, parameters);
        }

        public static string ToUpdateCommand(this IEnumerable<IFieldSet> me, out IDictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();

            var output = new StringBuilder();

            var index = 0;

            foreach (var item in me)
            {
                output.AppendLine(ToUpdateCommand(item, $"_{index++}", parameters));
            }

            return output.ToString();
        }

        private static string ToUpdateCommand(object me, string suffix, IDictionary<string, object> parameters)
        {
            var output = new StringBuilder();

            var tableName = me.GetType().GetCustomAttribute<TableAttribute>().Name;

            var conditions = new List<string>();
            var setters = new List<string>();

            foreach (var property in me.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<NotMappedAttribute>() != null) continue;

                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                var columnName = columnAttribute?.Name ?? property.Name;

                var parameterName = string.Concat(property.Name, suffix);
                var parameterValue = property.GetValue(me);

                switch (parameterValue)
                {
                    case null: continue;

                    case Field field:
                        setters.Add($"[{columnName}] = @{parameterName}");
                        parameters.Add(parameterName, field.GetValue());
                        break;

                    default:
                        conditions.Add($"[{columnName}] = @{parameterName}");
                        parameters.Add(parameterName, parameterValue);
                        break;
                }
            }

            output.AppendLine($"UPDATE [{tableName}]");
            output.AppendLine($"SET {string.Join(", ", setters)}");
            output.AppendLine($"WHERE {string.Join(" AND ", conditions)};");

            return output.ToString();
        }
    }
}