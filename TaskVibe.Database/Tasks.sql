CREATE TABLE [dbo].[Tasks]
(
    [TaskId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [DueDate] DATETIME NOT NULL,
    [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'In Process',
    [AssignedToUserId] INT NULL,

    -- Table Constraints
    CONSTRAINT [FK_Tasks_Users] FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users]([UserId]),
    CONSTRAINT [CHK_TaskStatus] CHECK ([Status] IN ('Completed', 'In Process', 'Late'))
)