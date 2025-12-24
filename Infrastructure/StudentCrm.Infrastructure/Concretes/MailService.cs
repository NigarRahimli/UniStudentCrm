using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Student.Domain.HelperEntities;
using StudentCrm.Application.Abstract.Services;
using StudentCrm.Application.GlobalAppException;

namespace StudentCrm.Infrastructure.Concretes
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            if (mailRequest == null)
                throw new GlobalAppException("MailRequest boşdur!");

            if (string.IsNullOrWhiteSpace(mailRequest.ToEmail))
                throw new GlobalAppException("ToEmail boş ola bilməz!");

            if (string.IsNullOrWhiteSpace(_mailSettings.Mail))
                throw new GlobalAppException("MailSettings:Mail (From email) boşdur! appsettings-i yoxla.");

            if (string.IsNullOrWhiteSpace(_mailSettings.Host))
                throw new GlobalAppException("MailSettings:Host boşdur! appsettings-i yoxla.");

            if (_mailSettings.Port <= 0)
                throw new GlobalAppException("MailSettings:Port yanlışdır! appsettings-i yoxla.");

            if (string.IsNullOrWhiteSpace(_mailSettings.Password))
                throw new GlobalAppException("MailSettings:Password boşdur! appsettings-i yoxla.");

            var fromEmail = _mailSettings.Mail.Trim();
            var toEmail = mailRequest.ToEmail.Trim();

            var email = new MimeMessage();

            // ✅ Recommended: From (not only Sender)
            email.From.Add(new MailboxAddress(
                string.IsNullOrWhiteSpace(_mailSettings.DisplayName) ? "StudentCRM" : _mailSettings.DisplayName.Trim(),
                fromEmail
            ));

            // optional: set Sender too
            email.Sender = MailboxAddress.Parse(fromEmail);

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = mailRequest.Subject ?? "";

            var builder = new BodyBuilder
            {
                HtmlBody = mailRequest.Body ?? ""
            };

            // Attachments
            if (mailRequest.Attachments != null)
            {
                foreach (var file in mailRequest.Attachments)
                {
                    if (file == null || file.Length <= 0) continue;

                    await using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();

                    builder.Attachments.Add(
                        file.FileName,
                        fileBytes,
                        ContentType.Parse(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType)
                    );
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(fromEmail, _mailSettings.Password);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
