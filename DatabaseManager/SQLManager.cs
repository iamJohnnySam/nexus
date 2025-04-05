using System.Data;
using System.Text;
using MySqlConnector;

namespace DatabaseManagers
{
    public class SQLManager: SQLOps
    {
        public SQLManager(string database, string dbUser, string dbPassword): base(database, dbUser, dbPassword)
        {
            CreateDatabaseIfMissingAsync().Wait();
        }


        internal async Task CreateDatabaseIfMissingAsync()
        {
            var connectionStringWithoutDb = new MySqlConnectionStringBuilder(ConnectionStringNoDB)
            {
                Database = null // Clear the database name
            }.ConnectionString;

            using var connection = new MySqlConnection(connectionStringWithoutDb);
            await connection.OpenAsync();

            var result = await CheckDatabaseExist(connection);

            // If the database doesn't exist, create it
            if (result == null)
            {
                string createDbQuery = $"CREATE DATABASE `{Database}`;";
                using var createCommand = new MySqlCommand(createDbQuery, connection);
                await createCommand.ExecuteNonQueryAsync();
                // Console.WriteLine($"Database '{Database}' created successfully.");
            }
            else
            {
                // Console.WriteLine($"Database '{Database}' already exists.");
            }
        }
        private async Task<object?> CheckDatabaseExist(MySqlConnection connection)
        {
            string checkDbQuery = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @DatabaseName;";
            using var checkCommand = new MySqlCommand(checkDbQuery, connection);
            checkCommand.Parameters.AddWithValue("@DatabaseName", Database);

            return await checkCommand.ExecuteScalarAsync();
        }
















        public void RunStructureCheck(string tableName, Dictionary<string, string> columns)
        {
            AddTableIfMissingAsync(tableName, columns).Wait();
            EnsureTableStructureAsync(tableName, columns).Wait();
        }


        // DATABASE CONTROLS
        

        

        // TABLE CONTROLS
        internal async Task<bool> CheckTableExistsAsync(string tableName)
        {
            var query = $"SHOW TABLES LIKE @TableName";
            var parameters = new Dictionary<string, object> { { "@TableName", tableName } };

            var result = await ExecuteScalarAsync(query, parameters);
            return result != null;
        }
        public async Task AddTableIfMissingAsync(string tableName, Dictionary<string, string> columns)
        {
            if (!await CheckTableExistsAsync(tableName))
            {
                await ExecuteNonQueryAsync(CreateTableStatement(tableName, columns));
                Console.WriteLine($"Table '{tableName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Table '{tableName}' already exists.");
                await EnsureTableStructureAsync(tableName, columns);
            }
        }
        public async Task EnsureTableStructureAsync(string tableName, Dictionary<string, string> columns)
        {
            if (!await CheckTableExistsAsync(tableName))
            {
                var createTableQuery = new StringBuilder($"CREATE TABLE {tableName} (");
                foreach (var column in columns)
                {
                    createTableQuery.Append($"{column.Key} {column.Value}, ");
                }

                createTableQuery.Length -= 2; // Remove trailing comma and space
                createTableQuery.Append(");");
                await ExecuteNonQueryAsync(createTableQuery.ToString());
            }
            else
            {
                foreach (var column in columns)
                {
                    var columnExistsQuery = $"SHOW COLUMNS FROM {tableName} LIKE @ColumnName";
                    var parameters = new Dictionary<string, object> { { "@ColumnName", column.Key } };
                    var columnExists = await ExecuteQueryAsync(columnExistsQuery, parameters);

                    if (columnExists.Rows.Count == 0)
                    {
                        var addColumnQuery = $"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value};";
                        await ExecuteNonQueryAsync(addColumnQuery);
                    }
                }
            }
        }
        public async Task EnsureValueInColumnAsync(string tableName, string columnName, object valueToCheck, Dictionary<string, object> valuesToInsert)
        {
            // Check if the value exists
            var checkQuery = $"SELECT 1 FROM {tableName} WHERE {columnName} = @ValueToCheck LIMIT 1;";
            var checkParameters = new Dictionary<string, object> { { "@ValueToCheck", valueToCheck } };

            var result = await ExecuteScalarAsync(checkQuery, checkParameters);

            if (result == null)
            {
                // Construct the INSERT query
                var columns = string.Join(", ", valuesToInsert.Keys);
                var parameters = string.Join(", ", valuesToInsert.Keys.Select(k => "@" + k));
                var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";

                await ExecuteNonQueryAsync(insertQuery, valuesToInsert);
                Console.WriteLine($"Value '{valueToCheck}' added to column '{columnName}' in table '{tableName}'.");
            }
            else
            {
                Console.WriteLine($"Value '{valueToCheck}' already exists in column '{columnName}' of table '{tableName}'.");
            }
        }








        // CONNECTION CONTROLS
        internal async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }



        // EXECUTION CONTROLS
        internal async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }
        internal async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            using var adapter = new MySqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }
        internal async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return await command.ExecuteScalarAsync();
        }



        // CRUD
        public async Task<int> InsertAsync(string tableName, Dictionary<string, object> values)
        {
            return await ExecuteNonQueryAsync(InsertStatement(tableName, values), values);
        }
        public async Task<int> UpdateAsync(string tableName, Dictionary<string, object> values, string condition)
        {
            return await ExecuteNonQueryAsync(UpdateStatement(tableName, values, condition), values);
        }
        public async Task<int> DeleteAsync(string tableName, string condition)
        {
            return await ExecuteNonQueryAsync(DeleteStatement(tableName, condition));
        }
        public async Task<DataTable> SelectAsync(string tableName, string condition = "1=1", string? orderBy = null, int? limit = null)
        {
            return await ExecuteQueryAsync(SelectStatement(tableName, condition, orderBy, limit));
        }

        public async Task<bool> ValueExistsAsync(string tableName, string columnName, object valueToCheck)
        {
            // Construct the SQL query to check for the value
            var query = $"SELECT 1 FROM {tableName} WHERE {columnName} = @ValueToCheck LIMIT 1;";
            var parameters = new Dictionary<string, object>
            {
                { "@ValueToCheck", valueToCheck }
            };

            // Execute the query and check the result
            var result = await ExecuteScalarAsync(query, parameters);

            // Return true if the value exists, otherwise false
            return result != null;
        }
        public async Task<bool> CombinationExistsAsync(string tableName, Dictionary<string, object> columnValuePairs)
        {
            if (columnValuePairs == null || columnValuePairs.Count == 0)
            {
                throw new ArgumentException("Column-value pairs cannot be null or empty.");
            }

            // Dynamically construct the WHERE clause
            var conditions = string.Join(" AND ", columnValuePairs.Keys.Select(key => $"{key} = @{key}"));
            var query = $"SELECT 1 FROM {tableName} WHERE {conditions} LIMIT 1;";

            // Execute the query and check the result
            var result = await ExecuteScalarAsync(query, columnValuePairs);

            // Return true if the combination exists, otherwise false
            return result != null;
        }

        public async Task<int> UpdateColumnValueAsync(string tableName, string columnName, object newValue)
        {
            // Construct the SQL update query
            var query = $"UPDATE {tableName} SET {columnName} = @NewValue;";

            // Create the parameters
            var parameters = new Dictionary<string, object>
            {
                { "@NewValue", newValue }
            };

            // Execute the query and return the number of affected rows
            return await ExecuteNonQueryAsync(query, parameters);
        }

    }
}
