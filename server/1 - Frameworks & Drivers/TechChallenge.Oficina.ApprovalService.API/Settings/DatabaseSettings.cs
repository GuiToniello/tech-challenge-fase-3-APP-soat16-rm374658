namespace TechChallenge.Oficina.ApprovalService.API.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    public string ConnectionString { get; init; } = string.Empty;
}
