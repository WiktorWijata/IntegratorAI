-- Answer with NVIDIA Nemotron instead of Gemma: Gemma's free tier is often rate-limited upstream (429).
-- Runs once per database; the active provider is read from this table (cached in Redis under
-- providers:active for an hour). A database without an OpenRouter row is left alone.
UPDATE [providers].[Provider]
SET [PrimaryModel] = N'nvidia/nemotron-3-super-120b-a12b:free'
WHERE [Type] = N'OpenRouter'
GO
