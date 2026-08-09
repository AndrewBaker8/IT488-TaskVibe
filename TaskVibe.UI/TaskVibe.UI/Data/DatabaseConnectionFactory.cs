using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TaskVibe.UI
{
    public static class DatabaseConnectionFactory
    {
        // Path to the local SQLite database file in the application directory
        private static readonly string DbFileName = "taskvibe.db";
        public static readonly string ConnectionString = $"Data Source={DbFileName};";

        /// <summary>
        /// Ensures the SQLite database file and required Tasks table exist on startup.
        /// </summary>
        public static void EnsureDatabaseCreated()
        {
            // Opening the connection automatically creates 'taskvibe.db' if missing
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();

                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        TaskId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        DueDate TEXT NOT NULL,
                        Status TEXT NOT NULL
                    );";

                using (var cmd = new SqliteCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }
    }
}