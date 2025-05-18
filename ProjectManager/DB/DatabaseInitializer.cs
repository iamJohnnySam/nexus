using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.DB
{
    public class DatabaseInitializer
    {
        public static void EnsureDatabaseCreated()
        {
            try
            {
                string dbPath = "appdata.db";
                bool dbExists = File.Exists(dbPath);

                using var context = new AppDbContext();

                // This creates the database and schema if they do not exist
                context.Database.Migrate();

                if (!dbExists)
                    Console.WriteLine("Database created successfully.");
                else
                    Console.WriteLine("Database already exists. Checked and updated schema if needed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                // Consider logging this error or displaying it to the user
            }
        }
    }
}
