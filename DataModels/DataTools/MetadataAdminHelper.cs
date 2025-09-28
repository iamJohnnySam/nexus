using Dapper;
using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Tools;

public static class MetadataDapperHelper
{
    // 🔹 INSERT ----------------------------------------------------------------
    public static async Task<int> InsertAsync(TableMetadata metadata, Dictionary<string, object?> values)
    {
        await using var conn = new SQLiteConnection("Data Source=NexusDB.sqlite;Version=3;");
        await conn.OpenAsync();

        var cols = metadata.Columns.Where(c => c.Value != EDataType.Key).Select(c => c.Key).ToList();
        var colList = string.Join(", ", cols);
        var paramList = string.Join(", ", cols.Select(c => "@" + c));

        var sql = $"INSERT INTO {metadata.TableName} ({colList}) VALUES ({paramList});";
        sql += " SELECT last_insert_rowid();"; // For SQLite (change to SCOPE_IDENTITY() for SQL Server)

        return await conn.ExecuteScalarAsync<int>(sql, values);
    }

    // 🔹 UPDATE ----------------------------------------------------------------
    public static async Task<int> UpdateAsync(TableMetadata metadata, Dictionary<string, object?> values)
    {
        await using var conn = new SQLiteConnection("Data Source=NexusDB.sqlite;Version=3;");
        await conn.OpenAsync();

        var keyCol = metadata.Columns.First(c => c.Value == EDataType.Key).Key;
        if (!values.ContainsKey(keyCol))
            throw new ArgumentException($"Key column '{keyCol}' missing in data.");

        var cols = metadata.Columns.Where(c => c.Value != EDataType.Key).Select(c => c.Key).ToList();
        var setList = string.Join(", ", cols.Select(c => $"{c} = @{c}"));

        var sql = $"UPDATE {metadata.TableName} SET {setList} WHERE {keyCol} = @{keyCol}";
        return await conn.ExecuteAsync(sql, values);
    }

    // 🔹 DELETE ----------------------------------------------------------------
    public static async Task<int> DeleteAsync(TableMetadata metadata, object keyValue)
    {
        await using var conn = new SQLiteConnection("Data Source=NexusDB.sqlite;Version=3;");
        await conn.OpenAsync();

        var keyCol = metadata.Columns.First(c => c.Value == EDataType.Key).Key;
        var sql = $"DELETE FROM {metadata.TableName} WHERE {keyCol} = @Key";
        return await conn.ExecuteAsync(sql, new { Key = keyValue });
    }

    // 🔹 GET ALL ---------------------------------------------------------------
    public static async Task<List<T>> GetAllAsync<T>(TableMetadata metadata)
    {
        await using var conn = new SQLiteConnection("Data Source=NexusDB.sqlite;Version=3;");
        await conn.OpenAsync();

        var sql = $"SELECT * FROM {metadata.TableName} ORDER BY {metadata.SortColumn} {(metadata.SortDescending ? "DESC" : "ASC")}";
        var result = await conn.QueryAsync<T>(sql);
        return result.ToList();
    }

    // 🔹 GET BY ID -------------------------------------------------------------
    public static async Task<T?> GetByIdAsync<T>(TableMetadata metadata, object keyValue)
    {
        await using var conn = new SQLiteConnection("Data Source=NexusDB.sqlite;Version=3;");
        await conn.OpenAsync();

        var keyCol = metadata.Columns.First(c => c.Value == EDataType.Key).Key;
        var sql = $"SELECT * FROM {metadata.TableName} WHERE {keyCol} = @Key";
        return await conn.QueryFirstOrDefaultAsync<T>(sql, new { Key = keyValue });
    }

    // 🔹 Helper: Generate raw SQL (optional) -----------------------------------
    public static string GenerateInsertSql(TableMetadata metadata)
    {
        var cols = metadata.Columns.Where(c => c.Value != EDataType.Key).Select(c => c.Key);
        var colList = string.Join(", ", cols);
        var paramList = string.Join(", ", cols.Select(c => "@" + c));
        return $"INSERT INTO {metadata.TableName} ({colList}) VALUES ({paramList});";
    }

    public static string GenerateUpdateSql(TableMetadata metadata)
    {
        var keyCol = metadata.Columns.First(c => c.Value == EDataType.Key).Key;
        var cols = metadata.Columns.Where(c => c.Value != EDataType.Key).Select(c => c.Key);
        var setList = string.Join(", ", cols.Select(c => $"{c} = @{c}"));
        return $"UPDATE {metadata.TableName} SET {setList} WHERE {keyCol} = @{keyCol};";
    }

    public static string GenerateDeleteSql(TableMetadata metadata)
    {
        var keyCol = metadata.Columns.First(c => c.Value == EDataType.Key).Key;
        return $"DELETE FROM {metadata.TableName} WHERE {keyCol} = @{keyCol};";
    }
}
