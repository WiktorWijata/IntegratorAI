CREATE TABLE [chat].[Message]
(
	[Id]            INT                 NOT NULL PRIMARY KEY IDENTITY(1,1),
	[CompletionId]  UNIQUEIDENTIFIER    NOT NULL,
	[Index]         INT                 NOT NULL,
	[Role]          NVARCHAR(50)        NOT NULL,
	[Content]       NVARCHAR(MAX)       NOT NULL,
	[CreatedAt]     DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
	CONSTRAINT [FK_Message_Completion] FOREIGN KEY ([CompletionId]) REFERENCES [chat].[Completion] ([Id])
)

GO

CREATE INDEX [IX_Message_Index] ON [chat].[Message] ([Index])
