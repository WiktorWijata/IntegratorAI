CREATE TABLE [dbo].[Tool]
(
	[Id]			BIGINT				NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ContextId]		UNIQUEIDENTIFIER	NOT NULL,
	[Name]			NVARCHAR(255)		NOT NULL,
	[Description]	NVARCHAR(MAX)		NOT NULL,

	FOREIGN KEY ([ContextId]) REFERENCES [dbo].[Context]([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_Tool_ContextId] ON [dbo].[Tool] ([ContextId])
