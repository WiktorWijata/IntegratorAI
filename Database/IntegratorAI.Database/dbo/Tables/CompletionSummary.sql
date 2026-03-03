CREATE TABLE [dbo].[CompletionSummary]
(
	[CompletionId]          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
	[Content]               NVARCHAR(MAX)       NOT NULL,
	[SummarizedUpToIndex]   INT                 NOT NULL,
	[CreatedAt]             DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
	[ModifiedAt]            DATETIME2           NULL,
	CONSTRAINT [FK_CompletionSummary_Completion] FOREIGN KEY ([CompletionId]) REFERENCES [dbo].[Completion]([Id])
)
