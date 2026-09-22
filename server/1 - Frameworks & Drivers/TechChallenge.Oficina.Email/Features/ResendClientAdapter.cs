using Resend;

namespace TechChallenge.Oficina.Email.Features;

public sealed class ResendClientAdapter : IResendClient
{
    private readonly IResend _resend;

    public ResendClientAdapter(IResend resend)
    {
        _resend = resend;
    }

    public async Task SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        await _resend.EmailSendAsync(emailMessage, cancellationToken);
    }
}
