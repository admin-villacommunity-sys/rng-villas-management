using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

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
            // Read from environment variables or secret file
            var apiKey = _configuration["EmailSettings:ApiKey"]
                         ?? _configuration["EmailSettings__ApiKey"]
                         ?? _configuration["EmailSettings_ApiKey"]
                         ?? Environment.GetEnvironmentVariable("EmailSettings__ApiKey");

            Console.WriteLine($"--- Brevo API Key: {(string.IsNullOrEmpty(apiKey) ? "NULL or EMPTY" : "SET (masked)")} ---");

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key is missing. Please set EmailSettings__ApiKey in environment variables.");

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
                Console.WriteLine($"--- Brevo email error: {ex.Message} ---");
                throw;
            }
        }
    }
}