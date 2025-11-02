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
                logger.InfoAsync("AllItems accessed but is currently empty.");
                Task.Run(async () => await GetAllAsync()).Wait();
            }
            return allItems;
        }
        set
        {
            allItems = value;
            logger.InfoAsync($"AllItems updated. New count: {allItems.Count}");
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
    }
    internal virtual async Task GetAllAsync()
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var sql = SqlFactory.BuildSelect(Metadata, orderBy: Metadata.SortColumn, descending: Metadata.SortDescending);
        AllItems = [.. (await connection.QueryAsync<T>(sql))];
    }
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        string primaryKey = SqlFactory.GetKeyColumn(Metadata);
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(
            SqlFactory.BuildSelect(Metadata, $"{primaryKey} = @{primaryKey}"),
            new Dictionary<string, object> { { primaryKey, id } });
    }
    public async Task<List<T>> GetByColumnAsync<TValue>(string columnName, TValue value)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        sql += $" ORDER BY {Metadata.SortColumn} {(Metadata.SortDescending ? "DESC" : "ASC")}";

        using var connection = new SqliteConnection(connectionString);
        return [.. (await connection.QueryAsync<T>(sql, new { Value = value }))];
    }
    public async Task<T?> GetOneByColumnAsync<TValue>(string columnName, TValue value)
    {
        var tableName = typeof(T).Name;
        var sql = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";

        sql += $" ORDER BY {Metadata.SortColumn} {(Metadata.SortDescending ? "DESC" : "ASC")}";

        using var connection = new SqliteConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Value = value });
    }
    public virtual async Task UpdateAsync(T entity)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildUpdate(Metadata), entity);

        AsyncHelper.RunInBackground(ReloadCachedData);
    }
    public virtual async Task DeleteAsync(T id)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(SqlFactory.BuildDelete(Metadata), id);

        AsyncHelper.RunInBackground(ReloadCachedData);
    }
    internal async Task<List<T>> QueryAsync(string sql, object? parameters = null)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        logger.InfoAsync($"Executing QueryAsync with SQL: {sql}");
        return [.. (await connection.QueryAsync<T>(sql, parameters))];
    }
    internal async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        logger.InfoAsync($"Executing QueryFirstOrDefaultAsync with SQL: {sql}");
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }
    internal virtual async Task ExecuteAsync(string sql, object? parameters = null, bool updateCache = true)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
        logger.InfoAsync($"Executed SQL: {sql}");
        if (updateCache)
            AsyncHelper.RunInBackground(ReloadCachedData);
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
