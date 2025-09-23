using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.DataTools;

public class SqlFactory
{
    public static string ToSqlType(EDataType type)
    {
        return type switch
        {
            EDataType.Key => "INTEGER PRIMARY KEY",
            EDataType.Integer => "INTEGER",
            EDataType.Text => "TEXT",
            EDataType.Float => "DECIMAL(18,2)",
            EDataType.Date => "TEXT",
            EDataType.Boolean => "INTEGER",
            _ => throw new NotSupportedException($"Unsupported type {type}")
        };
    }

    public static string GetKeyColumn(TableMetadata table)
    {
        foreach (var col in table.Columns)
        {
            if (col.Value == EDataType.Key)
                return col.Key;
        }
        throw new InvalidOperationException("No primary key defined in table metadata.");
    }

    public static string BuildCreateTable(TableMetadata table)
    {
        var defs = new List<string>();
        foreach (var col in table.Columns)
        {
            var sqlType = ToSqlType(col.Value);
            defs.Add($"{col.Key} {sqlType}");
        }

        var cols = string.Join(",\n    ", defs);
        return $"CREATE TABLE IF NOT EXISTS {table.TableName} (\n    {cols}\n);";
    }

    public static string BuildInsert(TableMetadata table)
    {
        var cols = table.Columns.Keys.Where(c => !c.Equals(GetKeyColumn(table), StringComparison.OrdinalIgnoreCase));

        var colNames = string.Join(", ", cols);
        var colParams = string.Join(", ", cols.Select(c => "@" + c));

        // Important: fetch the last inserted row id
        return $"INSERT INTO {table.TableName} ({colNames}) VALUES ({colParams}); " +
               "SELECT last_insert_rowid();";
    }

    public static string BuildSelect(TableMetadata table, string? whereClause = null, string? orderBy = null, bool descending = false)
    {
        var cols = string.Join(", ", table.Columns.Keys);
        var sql = $"SELECT {cols} FROM {table.TableName}";

        if (!string.IsNullOrWhiteSpace(whereClause))
            sql += " WHERE " + whereClause;

        if (!string.IsNullOrWhiteSpace(orderBy))
            sql += $" ORDER BY {orderBy} {(descending ? "DESC" : "ASC")}";

        return sql;
    }

    public static string BuildUpdate(TableMetadata table)
    {
        string primaryKey = GetKeyColumn(table);
        var sets = string.Join(", ", table.Columns.Keys
            .Where(c => !c.Equals(primaryKey, StringComparison.OrdinalIgnoreCase))
            .Select(c => $"{c} = @{c}"));

        return $"UPDATE {table.TableName} SET {sets} WHERE {primaryKey} = @{primaryKey};";
    }

    public static string BuildDelete(TableMetadata table)
    {
        string primaryKey = GetKeyColumn(table);
        return $"DELETE FROM {table.TableName} WHERE {primaryKey} = @{primaryKey};";
    }
}
