CREATE TABLE [context].[Guardrail]
(
	[Id]                    BIGINT          NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ToolId]                BIGINT          NOT NULL,
	[Description]           NVARCHAR(MAX)   NULL,
	[RequiresConfirmation]  BIT             NOT NULL DEFAULT 0,
	CONSTRAINT [FK_Guardrail_Tool] FOREIGN KEY ([ToolId]) REFERENCES [context].[Tool] ([Id]) ON DELETE CASCADE
)

GO

CREATE NONCLUSTERED INDEX [IX_Guardrail_ToolId] ON [context].[Guardrail] ([ToolId])
