-- FixDatabase.sql
-- This script adds missing columns to match the Entity Framework models

-- Check and add IdentityUserId to Students table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'IdentityUserId')
BEGIN
    ALTER TABLE [dbo].[Students] ADD [IdentityUserId] nvarchar(450) NULL;
END

-- Check and add FirstName to Students table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'FirstName')
BEGIN
    ALTER TABLE [dbo].[Students] ADD [FirstName] nvarchar(max) NULL;
END

-- Check and add LastName to Students table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'LastName')
BEGIN
    ALTER TABLE [dbo].[Students] ADD [LastName] nvarchar(max) NULL;
END

-- Check and add DOB to Students table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND name = 'DOB')
BEGIN
    ALTER TABLE [dbo].[Students] ADD [DOB] datetime2 NOT NULL DEFAULT GETDATE();
END

-- Check and add IdentityUserId to Instructors table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Instructors]') AND name = 'IdentityUserId')
BEGIN
    ALTER TABLE [dbo].[Instructors] ADD [IdentityUserId] nvarchar(450) NULL;
END

-- Check and add FirstName to Instructors table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Instructors]') AND name = 'FirstName')
BEGIN
    ALTER TABLE [dbo].[Instructors] ADD [FirstName] nvarchar(max) NULL;
END

-- Check and add LastName to Instructors table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Instructors]') AND name = 'LastName')
BEGIN
    ALTER TABLE [dbo].[Instructors] ADD [LastName] nvarchar(max) NULL;
END

-- Check and add GradeId to Enrollments table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') AND name = 'GradeId')
BEGIN
    ALTER TABLE [dbo].[Enrollments] ADD [GradeId] int NULL;
END

-- Add foreign key constraint for GradeId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Enrollments_Grades_GradeId')
BEGIN
    ALTER TABLE [dbo].[Enrollments]
    ADD CONSTRAINT [FK_Enrollments_Grades_GradeId] 
    FOREIGN KEY ([GradeId]) REFERENCES [dbo].[Grades] ([Id]) ON DELETE SET NULL;
END

-- Add index on GradeId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_GradeId')
BEGIN
    CREATE INDEX [IX_Enrollments_GradeId] ON [dbo].[Enrollments] ([GradeId]);
END
