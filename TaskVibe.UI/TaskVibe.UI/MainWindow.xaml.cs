using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskVibe.UI.Models;
using TaskVibe.UI.Repositories;

namespace TaskVibe.UI
{
    public partial class MainWindow : Window
    {
        private int _selectedTaskId = -1; // -1 means no task is selected (Create Mode)
        private readonly SqlTaskRepository _taskRepository;

        public MainWindow()
        {
            InitializeComponent();

            _taskRepository = new SqlTaskRepository();

            // Ensure SQLite database & table exist, then load data
            DatabaseConnectionFactory.EnsureDatabaseCreated();
            LoadTasks();
        }

        #region Helper Methods (State Management)

        /// <summary>
        /// Toggles visibility between Create Mode and Edit Mode panels
        /// and resets all form inputs when switching back to Create Mode.
        /// </summary>
        private void SetFormState(bool isEditMode)
        {
            if (isEditMode)
            {
                PnlCreateMode.Visibility = Visibility.Collapsed;
                PnlEditMode.Visibility = Visibility.Visible;
            }
            else
            {
                PnlCreateMode.Visibility = Visibility.Visible;
                PnlEditMode.Visibility = Visibility.Collapsed;
                ResetForm();
            }
        }

        /// <summary>
        /// Clears all input controls, unselects grid items, and resets the tracking ID.
        /// </summary>
        private void ResetForm()
        {
            _selectedTaskId = -1;
            TxtTaskTitle.Clear();
            TxtDescription.Clear();
            DpDueDate.SelectedDate = null;
            CmbStatus.SelectedIndex = 0;
            DgTasks.UnselectAll();
        }

        #endregion

        #region CRUD Button Operations

        private void BtnCreateTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Capture the inputs from the UI controls
            string taskTitle = TxtTaskTitle.Text.Trim();
            string description = TxtDescription.Text.Trim();
            DateTime? dueDate = DpDueDate.SelectedDate;

            // Get the text content of the selected ComboBoxItem
            string status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started";

