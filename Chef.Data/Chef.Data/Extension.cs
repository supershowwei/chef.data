using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Chef.Data
{
    public static class Extension
    {
        public static string GenerateUpdateCommand(
            this object me,
            out IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return GenerateUpdateCommand(me, string.Empty, out parameters);
        }

        private static string GenerateUpdateCommand(
            this object me,
            string suffix,
            out IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var output = new StringBuilder();

            if (me is IEnumerable enumerable)
            {
                parameters = new Dictionary<string, object>();

                var index = 0;

                foreach (var item in enumerable)
                {
                    output.AppendLine(GenerateUpdateCommand(item, index++.ToString(), out var tmpParameters));

                    if (tmpParameters != null) parameters = parameters.Concat(tmpParameters);
                }
            }
            else
            {
                var dict = new Dictionary<string, object>();

                var customTable = me.GetType()
                    .CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(TableAttribute));

                var tableName = customTable != null
                                    ? (string)customTable.ConstructorArguments[0].Value
                                    : me.GetType().Name;

                var conditions = new List<string>();
                var setters = new List<string>();

                foreach (var property in me.GetType().GetProperties())
                {
                    if (property.CustomAttributes.Any(x => x.AttributeType == typeof(NotMappedAttribute))) continue;

                    var customColumn =
                        property.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(ColumnAttribute));

                    var columnName = customColumn != null
                                         ? (string)customColumn.ConstructorArguments[0].Value
                                         : property.Name;

                    var parameterName = string.Concat(property.Name, suffix);
                    var parameterValue = property.GetValue(me);

                    switch (parameterValue)
                    {
                        case null: continue;

                        case Field field:
                            setters.Add($"[{columnName}] = @{parameterName}");
                            dict.Add(parameterName, field.GetValue());
                            break;

                        default:
                            conditions.Add($"[{columnName}] = @{parameterName}");
                            dict.Add(parameterName, parameterValue);
                            break;
                    }
                }

                parameters = dict.Count > 0 ? dict : null;

                output.AppendLine($"UPDATE [{tableName}]");
                output.AppendLine($"SET {string.Join(", ", setters)}");
                output.AppendLine($"WHERE {string.Join(" AND ", conditions)}");
            }

            return output.ToString();
        }
    }
}