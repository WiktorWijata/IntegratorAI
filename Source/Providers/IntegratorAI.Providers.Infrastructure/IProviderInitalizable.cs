namespace IntegratorAI.Providers.Infrastructure;

internal interface IProviderInitalizable
{
    string PrimaryModel { get; set; }
    string SummarizationModel { get; set; }

    void SetPrimaryModel(string model) => PrimaryModel = model;
    void SetSummarizationModel(string model) => SummarizationModel = model;
}