            // 2. Basic Validation
            if (string.IsNullOrEmpty(taskTitle))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dueDate == null)
            {
                MessageBox.Show("Please select a valid due date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqliteConnection conn = DatabaseConnectionFactory.GetConnection())
                {
                    conn.Open();

                    string query = @"INSERT INTO Tasks (Title, Description, DueDate, Status) 
                                     VALUES (@Title, @Description, @DueDate, @Status);";

                    using (SqliteCommand cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", taskTitle);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@DueDate", dueDate.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Status", status);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Task successfully saved to the database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form inputs, refresh the DataGrid, and update counters
                ResetForm();
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdateTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation
            if (_selectedTaskId == -1)
            {
                MessageBox.Show("Please select a task from the grid first to update.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTaskTitle.Text) || DpDueDate.SelectedDate == null)
            {
                MessageBox.Show("Task Title and Due Date cannot be blank.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqliteConnection conn = DatabaseConnectionFactory.GetConnection())
                {
                    conn.Open();

                    string query = @"UPDATE Tasks 
                                     SET Title = @Title, 
                                         Description = @Description, 
                                         DueDate = @DueDate, 
                                         Status = @Status 
                                     WHERE TaskId = @TaskId;";

                    using (SqliteCommand cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", _selectedTaskId);
                        cmd.Parameters.AddWithValue("@Title", TxtTaskTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Description", TxtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@DueDate", DpDueDate.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Status", (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started");

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Task updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            SetFormState(isEditMode: false);
                            LoadTasks();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskId == -1)
            {
                MessageBox.Show("Please select a task from the grid first to delete.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to permanently delete the selected task: \"{TxtTaskTitle.Text}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.No) return;

            try
            {
                using (SqliteConnection conn = DatabaseConnectionFactory.GetConnection())
                {
                    conn.Open();

                    string query = "DELETE FROM Tasks WHERE TaskId = @TaskId;";

                    using (SqliteCommand cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", _selectedTaskId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Task deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Switch back to Create Mode and refresh grid
                            SetFormState(isEditMode: false);
                            LoadTasks();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            // Exit Edit Mode and restore the form back to Create Mode
            SetFormState(isEditMode: false);
        }

        #endregion

        #region DataGrid & Database Loading Methods

        private void LoadTasks()
        {
            try
            {
                using (SqliteConnection conn = DatabaseConnectionFactory.GetConnection())
                {
                    conn.Open();

                    string query = "SELECT TaskId, Title, Description, DueDate, Status FROM Tasks;";

                    using (SqliteCommand cmd = new SqliteCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        using (SqliteDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }

                        // Automated "Late" status calculation for active overdue items
                        foreach (DataRow row in dt.Rows)
                        {
                            string currentStatus = row["Status"]?.ToString() ?? "";
                            DateTime dueDate = Convert.ToDateTime(row["DueDate"]);

                            if (currentStatus != "Completed" && dueDate < DateTime.Today)
                            {
                                row["Status"] = "Late";
                            }
                        }

                        DataView dv = dt.DefaultView;
                        dv.Sort = "DueDate ASC";

                        DgTasks.ItemsSource = null;
                        DgTasks.ItemsSource = dv;

                        // Recalculate summary totals whenever data loads
                        UpdateTaskSummary();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgTasks_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "TaskId")
            {
                e.Cancel = true;
            }
        }

        private void DgTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the grid row selection is cleared (or during grid rebind), revert back to Create Mode
            if (DgTasks.SelectedItem == null)
            {
                return;
            }

            if (DgTasks.SelectedItem is DataRowView row)
            {
                // 1. Store the primary key
                _selectedTaskId = Convert.ToInt32(row["TaskId"]);

                // 2. Map row fields into controls
                TxtTaskTitle.Text = row["Title"]?.ToString() ?? "";
                TxtDescription.Text = row["Description"]?.ToString() ?? "";
                DpDueDate.SelectedDate = Convert.ToDateTime(row["DueDate"]);

                string currentStatus = row["Status"]?.ToString() ?? "Not Started";

                foreach (ComboBoxItem item in CmbStatus.Items)
                {
                    if (item.Content.ToString() == currentStatus)
                    {
                        CmbStatus.SelectedItem = item;
                        break;
                    }
                }

                // 3. Reveal Edit/Delete mode controls dynamically
                SetFormState(isEditMode: true);
            }
        }

        #endregion

        #region Statistics and Data Clear

        private void UpdateTaskSummary()
        {
            // Query the bound DataView items directly
            if (DgTasks.ItemsSource is DataView dv)
            {
                int total = dv.Count;
                int completed = 0;
                int late = 0;

                foreach (DataRowView rowView in dv)
                {
                    string status = rowView["Status"]?.ToString() ?? "";
                    if (status == "Completed")
                        completed++;
                    else if (status == "Late")
                        late++;
                }

                // Update TextBlock UI elements
                TxtTotalCount.Text = total.ToString();
                TxtCompletedCount.Text = completed.ToString();
                TxtLateCount.Text = late.ToString();
            }
        }

        private void BtnClearCompleted_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to remove all completed tasks?",
                "Confirm Clear",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqliteConnection conn = DatabaseConnectionFactory.GetConnection())
                    {
                        conn.Open();

                        string query = "DELETE FROM Tasks WHERE Status = 'Completed';";

                        using (SqliteCommand cmd = new SqliteCommand(query, conn))
                        {
                            int rowsDeleted = cmd.ExecuteNonQuery();

                            if (rowsDeleted > 0)
                            {
                                MessageBox.Show($"{rowsDeleted} completed task(s) removed successfully.", "Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("No completed tasks found to remove.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }

                    // Reload grid & recalculate metrics
                    LoadTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing completed tasks: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}