using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NexusMaintenance;

public class SqliteLogger : IDisposable
{
    private readonly string _connectionString;
    private readonly BlockingCollection<LogEntry> _logQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundTask;

    private string _sessionId = "N/A";
    private string _userName = "Anonymous";

    private record LogEntry(string Type, string Message, string Interaction, string Method, string File, string Timestamp);

    public SqliteLogger()
    {
        string dbFileName = "LoggerDB.sqlite";
        string dbPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), dbFileName)
            : dbFileName;

        _connectionString = $"Data Source={dbPath}";

        if (!File.Exists(dbPath))
            CreateDatabase(dbPath);

        // Start background writer
        _backgroundTask = Task.Run(ProcessQueueAsync);
    }

    public void SetSessionContext(string sessionId, string userName)
    {
        _sessionId = sessionId ?? "N/A";
        _userName = userName ?? "Anonymous";
    }

    private void CreateDatabase(string dbPath)
    {
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS LogEntries (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Timestamp TEXT NOT NULL,
            Program TEXT NOT NULL,
            Method TEXT NOT NULL,
            Type TEXT NOT NULL,
            Message TEXT NOT NULL,
            Interaction TEXT NOT NULL DEFAULT 'Program output',
            SessionId TEXT NOT NULL DEFAULT 'N/A',
            UserName TEXT NOT NULL DEFAULT 'Anonymous'
        );";
        cmd.ExecuteNonQuery();
    }

    private async Task ProcessQueueAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                List<LogEntry> batch = new();
                while (_logQueue.TryTake(out var entry, TimeSpan.FromMilliseconds(100)))
                    batch.Add(entry);

                if (batch.Count == 0)
                    continue;

                using var transaction = connection.BeginTransaction();
                foreach (var entry in batch)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO LogEntries 
                        (Timestamp, Program, Method, Type, Message, Interaction, SessionId, UserName)
                        VALUES ($Timestamp, $Program, $Method, $Type, $Message, $Interaction, $SessionId, $UserName)";
                    cmd.Parameters.AddWithValue("$Timestamp", entry.Timestamp);
                    cmd.Parameters.AddWithValue("$Program", entry.File);
                    cmd.Parameters.AddWithValue("$Method", entry.Method);
                    cmd.Parameters.AddWithValue("$Type", entry.Type);
                    cmd.Parameters.AddWithValue("$Message", entry.Message);
                    cmd.Parameters.AddWithValue("$Interaction", entry.Interaction);
                    cmd.Parameters.AddWithValue("$SessionId", _sessionId);
                    cmd.Parameters.AddWithValue("$UserName", _userName);
                    await cmd.ExecuteNonQueryAsync();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logger error: {ex.Message}");
                await Task.Delay(500);
            }
        }
    }

    private void Enqueue(string type, string message, string interaction, string method, string file)
    {
        var entry = new LogEntry(
            type,
            message,
            interaction,
            method,
            Path.GetFileNameWithoutExtension(file),
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        );

        _logQueue.Add(entry);
        Console.WriteLine($"{entry.Type}: {entry.Timestamp} - {entry.Message}");
    }

    public void Info(string message, string interaction = "Program output", [CallerMemberName] string method = "", [CallerFilePath] string file = "")
        => Enqueue("INFO", message, interaction, method, file);

    public void Warn(string message, string interaction = "Program output", [CallerMemberName] string method = "", [CallerFilePath] string file = "")
        => Enqueue("WARN", message, interaction, method, file);

    public void Error(string message, string interaction = "Program output", [CallerMemberName] string method = "", [CallerFilePath] string file = "")
        => Enqueue("ERROR", message, interaction, method, file);

    public void Error(Exception ex, string interaction = "Program output", [CallerMemberName] string method = "", [CallerFilePath] string file = "")
        => Enqueue("ERROR", FormatException(ex), interaction, method, file);

    private static string FormatException(Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ex.Message);
        sb.AppendLine(ex.StackTrace);
        var inner = ex.InnerException;
        while (inner != null)
        {
            sb.AppendLine("---- Inner Exception ----");
            sb.AppendLine(inner.Message);
            sb.AppendLine(inner.StackTrace);
            inner = inner.InnerException;
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _backgroundTask.Wait();
        _logQueue.Dispose();
        _cts.Dispose();
    }
}
