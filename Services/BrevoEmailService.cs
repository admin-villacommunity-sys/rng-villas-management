using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

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
            // ============================================================
            // READ API KEY FROM SECRET FILE (most reliable on Render)
            // ============================================================
            string apiKey = null;

            // Try to read from the secret file
            try
            {
                if (File.Exists("brevo.key"))
                {
                    apiKey = File.ReadAllText("brevo.key").Trim();
                    Console.WriteLine("--- API key loaded from brevo.key file ---");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- Failed to read brevo.key: {ex.Message} ---");
            }

            // Fallback: try environment variables
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = _configuration["EmailSettings:ApiKey"]
                         ?? _configuration["EmailSettings__ApiKey"]
                         ?? _configuration["EmailSettings_ApiKey"]
                         ?? Environment.GetEnvironmentVariable("EmailSettings__ApiKey");
                Console.WriteLine($"--- API key loaded from environment: {(string.IsNullOrEmpty(apiKey) ? "NULL" : "SET")} ---");
            }

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key is missing. Please create brevo.key secret file or set EmailSettings__ApiKey.");

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