using DatabaseManagers;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FATBuilder
{
    internal class FATManager
    {
        SQLiteManager db;

        Dictionary<string, string> fatTestCaseColumns = new()
        {
            { "testID", "INT PRIMARY KEY AUTO_INCREMENT" },
            { "timestamp", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "revision", "INT NOT NULL" },
            { "revisionDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "revisionBy", "TEXT NOT NULL" },
            { "category", "TEXT NOT NULL" },
            { "module", "TEXT NOT NULL" },
            { "case", "TEXT NOT NULL" },
            { "procedure", "LONGTEXT" },
            { "check", "TEXT" },
            { "image", "BOOLEAN" },
            { "unit", "TEXT" }
        };
        Dictionary<string, string> fatDocumentColumns = new()
        {
            { "id", "INT PRIMARY KEY AUTO_INCREMENT" },
            { "project", "TEXT NOT NULL" },
            { "revision", "INT NOT NULL" },
            { "revisionDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "revisionBy", "TEXT NOT NULL" },
            { "testIndex", "INT NOT NULL" },
            { "testID", "INT NOT NULL" }
        };


        public FATManager(string databaseName)
        {
            db = new(databaseName, "TestCases", fatTestCaseColumns);
        }

    }
}
