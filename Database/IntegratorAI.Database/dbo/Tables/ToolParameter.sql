CREATE TABLE [dbo].[ToolParameter]
(
	[Id]			INT				NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ToolId]		INT				NOT NULL,
	[Name]			NVARCHAR(255)	NOT NULL,
	[Type]			NVARCHAR(50)	NOT NULL,
	[Description]	NVARCHAR(MAX)	NOT NULL,

	FOREIGN KEY ([ToolId]) REFERENCES [dbo].[Tool]([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_ToolParameter_ToolId] ON [dbo].[ToolParameter] ([ToolId])
