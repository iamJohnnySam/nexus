using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManagers
{
    public class SQLOps
    {
        internal string Database { get; private set; }
        internal string DatabaseLocation { get; set; }

        internal string ConnectionString { get; set; }
        internal string ConnectionStringNoDB { get; set; }



        public SQLOps(string database, string dbUser, string dbPassword)
        {
            Database = database;
            DatabaseLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{database}.db");
            ConnectionString = $"Server=localhost;Database={database};User ID={dbUser};Password={dbPassword};Port=3306;";
            ConnectionStringNoDB = $"Server=localhost;User ID={dbUser};Password={dbPassword};Port=3306;";
        }

        public SQLOps(string database)
        {
            Database = database;
            DatabaseLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{database}.db");
            ConnectionString = $"Data Source={DatabaseLocation}";
            ConnectionStringNoDB = string.Empty;
        }


        // CRUD STATEMENTS
        internal static string InsertStatement(string tableName, Dictionary<string, object> values)
        {
            var columns = string.Join(", ", values.Keys);
            var parameters = string.Join(", ", values.Keys.Select(key => $"@{key}"));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        }
        internal static string UpdateStatement(string tableName, Dictionary<string, object> values, string condition)
        {
            var updates = string.Join(", ", values.Keys.Select(key => $"{key} = @{key}"));
            return $"UPDATE {tableName} SET {updates} WHERE {condition}";
        }
        internal static string DeleteStatement(string tableName, string condition)
        {
            return $"DELETE FROM {tableName} WHERE {condition}";
        }
        internal static string SelectStatement(string tableName, string condition = "1=1", string? orderBy = null, int? limit = null)
        {
            var query = $"SELECT * FROM {tableName} WHERE {condition}";

            if (!string.IsNullOrEmpty(orderBy))
            {
                query += $" ORDER BY {orderBy}";
            }

            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }
            return query;
        }
        internal static string CreateTableStatement(string tableName, Dictionary<string, string> columns)
        {
            var createTableQuery = new StringBuilder($"CREATE TABLE {tableName} (");
            foreach (var column in columns)
            {
                createTableQuery.Append($"{column.Key} {column.Value}, ");
            }
            createTableQuery.Length -= 2; // Remove trailing comma and space
            createTableQuery.Append(");");
            return createTableQuery.ToString();
        }

    }
}
