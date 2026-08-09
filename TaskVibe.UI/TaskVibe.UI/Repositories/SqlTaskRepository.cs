using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using TaskVibe.UI.Models;

namespace TaskVibe.UI.Repositories
{
    public class SqlTaskRepository : ITaskRepository
    {
        // US-02: Task Creation Entry
        public bool AddTask(TaskItem task)
        {
            const string query = @"
                INSERT INTO Tasks (Title, Description, DueDate, Status, AssignedToUserId)
                VALUES (@Title, @Description, @DueDate, @Status, @AssignedToUserId);";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        // Using parameters to securely pass data and prevent SQL injection
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DueDate", task.DueDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Status", task.Status ?? "In Process");
                        command.Parameters.AddWithValue("@AssignedToUserId", (object?)task.AssignedToUserId ?? DBNull.Value);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqliteException ex)
            {
                // Logs the exception to the Output Window during debugging
                System.Diagnostics.Debug.WriteLine($"Database error in AddTask: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            const string query = "SELECT TaskId, Title, Description, DueDate, Status, AssignedToUserId FROM Tasks;";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        connection.Open();
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskItem task = new TaskItem
                                {
                                    TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    // Handling potential database NULL values safely
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? DateTime.MinValue : Convert.ToDateTime(reader.GetString(reader.GetOrdinal("DueDate"))),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "In Process" : reader.GetString(reader.GetOrdinal("Status")),
                                    AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedToUserId"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in GetAllTasks: {ex.Message}");
            }

            return tasks;
        }

        public IEnumerable<TaskItem> GetTasksByUserId(int userId)
        {
            List<TaskItem> tasks = new List<TaskItem>();
            const string query = "SELECT TaskId, Title, Description, DueDate, Status, AssignedToUserId FROM Tasks WHERE AssignedToUserId = @AssignedToUserId;";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AssignedToUserId", userId);

                        connection.Open();
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskItem task = new TaskItem
                                {
                                    TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? DateTime.MinValue : Convert.ToDateTime(reader.GetString(reader.GetOrdinal("DueDate"))),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "In Process" : reader.GetString(reader.GetOrdinal("Status")),
                                    AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedToUserId"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in GetTasksByUserId: {ex.Message}");
            }

            return tasks;
        }

        public bool UpdateTaskStatus(int taskId, string status)
        {
            const string query = "UPDATE Tasks SET Status = @Status WHERE TaskId = @TaskId;";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", (object)status ?? "In Process");
                        command.Parameters.AddWithValue("@TaskId", taskId);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in UpdateTaskStatus: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTaskDeadline(int taskId, DateTime newDeadline)
        {
            const string query = "UPDATE Tasks SET DueDate = @DueDate WHERE TaskId = @TaskId;";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DueDate", newDeadline.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@TaskId", taskId);

                        connection.Open();
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in UpdateTaskDeadline: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            const string query = "DELETE FROM Tasks WHERE TaskId = @TaskId;";

            try
            {
                using (SqliteConnection connection = DatabaseConnectionFactory.GetConnection())
                {
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqliteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in DeleteTask: {ex.Message}");
                return false;
            }
        }
    }
}