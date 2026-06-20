CREATE TABLE [context].[Tool]
(
	[Id]            BIGINT              NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ContextId]     UNIQUEIDENTIFIER    NOT NULL,
	[Name]          NVARCHAR(255)       NOT NULL,
	[Description]   NVARCHAR(MAX)       NOT NULL,
	CONSTRAINT [FK_Tool_Context] FOREIGN KEY ([ContextId]) REFERENCES [context].[Context] ([Id]) ON DELETE CASCADE
)

GO

CREATE NONCLUSTERED INDEX [IX_Tool_ContextId] ON [context].[Tool] ([ContextId])
