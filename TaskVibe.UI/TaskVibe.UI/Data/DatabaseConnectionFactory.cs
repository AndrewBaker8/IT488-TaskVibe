using System.Configuration;
using Microsoft.Data.SqlClient;

namespace TaskVibe.UI.Data
{
    public static class DatabaseConnectionFactory
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["TaskVibeDB"].ConnectionString;

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}