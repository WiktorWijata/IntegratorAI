CREATE TABLE [dbo].[Message]
(
    [Id]            INT                 NOT NULL PRIMARY KEY IDENTITY(1,1),
    [CompletionId]  UNIQUEIDENTIFIER    NOT NULL,
    [Role]          NVARCHAR(50)        NOT NULL,
    [Content]       NVARCHAR(MAX)       NOT NULL,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Message_Completion] FOREIGN KEY ([CompletionId]) REFERENCES [dbo].[Completion]([Id])
)
