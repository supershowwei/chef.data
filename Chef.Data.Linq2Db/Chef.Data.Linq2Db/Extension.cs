﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Linq;

namespace Chef.Data.Linq2Db
{
    public static class Extension
    {
        public static void Update<TTable>(this IFieldSet me, DataConnection cnn)
            where TTable : class
        {
            var tableType = typeof(TTable);
            var parameterExpr = Expression.Parameter(tableType);

            Expression predicate = null;
            var setters = new List<(Field Field, Expression Lambda)>();

            foreach (var property in me.GetType().GetProperties())
            {
                if (property.CustomAttributes.Any(x => x.AttributeType == typeof(NotMappedAttribute))) continue;

                var propertyExpr = Expression.Property(parameterExpr, property.Name);
                var propertyValue = property.GetValue(me);

                if (propertyValue is Field field)
                {
                    setters.Add((field, Expression.Lambda(propertyExpr, parameterExpr)));
                }
                else
                {
                    var keyExpr = Expression.Equal(propertyExpr, Expression.Constant(propertyValue));

                    predicate = predicate == null ? keyExpr : Expression.And(predicate, keyExpr);
                }
            }

            var queryable = cnn.GetTable<TTable>().Where(Expression.Lambda<Func<TTable, bool>>(predicate, parameterExpr));

            var setMethod = typeof(LinqExtensions).GetMethods()
                .Where(
                    x =>
                    {
                        if (!x.Name.Equals("Set")) return false;

                        var ps = x.GetParameters();

                        if (ps.Length != 3) return false;
                        if (ps[2].ParameterType.IsGenericType) return false;

                        return true;
                    })
                .ToDictionary(x => x.GetParameters()[0].ParameterType.Name, x => x);

            IUpdatable<TTable> updatable = null;

            foreach (var setter in setters)
            {
                if (updatable == null)
                {
                    updatable = setMethod["IQueryable`1"]
                                        .MakeGenericMethod(tableType, setter.Field.GetValueType())
                                        .Invoke(
                                            null,
                                            new[] { queryable, setter.Lambda, setter.Field.GetValue() }) as IUpdatable<TTable>;
                }
                else
                {
                    updatable = setMethod["IUpdatable`1"]
                                        .MakeGenericMethod(tableType, setter.Field.GetValueType())
                                        .Invoke(
                                            null,
                                            new[] { updatable, setter.Lambda, setter.Field.GetValue() }) as IUpdatable<TTable>;
                }
            }

            updatable.Update();
        }
    }
}