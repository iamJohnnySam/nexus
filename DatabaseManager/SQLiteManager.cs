using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DatabaseManagers
{
    public class SQLiteManager : SQLOps
    {
        private readonly string _tableName;
        private readonly Dictionary<string, string> _columns;

        public SQLiteManager(string database, string tableName, Dictionary<string, string> columns)
            : base(database)
        {
            _tableName = tableName;
            _columns = columns;
            InitializeDatabaseAsync().GetAwaiter().GetResult(); // Block safely at constructor
        }

        private async Task InitializeDatabaseAsync()
        {
            bool databaseExists = File.Exists(DatabaseLocation);

            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            if (!databaseExists)
            {
                await CreateTableAsync(connection);
            }
            else
            {
                bool tableExists = await TableExistsAsync(connection, _tableName);
                if (!tableExists)
                {
                    await CreateTableAsync(connection);
                }
                else
                {
                    await EnsureColumnsExistAsync(connection);
                }
            }
        }

        private async Task<bool> TableExistsAsync(SqliteConnection connection, string tableName)
        {
            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName;";
            await using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private async Task CreateTableAsync(SqliteConnection connection)
        {
            var createTableSql = CreateTableStatement(_tableName, _columns);
            await using var command = new SqliteCommand(createTableSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task EnsureColumnsExistAsync(SqliteConnection connection)
        {
            var existingColumns = new HashSet<string>();

            var pragmaQuery = $"PRAGMA table_info({_tableName});";
            await using var command = new SqliteCommand(pragmaQuery, connection);
           using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                existingColumns.Add(reader.GetString(1)); // column name
            }

            foreach (var column in _columns)
            {
                if (!existingColumns.Contains(column.Key))
                {
                    string alterQuery = $"ALTER TABLE {_tableName} ADD COLUMN {column.Key} {column.Value};";
                    await using var alterCommand = new SqliteCommand(alterQuery, connection);
                    await alterCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<DataRow?> GetTestCaseAsync(string testNumber, int? revision = null)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var query = revision.HasValue
                ? "SELECT * FROM TestCases WHERE TestNumber = @TestNumber AND Revision = @Revision ORDER BY Revision DESC LIMIT 1"
                : "SELECT * FROM TestCases WHERE TestNumber = @TestNumber ORDER BY Revision DESC LIMIT 1";

            await using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TestNumber", testNumber);
            if (revision.HasValue)
                command.Parameters.AddWithValue("@Revision", revision.Value);

            var dataTable = new DataTable();
            await using (var reader = await command.ExecuteReaderAsync())
            {
                dataTable.Load(reader);
            }

            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        public async Task UpdateTestCaseAsync(string testNumber, string updatedBy, string? testCase = null, string? testProcedure = null, string? checkCases = null)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var latestTestCase = await GetTestCaseAsync(testNumber);
            if (latestTestCase == null)
                throw new InvalidOperationException($"Test case '{testNumber}' not found.");

            int latestRevision = Convert.ToInt32(latestTestCase["Revision"]);
            string testCategory = latestTestCase["TestCategory"].ToString() ?? "";
            string testModule = latestTestCase["TestModule"].ToString() ?? "";
            string newTestCase = testCase ?? latestTestCase["TestCase"].ToString() ?? "";
            string newTestProcedure = testProcedure ?? latestTestCase["TestProcedure"].ToString() ?? "";
            string newCheckCases = checkCases ?? latestTestCase["CheckCases"].ToString() ?? "";
            bool imageNeeded = Convert.ToBoolean(latestTestCase["ImageNeeded"]);
            string revisionDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var insertQuery = @"
                INSERT INTO TestCases 
                    (TestNumber, Revision, RevisionDate, RevisionBy, TestCategory, TestModule, TestCase, TestProcedure, CheckCases, ImageNeeded)
                VALUES 
                    (@TestNumber, @Revision, @RevisionDate, @RevisionBy, @TestCategory, @TestModule, @TestCase, @TestProcedure, @CheckCases, @ImageNeeded);";

            await using var command = new SqliteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@TestNumber", testNumber);
            command.Parameters.AddWithValue("@Revision", latestRevision + 1);
            command.Parameters.AddWithValue("@RevisionDate", revisionDate);
            command.Parameters.AddWithValue("@RevisionBy", updatedBy);
            command.Parameters.AddWithValue("@TestCategory", testCategory);
            command.Parameters.AddWithValue("@TestModule", testModule);
            command.Parameters.AddWithValue("@TestCase", newTestCase);
            command.Parameters.AddWithValue("@TestProcedure", newTestProcedure);
            command.Parameters.AddWithValue("@CheckCases", newCheckCases);
            command.Parameters.AddWithValue("@ImageNeeded", imageNeeded);

            await command.ExecuteNonQueryAsync();
        }
    }
}