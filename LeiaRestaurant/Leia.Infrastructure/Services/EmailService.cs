using Leia.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Leia.Infrastructure.Services
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var from = config["Email:From"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Leia", from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(config["Email:Host"], int.Parse(config["Email:Port"] ?? "587"), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(from, config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}