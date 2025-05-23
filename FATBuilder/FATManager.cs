﻿using DatabaseManagers;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FATBuilder
{
    public class FATManager : IFATManager
    {
        public SQLiteManager db { get; private set; }

        Dictionary<string, string> fatTestCaseColumns = new()
        {
            { "testID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
            { "timestamp", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "testIndex", "INTEGER NOT NULL" },
            { "revision", "INTEGER NOT NULL" },
            { "revisionDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "revisionBy", "TEXT NOT NULL" },
            { "testCategory", "TEXT NOT NULL" },
            { "testModule", "TEXT NOT NULL" },
            { "testCase", "TEXT NOT NULL" },
            { "testProcedure", "TEXT" },
            { "testCheck", "TEXT" },
            { "image", "BOOLEAN" },
            { "unit", "TEXT" }
        };
        Dictionary<string, string> fatDocumentColumns = new()
        {
            { "id", "INTEGER PRIMARY KEY AUTOINCREMENT" },
            { "project", "TEXT NOT NULL" },
            { "revision", "INTEGER NOT NULL" },
            { "revisionDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
            { "revisionBy", "TEXT NOT NULL" },
            { "testIndex", "INTEGER NOT NULL" },
            { "testID", "INTEGER NOT NULL" }
        };


        public FATManager()
        {
            db = new("FAT", "TestCases", fatTestCaseColumns);
        }



    }
}
