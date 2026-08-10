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
            string apiKey = null;

            // Try multiple locations for the secret file
            string[] possiblePaths = {
                "/etc/secrets/brevo.key",
                "brevo.key",
                "/app/brevo.key"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        apiKey = File.ReadAllText(path).Trim();
                        Console.WriteLine($"--- API key loaded from: {path} ---");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- Error reading {path}: {ex.Message} ---");
                }
            }

            // Fallback: try environment variables
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = _configuration["EmailSettings:ApiKey"]
                         ?? _configuration["EmailSettings__ApiKey"]
                         ?? _configuration["EmailSettings_ApiKey"]
                         ?? Environment.GetEnvironmentVariable("EmailSettings__ApiKey");
                Console.WriteLine($"--- API key from environment: {(string.IsNullOrEmpty(apiKey) ? "NULL" : "SET")} ---");
            }

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key is missing. Please create brevo.key secret file or set EmailSettings__ApiKey.");

            var apiInstance = new TransactionalEmailsApi();

            // ✅ FIX: Use indexer instead of Add to avoid "key already exists" error
            apiInstance.Configuration.ApiKey["api-key"] = apiKey;

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