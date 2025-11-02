using Dapper;
using DataModels.Tools;
using Microsoft.Data.Sqlite;
using NexusMaintenance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.DataTools;

public class DataAccess<T> : INotifyPropertyChanged where T : class
{
    public event PropertyChangedEventHandler? PropertyChanged;
    SqliteLogger logger = new SqliteLogger();

    private List<T> allItems = [];
    public List<T> AllItems
    {
        get
        {
            if(allItems.Count == 0)
            {
                logger.Info("AllItems accessed but is currently empty.");
                Task.Run(async () => await GetAllAsync()).Wait();
            }
            return allItems;
        }
        set
        {
            allItems = value;
            logger.Info($"AllItems updated. New count: {allItems.Count}");
            OnPropertyChanged();
        }
    }

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
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildCreateTable(Metadata));
    }

    internal virtual async Task ReloadCachedData()
    {
        await GetAllAsync();
    }

    public virtual async Task InsertAsync(T entity)
    {
        await using var connection = new SqliteConnection(connectionString);
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
        logger.Info(message: $"Inserted new {typeof(T).Name} with ID {newId}.", interaction: "SQLite");
    }
    internal virtual async Task GetAllAsync()
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var sql = SqlFactory.BuildSelect(Metadata, orderBy: Metadata.SortColumn, descending: Metadata.SortDescending);
        AllItems = [.. (await connection.QueryAsync<T>(sql))];
        logger.Info(message: $"Loaded {AllItems.Count} items of type {typeof(T).Name}.", interaction: "SQLite");
    }
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        string primaryKey = SqlFactory.GetKeyColumn(Metadata);
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var result = await connection.QuerySingleOrDefaultAsync<T>(
            SqlFactory.BuildSelect(Metadata, $"{primaryKey} = @{primaryKey}"),
            new Dictionary<string, object> { { primaryKey, id } });
        logger.Info(message: $"Retrieved {typeof(T).Name} with ID {id}.", interaction: "SQLite");
        return result;
    }
    public async Task<List<T>> GetByColumnAsync<TValue>(string columnName, TValue value)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        sql += $" ORDER BY {Metadata.SortColumn} {(Metadata.SortDescending ? "DESC" : "ASC")}";

        using var connection = new SqliteConnection(connectionString);
        List<T> result = [.. (await connection.QueryAsync<T>(sql, new { Value = value }))];
        logger.Info(message: $"Retrieved {result.Count} items of type {typeof(T).Name} where {columnName} = {value}.", interaction: "SQLite");
        return result;
    }
    public async Task<T?> GetOneByColumnAsync<TValue>(string columnName, TValue value)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        sql += $" ORDER BY {Metadata.SortColumn} {(Metadata.SortDescending ? "DESC" : "ASC")}";

        using var connection = new SqliteConnection(connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<T>(sql, new { Value = value });
        logger.Info(message: $"Retrieved item of type {typeof(T).Name} where {columnName} = {value}.", interaction: "SQLite");
        return result;
    }
    public virtual async Task UpdateAsync(T entity)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildUpdate(Metadata), entity);

        AsyncHelper.RunInBackground(ReloadCachedData);
        logger.Info(message: $"Updated {typeof(T).Name} entity.", interaction: "SQLite");
    }
    public virtual async Task DeleteAsync(T id)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildDelete(Metadata), id);

        AsyncHelper.RunInBackground(ReloadCachedData);
        logger.Info(message: $"Deleted {typeof(T).Name} entity.", interaction: "SQLite");
    }
    internal async Task<List<T>> QueryAsync(string sql, object? parameters = null)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        List<T> result = [.. (await connection.QueryAsync<T>(sql, parameters))];
        logger.Info(message: $"Executed QueryAsync with SQL: {sql}, returned {result.Count} results", interaction: "SQLite");
        return result;
    }
    internal async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        logger.Info(message: $"Executed QueryFirstOrDefaultAsync with SQL: {sql}", interaction: "SQLite");
        return result;
    }
    internal virtual async Task ExecuteAsync(string sql, object? parameters = null, bool updateCache = true)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
        if (updateCache)
            AsyncHelper.RunInBackground(ReloadCachedData);
        logger.Info(message: $"Executed ExecuteAsync with SQL: {sql}", interaction: "SQLite");
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
