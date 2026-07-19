using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TaskVibe.UI.Data; 

namespace TaskVibe.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadTasks(); //Pulls existing tasks on startup
        }

        private int _selectedTaskId = -1; // -1 means no task is selected yet

        private void BtnCreateTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Capture the inputs from the UI controls
            string taskTitle = TxtTaskTitle.Text.Trim();
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
                // 1. Establish connection using your factory
                using (SqlConnection conn = DatabaseConnectionFactory.CreateConnection())
                {
                    conn.Open();

                    // 2. Draft the SQL parameter query (prevents SQL injection)
                    string query = @"INSERT INTO dbo.Tasks (Title, DueDate, Status) 
                             VALUES (@Title, @DueDate, @Status);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // 3. Bind the UI values safely to the parameters
                        cmd.Parameters.AddWithValue("@Title", taskTitle);
                        cmd.Parameters.AddWithValue("@DueDate", dueDate.Value);
                        cmd.Parameters.AddWithValue("@Status", status);

                        // 4. Execute the command
                        cmd.ExecuteNonQuery();
                    }
                }

                // 5. Success feedback and form reset
                MessageBox.Show("Task successfully saved to the database!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                TxtTaskTitle.Clear();
                DpDueDate.SelectedDate = null;
                CmbStatus.SelectedIndex = 0;

                // TODO: Refresh the DataGrid on the right so the new task appears instantly!
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdateTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation: Ensure a task is actually selected
            if (_selectedTaskId == -1)
            {
                MessageBox.Show("Please select a task from the grid first to update.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validation: Ensure inputs aren't empty
            if (string.IsNullOrWhiteSpace(TxtTaskTitle.Text) || DpDueDate.SelectedDate == null)
            {
                MessageBox.Show("Task Title and Due Date cannot be blank.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = DatabaseConnectionFactory.CreateConnection())
                {
                    conn.Open();

                    // SQL Update Query targeting the selected TaskId
                    string query = @"UPDATE dbo.Tasks 
                             SET Title = @Title, DueDate = @DueDate, Status = @Status 
                             WHERE TaskId = @TaskId;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Sanitize inputs using parameters
                        cmd.Parameters.AddWithValue("@TaskId", _selectedTaskId);
                        cmd.Parameters.AddWithValue("@Title", TxtTaskTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@DueDate", DpDueDate.SelectedDate.Value);

                        // Get the text from the selected ComboBoxItem
                        string selectedStatus = (CmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started";
                        cmd.Parameters.AddWithValue("@Status", selectedStatus);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Task updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // 3. Reset the form and clear tracking variable
                            TxtTaskTitle.Clear();
                            DpDueDate.SelectedDate = null;
                            CmbStatus.SelectedIndex = 0;
                            _selectedTaskId = -1;

                            // 4. Refresh the grid immediately to see the changes
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
            // 1. Ensure a task is actually selected
            if (_selectedTaskId == -1)
            {
                MessageBox.Show("Please select a task from the grid first to delete.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Security/UX: Ask the user to confirm the deletion
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to permanently delete the selected task: \"{TxtTaskTitle.Text}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            // If they change their mind, exit immediately
            if (result == MessageBoxResult.No) return;

            try
            {
                using (SqlConnection conn = DatabaseConnectionFactory.CreateConnection())
                {
                    conn.Open();

                    // SQL query targeting the tracked TaskId
                    string query = "DELETE FROM dbo.Tasks WHERE TaskId = @TaskId;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", _selectedTaskId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Task deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                            // 3. Reset the entry form and tracking variable
                            TxtTaskTitle.Clear();
                            DpDueDate.SelectedDate = null;
                            CmbStatus.SelectedIndex = 0;
                            _selectedTaskId = -1;

                            // 4. Instantly refresh the grid
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

        private void LoadTasks()
        {
            try
            {
                using (SqlConnection conn = DatabaseConnectionFactory.CreateConnection())
                {
                    conn.Open();

                    // Query all tasks
                    string query = "SELECT TaskId, Title, DueDate, Status FROM dbo.Tasks;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Automated "Late" status logic:
                            // Loop through the rows to check if an active task has passed its due date
                            foreach (DataRow row in dt.Rows)
                            {
                                string currentStatus = row["Status"]?.ToString() ?? "";
                                DateTime dueDate = Convert.ToDateTime(row["DueDate"]);

                                if (currentStatus != "Completed" && dueDate < DateTime.Today)
                                {
                                    row["Status"] = "Late";
                                }
                            }

                            // Create a DataView to handle automatic sorting by Due Date
                            DataView dv = dt.DefaultView;
                            dv.Sort = "DueDate ASC";

                            // Bind the populated table directly to the DataGrid UI element
                            DgTasks.ItemsSource = dt.DefaultView;
                        }
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
            // If the grid tries to build a column for TaskId, cancel it immediately
            if (e.PropertyName == "TaskId")
            {
                e.Cancel = true;
            }
        }

        private void DgTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Temporary Debug Alert
            MessageBox.Show("Grid row clicked!");

            // If the user clears selection or the grid refreshes, do nothing
            if (DgTasks.SelectedItem == null) return;

            // Cast the selected item to a DataRowView (since we bound it to a DataTable/DataView)
            if (DgTasks.SelectedItem is DataRowView row)
            {
                // 1. Store the TaskId securely behind the scenes
                _selectedTaskId = Convert.ToInt32(row["TaskId"]);

                // 2. Populate the UI input fields with the current values
                TxtTaskTitle.Text = row["Title"]?.ToString() ?? "";
                DpDueDate.SelectedDate = Convert.ToDateTime(row["DueDate"]);

                // 3. Match the ComboBox text to the status
                string currentStatus = row["Status"]?.ToString() ?? "Not Started";

                // If your logic turned it into "Late", let's map it back to "In Progress" or "Not Started" 
                // depending on what options your ComboBox has, or just select it directly:
                foreach (ComboBoxItem item in CmbStatus.Items)
                {
                    if (item.Content.ToString() == currentStatus)
                    {
                        CmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
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