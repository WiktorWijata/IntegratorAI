CREATE TABLE [dbo].[Example]
(
	[Id]				BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ContextId]			UNIQUEIDENTIFIER NOT NULL,
	[Input]				NVARCHAR(MAX) NOT NULL,
	[ExpectedResponse]	NVARCHAR(MAX) NOT NULL,

	CONSTRAINT [FK_Example_Context] FOREIGN KEY ([ContextId]) REFERENCES [dbo].[Context] ([Id]) ON DELETE CASCADE,
)

GO

CREATE NONCLUSTERED INDEX [IX_Example_ContextId] ON [dbo].[Example] ([ContextId])