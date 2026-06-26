IF NOT EXISTS (SELECT 1 FROM [dbo].[Users])
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email]) VALUES 
    ('AndrewB', 'andrew@taskvibe.com'),
    ('JulianM', 'julian@taskvibe.com'),
    ('TaMarraW', 'tamarra@taskvibe.com');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks])
BEGIN
    INSERT INTO [dbo].[Tasks] ([Title], [Description], [DueDate], [Priority], [Status], [AssignedToUserId]) VALUES 
    ('Setup WPF Main Window', 'Initialize core UI layout structure.', '2026-07-01', 'High', 'In Process', 1),
    ('Verify Database Schema', 'Execute script and run initial testing queries.', '2026-06-28', 'High', 'Completed', 3),
    ('Create C# Repositories', 'Write data access logic for CRUD operations.', '2026-07-06', 'High', 'In Process', 2);
END