using System;
using Microsoft.Data.Sqlite;

namespace TaskVibe.UI.Data
{
    public static class DatabaseConnectionFactory
    {
        private static readonly string ConnectionString = "Data Source=TaskVibe.db";

        /// <summary>
        /// Creates and returns a new SQLite connection.
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        /// <summary>
        /// Automatically creates the SQLite database file and Tasks table if they don't exist yet.
        /// </summary>
        public static void EnsureDatabaseCreated()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string tableCmd = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        TaskId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        DueDate TEXT NOT NULL,
                        Status TEXT NOT NULL
                    );";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = tableCmd;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}