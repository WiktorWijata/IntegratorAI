CREATE TABLE [dbo].[Version]
(
    [Version]     NVARCHAR(20)  NOT NULL PRIMARY KEY,
    [Description] NVARCHAR(255) NULL,
    [AppliedAt]   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
)
GO

CREATE SCHEMA [chat]
GO

CREATE SCHEMA [context]
GO

CREATE SCHEMA [providers]
GO

CREATE TABLE [chat].[Completion]
(
    [Id]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [CreatedAt] DATETIME2           NOT NULL DEFAULT GETUTCDATE()
)
GO

CREATE TABLE [context].[Context]
(
    [Id]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name]            NVARCHAR(255)       NOT NULL,
    [SystemRole]      NVARCHAR(MAX)       NOT NULL,
    [DomainContext]   NVARCHAR(MAX)       NULL,
    [DecisionPolicy]  NVARCHAR(MAX)       NULL,
    [OperatingRules]  NVARCHAR(MAX)       NULL,
    [OutputFormat]    NVARCHAR(MAX)       NULL,
    [CreatedAt]       DATETIME2           NOT NULL DEFAULT GETUTCDATE()
)
GO

CREATE TABLE [providers].[Provider]
(
    [Id]                 INT             NOT NULL PRIMARY KEY IDENTITY(1,1),
    [Type]               NVARCHAR(255)   NOT NULL,
    [PrimaryModel]       NVARCHAR(500)   NOT NULL,
    [SummarizationModel] NVARCHAR(500)   NULL,
    [IsActive]           BIT             NOT NULL DEFAULT 0
)
GO

CREATE TABLE [chat].[CompletionSummary]
(
    [CompletionId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
    [Content]             NVARCHAR(MAX)       NOT NULL,
    [SummarizedUpToIndex] INT                 NOT NULL,
    [CreatedAt]           DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    [ModifiedAt]          DATETIME2           NULL,
    CONSTRAINT [FK_CompletionSummary_Completion] FOREIGN KEY ([CompletionId]) REFERENCES [chat].[Completion]([Id])
)
GO

CREATE TABLE [chat].[Message]
(
    [Id]           INT                 NOT NULL PRIMARY KEY IDENTITY(1,1),
    [CompletionId] UNIQUEIDENTIFIER    NOT NULL,
    [Index]        INT                 NOT NULL,
    [Role]         NVARCHAR(50)        NOT NULL,
    [Content]      NVARCHAR(MAX)       NOT NULL,
    [CreatedAt]    DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Message_Completion] FOREIGN KEY ([CompletionId]) REFERENCES [chat].[Completion]([Id])
)
GO

CREATE INDEX [IX_Message_Index] ON [chat].[Message] ([Index])
GO

CREATE TABLE [context].[Example]
(
    [Id]               BIGINT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ContextId]        UNIQUEIDENTIFIER NOT NULL,
    [Input]            NVARCHAR(MAX)    NOT NULL,
    [ExpectedResponse] NVARCHAR(MAX)    NOT NULL,
    CONSTRAINT [FK_Example_Context] FOREIGN KEY ([ContextId]) REFERENCES [context].[Context]([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_Example_ContextId] ON [context].[Example] ([ContextId])
GO

CREATE TABLE [context].[Tool]
(
    [Id]          BIGINT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ContextId]   UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR(255)    NOT NULL,
    [Description] NVARCHAR(MAX)    NOT NULL,
    CONSTRAINT [FK_Tool_Context] FOREIGN KEY ([ContextId]) REFERENCES [context].[Context]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_Tool_ContextId] ON [context].[Tool] ([ContextId])
GO

CREATE TABLE [context].[ToolParameter]
(
    [Id]          BIGINT        NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ToolId]      BIGINT        NOT NULL,
    [Name]        NVARCHAR(255) NOT NULL,
    [Type]        NVARCHAR(50)  NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [FK_ToolParameter_Tool] FOREIGN KEY ([ToolId]) REFERENCES [context].[Tool]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_ToolParameter_ToolId] ON [context].[ToolParameter] ([ToolId])
GO

CREATE TABLE [context].[Guardrail]
(
    [Id]                   BIGINT        NOT NULL PRIMARY KEY IDENTITY(1,1),
    [ToolId]               BIGINT        NOT NULL,
    [Description]          NVARCHAR(MAX) NULL,
    [RequiresConfirmation] BIT           NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Guardrail_Tool] FOREIGN KEY ([ToolId]) REFERENCES [context].[Tool]([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_Guardrail_ToolId] ON [context].[Guardrail] ([ToolId])
GO

INSERT INTO [dbo].[Version] ([Version], [Description]) VALUES (N'1.0.0', N'Initial schema')
