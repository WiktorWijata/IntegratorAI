CREATE TABLE [context].[Example]
(
	[Id]                BIGINT              NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ContextId]         UNIQUEIDENTIFIER    NOT NULL,
	[Input]             NVARCHAR(MAX)       NOT NULL,
	[ExpectedResponse]  NVARCHAR(MAX)       NOT NULL,
	CONSTRAINT [FK_Example_Context] FOREIGN KEY ([ContextId]) REFERENCES [context].[Context] ([Id]) ON DELETE CASCADE
)

GO

CREATE NONCLUSTERED INDEX [IX_Example_ContextId] ON [context].[Example] ([ContextId])
