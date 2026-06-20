CREATE TABLE [providers].[Provider]
(
	[Id]                 INT             NOT NULL PRIMARY KEY IDENTITY(1,1),
	[Type]               NVARCHAR(255)   NOT NULL,
	[PrimaryModel]       NVARCHAR(500)   NOT NULL,
	[SummarizationModel] NVARCHAR(500)   NULL,
	[IsActive]           BIT             NOT NULL DEFAULT 0
)
