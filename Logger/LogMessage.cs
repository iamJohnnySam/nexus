using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Logger
{
    public class LogMessage
    {
        private static LogMessage? instance = null;
        private static readonly object threadLock = new();

        private readonly SQLManager db;

        readonly string program;
        private static readonly Dictionary<string, string> columns = new()
        {
            { "id", "INT PRIMARY KEY AUTO_INCREMENT" },
            { "level", "ENUM('INFO', 'WARN', 'ERROR', 'DEBUG') NOT NULL DEFAULT 'INFO'" },
            { "timestamp", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "job_id", "TEXT" },
            { "source", "TEXT" },
            { "log", "TEXT NOT NULL" },
        };

        private LogMessage()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("LogSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfiguration config = builder.Build();

            string dbUser = config.GetValue<string>("DB_USER") ?? "nexus";
            string dbPassword = config.GetValue<string>("ATOM_DB_PASSWORD") ?? "nexus";

            db = new("Log", dbUser, dbPassword);
            program = GetImplementingProgramName().Split('.')[0];
            db.RunStructureCheck(program, columns);
        }

        public static LogMessage GetInstance()
        {
            if (instance == null)
            {
                lock (threadLock) // now I can claim some form of thread safety...
                {
                    if (instance == null)
                    {
                        instance = new LogMessage();
                    }
                }
            }

            return instance;
        }



        public async Task<bool> Log(string jobID, LogLevel level, string? sender, string logMessage)
        {
            string logLevel = level switch
            {
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Debug => "DEBUG",
                _ => "DEBUG",
            };
            await db.InsertAsync(program, new Dictionary<string, object>
            {
            { "level", logLevel},
            { "job", jobID },
            { "source", sender ?? string.Empty },
            { "log", logMessage }
            });
            // Console.WriteLine($"{DateTime.Now:h:mm:ss tt}:{logLevel}:{program}:{jobID}:{sender}:{logMessage}");
            return true;
        }

        public async Task<bool> Log(LogLevel level, string? sender, string logMessage)
        {
            return await Log(string.Empty, level, sender, logMessage);
        }

        public async Task<bool> Debug(string jobID, string? sender, string logMessage)
        {
            return await Log(jobID, LogLevel.Debug, sender, logMessage);
        }

        public async Task<bool> Error(string jobID, string? sender, string logMessage)
        {
            return await Log(jobID, LogLevel.Error, sender, logMessage);
        }

        public async Task<bool> Warn(string jobID, string? sender, string logMessage)
        {
            return await Log(jobID, LogLevel.Warning, sender, logMessage);
        }

        public async Task<bool> Info(string jobID, string? sender, string logMessage)
        {
            return await Log(jobID, LogLevel.Information, sender, logMessage);
        }






        public static string GetImplementingProgramName()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                return "General";
            }
            return entryAssembly.GetName().Name ?? "General";
        }
    }
}
