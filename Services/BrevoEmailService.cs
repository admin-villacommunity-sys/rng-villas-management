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
            // Log all available configuration keys (for debugging)
            Console.WriteLine("--- Available configuration keys (first 20): ---");
            var allKeys = new List<string>();
            foreach (var child in _configuration.GetChildren())
            {
                allKeys.Add(child.Key);
                Console.WriteLine($"  Key: {child.Key}, Value: {child.Value ?? "(null)"}");
            }
            Console.WriteLine($"--- Total keys found: {allKeys.Count} ---");

            // Try multiple naming conventions for the API key
            var apiKey = _configuration["EmailSettings:ApiKey"]
                         ?? _configuration["EmailSettings__ApiKey"]
                         ?? _configuration["EmailSettings_ApiKey"]
                         ?? _configuration["EmailSettings.ApiKey"];

            Console.WriteLine($"--- Brevo API Key: {(string.IsNullOrEmpty(apiKey) ? "NULL or EMPTY" : "SET (masked)")} ---");

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Brevo API key is missing. Please set EmailSettings__ApiKey or EmailSettings_ApiKey in environment variables.");

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