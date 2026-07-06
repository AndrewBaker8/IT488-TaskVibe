using System;
using System.Collections.Generic;
using TaskVibe.UI.Models;

namespace TaskVibe.UI.Repositories
{
    public interface ITaskRepository
    {
        // For US-02: Task Creation Entry
        bool AddTask(TaskItem task);

        // For US-06: Task Overview List UI
        IEnumerable<TaskItem> GetAllTasks();
        IEnumerable<TaskItem> GetTasksByUserId(int userId);

        // For US-03 & US-04: Deadline Management and Backlog Status Mutation
        bool UpdateTaskStatus(int taskId, string status);
        bool UpdateTaskDeadline(int taskId, DateTime newDeadline);

        // For US-05: Task Record Purging (Delete)
        bool DeleteTask(int taskId);
    }
}