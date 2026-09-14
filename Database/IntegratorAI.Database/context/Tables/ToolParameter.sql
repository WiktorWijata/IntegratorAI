CREATE TABLE [context].[ToolParameter]
(
	[Id]            BIGINT          NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ToolId]        BIGINT          NOT NULL,
	[Name]          NVARCHAR(255)   NOT NULL,
	[Type]          NVARCHAR(50)    NOT NULL,
	[Description]   NVARCHAR(MAX)   NOT NULL,
	CONSTRAINT [FK_ToolParameter_Tool] FOREIGN KEY ([ToolId]) REFERENCES [context].[Tool] ([Id]) ON DELETE CASCADE
)

GO

CREATE NONCLUSTERED INDEX [IX_ToolParameter_ToolId] ON [context].[ToolParameter] ([ToolId])
