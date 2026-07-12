using System;
using System.Windows;
using Microsoft.Data.SqlClient;
using TaskVibe.UI.Data; 

namespace TaskVibe.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = DatabaseConnectionFactory.CreateConnection())
                {
                    conn.Open();

                    string query = "SELECT Username FROM dbo.Users";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string userList = "Connected! Seeded Users:" + Environment.NewLine;
                        while (reader.Read())
                        {
                            userList += "- " + reader["Username"] + Environment.NewLine;
                        }

                        MessageBox.Show(userList, "Database Test Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Failed:" + Environment.NewLine + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}