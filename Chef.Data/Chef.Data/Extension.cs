using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Chef.Data
{
    public static class Extension
    {
        public static string GenerateUpdateCommand(this object me, out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();

            var table = (string)me.GetType()
                .CustomAttributes.Single(x => x.AttributeType == typeof(TableAttribute))
                .ConstructorArguments[0]
                .Value;

            var conditions = new List<string>();
            var setters = new List<string>();

            foreach (var property in me.GetType().GetProperties())
            {
                var customColumn =
                    property.CustomAttributes.SingleOrDefault(x => x.AttributeType == typeof(ColumnAttribute));

                var columnName = customColumn != null
                                     ? (string)customColumn.ConstructorArguments[0].Value
                                     : property.Name;

                var field = property.GetValue(me);

                if (field is Field)
                {
                    setters.Add($"[{columnName}] = @{property.Name}");
                    parameters.Add(property.Name, ((Field)field).GetValue());
                }
                else
                {
                    conditions.Add($"[{columnName}] = @{property.Name}");
                    parameters.Add(property.Name, field);
                }
            }

            return @"
UPDATE [" + table + @"]
SET " + string.Join(", ", setters) + @"
WHERE " + string.Join(" AND ", conditions);
        }
    }
}