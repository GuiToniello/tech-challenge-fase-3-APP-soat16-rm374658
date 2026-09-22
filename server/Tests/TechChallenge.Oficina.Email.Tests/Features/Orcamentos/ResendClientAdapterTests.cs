using Moq;
using Resend;
using TechChallenge.Oficina.Email.Features;
using Xunit;

namespace TechChallenge.Oficina.Email.Tests.Features.Orcamentos;

public sealed class ResendClientAdapterTests
{
    [Fact]
    public async Task SendEmailAsync_DeveDelegarParaResend()
    {
        var resendMock = new Mock<IResend>();
        var adapter = new ResendClientAdapter(resendMock.Object);
        var message = new EmailMessage
        {
            From = "onboarding@resend.dev",
            To = "cliente@teste.com",
            Subject = "Assunto",
            HtmlBody = "<p>teste</p>"
        };

        await adapter.SendEmailAsync(message);

        resendMock.Verify(r => r.EmailSendAsync(message), Times.Once);
    }
}
