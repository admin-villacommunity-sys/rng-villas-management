using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VillaCommunityManagement.Services
{
    public class BrevoEmailService
    {
        private readonly IConfiguration _configuration;

        public BrevoEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async System.Threading.Tasks.Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _configuration["EmailSettings:ApiKey"]
                         ?? _configuration["EmailSettings__ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key is missing.");

            var apiInstance = new TransactionalEmailsApi();
            apiInstance.Configuration.ApiKey.Add("api-key", apiKey);

            var sender = new SendSmtpEmailSender(
                _configuration["EmailSettings:SenderName"] ?? "RNG Supra Villas",
                _configuration["EmailSettings:SenderEmail"] ?? "admin.villacommunity@gmail.com"
            );

            var to = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) };

            var sendSmtpEmail = new SendSmtpEmail(
                sender: sender,
                to: to,
                subject: subject,
                htmlContent: body
            );

            try
            {
                await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Brevo email error: {ex.Message}");
                throw; // re-throw the exception
            }
        }
    }
}