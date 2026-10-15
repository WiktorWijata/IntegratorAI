-- Make OpenRouter the answering provider: HuggingFace has no credit left.
-- Runs once per database; the active provider is read from this table (cached in Redis
-- under providers:active for an hour).
UPDATE [providers].[Provider] SET [IsActive] = 0
GO

IF EXISTS (SELECT 1 FROM [providers].[Provider] WHERE [Type] = N'OpenRouter')
    UPDATE [providers].[Provider]
    SET [PrimaryModel] = N'google/gemma-4-31b-it:free', [SummarizationModel] = NULL, [IsActive] = 1
    WHERE [Type] = N'OpenRouter'
ELSE
    INSERT [providers].[Provider] ([Type], [PrimaryModel], [SummarizationModel], [IsActive])
    VALUES (N'OpenRouter', N'google/gemma-4-31b-it:free', NULL, 1)
GO
