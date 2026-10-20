-- Answer with NVIDIA Nemotron 3 Ultra: the free Nemotron 3 Super endpoint started to answer 404 on the provider's side.
-- Runs once per database; the active provider is read from this table (cached in Redis under
-- providers:active for an hour). A database without an OpenRouter row is left alone.
UPDATE [providers].[Provider]
SET [PrimaryModel] = N'nvidia/nemotron-3-ultra-550b-a55b:free'
WHERE [Type] = N'OpenRouter'
GO
