-- Tool calling: a completion remembers its context (to know the tools), a message can carry the tools the
-- assistant asked for or the id of the call a tool message answers, and a tool parameter can be required.
ALTER TABLE [chat].[Completion] ADD [ContextId] UNIQUEIDENTIFIER NULL
GO

ALTER TABLE [chat].[Message] ADD [ToolCallsJson] NVARCHAR(MAX) NULL, [ToolCallId] NVARCHAR(200) NULL
GO

ALTER TABLE [context].[ToolParameter] ADD [Required] BIT NOT NULL CONSTRAINT [DF_ToolParameter_Required] DEFAULT 0
GO
