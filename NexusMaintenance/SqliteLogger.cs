using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NexusMaintenance;

public class SqliteLogger
{
    private readonly string dbFileName = "LoggerDB.sqlite";
    private readonly string _connectionString;
    private readonly object _lock = new();

    // Session context fields
    private string _sessionId = "N/A";
    private string _userName = "Anonymous";

    public SqliteLogger()
    {
        string dbPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dbPath = Path.Combine(homeDir, dbFileName);
        }
        else
        {
            dbPath = dbFileName;
        }


        if (!File.Exists(dbPath))
        {
            //Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            CreateDatabase(dbPath);
        }

        _connectionString = $"Data Source={dbPath}";
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

    private string FormatException(Exception ex)
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

    private void LogInternal(
        string type,
        string message,
        string interaction,
        string method,
        string file)
    {
        //var program = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule?.FileName);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        lock (_lock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO LogEntries 
                (Timestamp, Program, Method, Type, Message, Interaction, SessionId, UserName)
                VALUES ($Timestamp, $Program, $Method, $Type, $Message, $Interaction, $SessionId, $UserName)";
            cmd.Parameters.AddWithValue("$Timestamp", timestamp);
            cmd.Parameters.AddWithValue("$Program", file);
            cmd.Parameters.AddWithValue("$Method", method);
            cmd.Parameters.AddWithValue("$Type", type);
            cmd.Parameters.AddWithValue("$Message", message);
            cmd.Parameters.AddWithValue("$Interaction", interaction);
            cmd.Parameters.AddWithValue("$SessionId", _sessionId);
            cmd.Parameters.AddWithValue("$UserName", _userName);
            cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine($"{type}: {timestamp} - {message}");
    }

    // === PUBLIC METHODS ===

    public void InfoAsync(string message,
        string interaction = "Program output",
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
        => LogInternal("INFO", message, interaction, method, file);

    public void WarnAsync(string message,
        string interaction = "Program output",
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
        => LogInternal("WARN", message, interaction, method, file);

    public void ErrorAsync(string message,
        string interaction = "Program output",
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
        => LogInternal("ERROR", message, interaction, method, file);

    public void ErrorAsync(Exception ex,
        string interaction = "Program output",
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
        => LogInternal("ERROR", FormatException(ex), interaction, method, file);
}