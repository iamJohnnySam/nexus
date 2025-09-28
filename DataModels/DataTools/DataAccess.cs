using Dapper;
using System.Data.SQLite;
using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.DataTools;

public class DataAccess<T> where T : class
{
    public TableMetadata Metadata { get; }
    internal readonly string connectionString;

    public DataAccess(string connectionString, TableMetadata metadata)
    {
        this.connectionString = connectionString;
        Metadata = metadata;

        // Ensure table exists at startup
        CreateTableAsync().GetAwaiter().GetResult();
    }

    internal async Task CreateTableAsync()
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildCreateTable(Metadata));
    }
    public async Task InsertAsync(T entity)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();

        // ExecuteScalarAsync<long> returns the new row id
        var newId = await connection.ExecuteScalarAsync<long>(
            SqlFactory.BuildInsert(Metadata), entity);

        // Set the primary key property on the entity
        var pkProp = typeof(T).GetProperty(SqlFactory.GetKeyColumn(Metadata));
        if (pkProp != null && pkProp.CanWrite)
        {
            if (pkProp.PropertyType == typeof(int))
                pkProp.SetValue(entity, (int)newId);
            else if (pkProp.PropertyType == typeof(long))
                pkProp.SetValue(entity, newId);
        }
    }
    public virtual async Task<List<T>> GetAllAsync(string? orderBy = null, bool descending = false)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        var sql = SqlFactory.BuildSelect(Metadata, orderBy: orderBy, descending: descending);
        return [.. (await connection.QueryAsync<T>(sql))];
    }
    public async Task<List<T>> GetAllSortedAsync()
    {
        return await GetAllAsync(Metadata.SortColumn, Metadata.SortDescending);
    }
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        string primaryKey = SqlFactory.GetKeyColumn(Metadata);
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(
            SqlFactory.BuildSelect(Metadata, $"{primaryKey} = @{primaryKey}"),
            new Dictionary<string, object> { { primaryKey, id } });
    }
    public async Task<List<T>> GetByColumnAsync<TValue>(string columnName, TValue value, string? orderByColumn = null, bool ascending = true)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        if (!string.IsNullOrEmpty(orderByColumn))
        {
            sql += $" ORDER BY {orderByColumn} {(ascending ? "ASC" : "DESC")}";
        }

        using var connection = new SQLiteConnection(connectionString);
        return [.. (await connection.QueryAsync<T>(sql, new { Value = value }))];
    }
    public async Task<T?> GetOneByColumnAsync<TValue>(string columnName, TValue value, string? orderByColumn = null, bool ascending = true)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        if (!string.IsNullOrEmpty(orderByColumn))
        {
            sql += $" ORDER BY {orderByColumn} {(ascending ? "ASC" : "DESC")}";
        }

        using var connection = new SQLiteConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Value = value });
    }
    public virtual async Task UpdateAsync(T entity)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildUpdate(Metadata), entity);
    }
    public virtual async Task DeleteAsync(object id)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildDelete(Metadata), id);
    }
    internal async Task<List<T>> QueryAsync(string sql, object? parameters = null)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        return [.. (await connection.QueryAsync<T>(sql, parameters))];
    }
    internal async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }
    internal virtual async Task ExecuteAsync(string sql, object? parameters = null)
    {
        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
    }
}
