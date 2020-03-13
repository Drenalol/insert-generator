using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InsertGenerator
{
    public static class Generator
    {
        public static string GetStringInsert<T>(string returning = null) where T : new()
        {
            var type = typeof(T);

            var tablename = type.CustomAttributes
                .FirstOrDefault(t => t.AttributeType == typeof(TableAttribute))?
                .ConstructorArguments
                .First();

            var columns = type
                .GetProperties()
                .Where(x => x.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == typeof(InsertColumnIgnoreAttribute)) == null)
                .Select(prop => prop.Name.Underscore())
                .ToList();

            string GetColumnNames(string v = "") => columns.Select(s => s.Insert(0, v)).Aggregate((i, j) => $"{i}, {j}");

            var sb = new StringBuilder();

            sb.Append($"INSERT INTO {tablename?.Value} ({GetColumnNames()}) VALUES ({GetColumnNames(":")})");
            sb.Append(string.IsNullOrEmpty(returning) ? ";" : $" RETURNING {returning};");
            
            return sb.ToString();
        }

        /// <summary>
        /// Creates INSERT query from T with `ON CONFLICT` properties from unqiueAnonymouesExpression
        /// </summary>
        /// <param name="unqiueAnonymouesExpression"></param>
        /// <param name="returning"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public static string GetStringInsertOnConflict<T>(Expression<Func<T, object>> unqiueAnonymouesExpression, string returning = null) where T : new()
        {
            var type = typeof(T);

            var tablename = type.CustomAttributes
                .FirstOrDefault(t => t.AttributeType == typeof(TableAttribute))?
                .ConstructorArguments
                .First();

            if (tablename == null)
                throw new AggregateException($"{nameof(TableAttribute)} is required in {type}");

            var columns = type
                .GetProperties()
                .Where(x => x.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == typeof(InsertColumnIgnoreAttribute)) == null)
                .Select(prop => prop.Name.Underscore())
                .ToList();

            var conflict = unqiueAnonymouesExpression
                .Compile()(new T())
                .GetType()
                .GetProperties()
                .Select(p => type.GetProperty(p.Name))
                .Where(p => p != null)
                .Select(p => p.Name.Underscore())
                .Aggregate((i, j) => $"{i}, {j}");

            var sb = new StringBuilder();

            sb.Append($"INSERT INTO {tablename.Value.Value} ({GetColumnNames()}) VALUES ({GetColumnNames(":")}) ON CONFLICT ({conflict})");
            // UGLY Solution for Returning
            var doUpdate = columns.Last();
            sb.Append(string.IsNullOrEmpty(returning) ? " DO NOTHING;" : $" DO UPDATE SET {doUpdate}=excluded.{doUpdate} RETURNING {returning};");

            string GetColumnNames(string v = "") => columns.Select(s => s.Insert(0, v)).Aggregate((i, j) => $"{i}, {j}");
            return sb.ToString();
        }
    }
}