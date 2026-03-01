namespace IntegratorAI.Providers.Infrastructure;

internal interface IProviderInitalizable
{
    string Model { get; set; }

    void SetModel(string model) => Model = model;
}
