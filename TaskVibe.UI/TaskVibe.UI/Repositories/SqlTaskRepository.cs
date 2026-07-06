using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using TaskVibe.UI.Data;
using TaskVibe.UI.Models;

namespace TaskVibe.UI.Repositories
{
    public class SqlTaskRepository : ITaskRepository
    {
        // US-02: Task Creation Entry
        public bool AddTask(TaskItem task)
        {
            const string query = @"
                INSERT INTO dbo.Tasks (Title, Description, DueDate, Status, AssignedToUserId)
                VALUES (@Title, @Description, @DueDate, @Status, @AssignedToUserId);";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Using parameters to securely pass data and prevent SQL injection
                        command.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = task.Title;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object)task.Description ?? DBNull.Value;
                        command.Parameters.Add("@DueDate", SqlDbType.DateTime).Value = (object)task.DueDate ?? DBNull.Value;
                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = task.Status ?? "In Process";
                        command.Parameters.Add("@AssignedToUserId", SqlDbType.Int).Value = (object)task.AssignedToUserId ?? DBNull.Value;

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Logs the exception to the Output Window during debugging
                System.Diagnostics.Debug.WriteLine($"Database error in AddTask: {ex.Message}");
                return false;
            }
        }

        // --- Placeholders for the remaining interface methods ---
        public IEnumerable<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            const string query = "SELECT TaskId, Title, Description, DueDate, Status, AssignedToUserId FROM dbo.Tasks;";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskItem task = new TaskItem
                                {
                                    TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    // Handling potential database NULL values safely
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "In Process" : reader.GetString(reader.GetOrdinal("Status")),
                                    AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedToUserId"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in GetAllTasks: {ex.Message}");
                // Return whatever tasks we managed to safely harvest before the error, or an empty list
            }

            return tasks;
        }

        public IEnumerable<TaskItem> GetTasksByUserId(int userId)
        {
            List<TaskItem> tasks = new List<TaskItem>();
            const string query = "SELECT TaskId, Title, Description, DueDate, Status, AssignedToUserId FROM dbo.Tasks WHERE AssignedToUserId = @AssignedToUserId;";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@AssignedToUserId", SqlDbType.Int).Value = userId;

                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TaskItem task = new TaskItem
                                {
                                    TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    // Using Option A: strict DateTime matching your model
                                    DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "In Process" : reader.GetString(reader.GetOrdinal("Status")),
                                    AssignedToUserId = reader.IsDBNull(reader.GetOrdinal("AssignedToUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedToUserId"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in GetTasksByUserId: {ex.Message}");
            }

            return tasks;
        }

        public bool UpdateTaskStatus(int taskId, string status)
        {
            const string query = "UPDATE dbo.Tasks SET Status = @Status WHERE TaskId = @TaskId;";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = (object)status ?? "In Process";
                        command.Parameters.Add("@TaskId", SqlDbType.Int).Value = taskId;

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in UpdateTaskStatus: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTaskDeadline(int taskId, DateTime newDeadline)
        {
            const string query = "UPDATE dbo.Tasks SET DueDate = @DueDate WHERE TaskId = @TaskId;";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@DueDate", SqlDbType.DateTime).Value = newDeadline;
                        command.Parameters.Add("@TaskId", SqlDbType.Int).Value = taskId;

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in UpdateTaskDeadline: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            const string query = "DELETE FROM dbo.Tasks WHERE TaskId = @TaskId;";

            try
            {
                using (SqlConnection connection = DatabaseConnectionFactory.CreateConnection())
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@TaskId", SqlDbType.Int).Value = taskId;

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in DeleteTask: {ex.Message}");
                return false;
            }
        }
    }
}