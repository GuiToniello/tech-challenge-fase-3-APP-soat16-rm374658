namespace TechChallenge.Oficina.Email.Configuration;

public sealed class ResendSettings
{
    public const string SectionName = "ResendSettings";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public bool SendEmailOnStatusChange { get; set; } = false;
}
