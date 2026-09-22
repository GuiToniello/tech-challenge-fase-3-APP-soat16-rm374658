using Resend;

namespace TechChallenge.Oficina.Email.Features;

public interface IResendClient
{
    Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
