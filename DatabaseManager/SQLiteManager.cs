using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DatabaseManagers
{
    public class SQLiteManager: SQLOps
    {
        public SQLiteManager(string database, string tableName, Dictionary<string, string> columns): base(database)
        {
            CreateDatabaseAndTableIfMissingAsync(tableName, columns).Wait();
        }

        internal async Task CreateDatabaseAndTableIfMissingAsync(string tableName, Dictionary<string, string> columns)
        {
            if (!File.Exists(DatabaseLocation))
            {
                await using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();

                await using var command = new SqliteCommand(CreateTableStatement(tableName, columns), connection);
                await command.ExecuteNonQueryAsync();

                //Console.WriteLine("Database and table created successfully.");
            }
            else
            {
                await EnsureNoMissingColumns(tableName, columns);
            }
        }

        private async Task EnsureNoMissingColumns(string tableName, Dictionary<string, string> expectedColumns)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var existingColumns = new HashSet<string>();

            await using (var command = new SqliteCommand($"PRAGMA table_info({tableName});", connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1)); // Column name
                }
            }

            foreach (KeyValuePair<string, string> column in expectedColumns)
            {
                if (!existingColumns.Contains(column.Key))
                {
                    string alterQuery = $"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value};";
                    await using var alterCommand = new SqliteCommand(alterQuery, connection);
                    await alterCommand.ExecuteNonQueryAsync();
                }
            }
        }





        public DataRow GetTestCase(string testNumber, int? revision = null)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = revision.HasValue ?
                "SELECT * FROM TestCases WHERE TestNumber = @TestNumber AND Revision = @Revision ORDER BY Revision DESC LIMIT 1" :
                "SELECT * FROM TestCases WHERE TestNumber = @TestNumber ORDER BY Revision DESC LIMIT 1";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@TestNumber", testNumber);
            if (revision.HasValue)
                command.Parameters.AddWithValue("@Revision", revision.Value);

            using var adapter = new DataTable();
            using var reader = command.ExecuteReader();
            adapter.Load(reader);

            return adapter.Rows.Count > 0 ? adapter.Rows[0] : null;
        }

        public void UpdateTestCase(string testNumber, string updatedBy, string testCase = null, string testProcedure = null, string checkCases = null)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var latestTestCase = GetTestCase(testNumber);
            if (latestTestCase == null)
                throw new Exception("Test case not found");

            int latestRevision = Convert.ToInt32(latestTestCase["Revision"]);
            string testCategory = latestTestCase["TestCategory"].ToString();
            string testModule = latestTestCase["TestModule"].ToString();
            string newTestCase = testCase ?? latestTestCase["TestCase"].ToString();
            string newTestProcedure = testProcedure ?? latestTestCase["TestProcedure"].ToString();
            string newCheckCases = checkCases ?? latestTestCase["CheckCases"].ToString();
            bool imageNeeded = Convert.ToBoolean(latestTestCase["ImageNeeded"]);
            string revisionDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            string insertQuery = @"
            INSERT INTO TestCases (TestNumber, Revision, RevisionDate, RevisionBy, TestCategory, TestModule, TestCase, TestProcedure, CheckCases, ImageNeeded)
            VALUES (@TestNumber, @Revision, @RevisionDate, @RevisionBy, @TestCategory, @TestModule, @TestCase, @TestProcedure, @CheckCases, @ImageNeeded);";

            using var command = new SqliteCommand(insertQuery, connection);
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

            command.ExecuteNonQuery();
        }

    }
}
