using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Models;
using VillaCommunityManagement.Services;
using System.Security.Cryptography;
using BCrypt.Net;

namespace VillaCommunityManagement.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public LoginController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ==========================================
        // LOGIN
        // ==========================================

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(AdminLogin login)
        {
            var admin = _context.AdminLogins
                .FirstOrDefault(x => x.Username == login.Username && x.IsActive);

            if (admin != null && BCrypt.Net.BCrypt.Verify(login.Password, admin.Password))
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("Username", admin.Username);
                HttpContext.Session.SetString("AdminId", admin.AdminId.ToString());

                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid Username or Password";
            return View(login);
        }

        // ==========================================
        // FORGOT PASSWORD
        // ==========================================

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _context.AdminLogins
                .FirstOrDefault(x => x.Email == model.Email && x.IsActive);

            if (admin == null)
            {
                ViewBag.Error = "No account found with this email address.";
                return View(model);
            }

            // Generate a reset token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            admin.PasswordResetToken = token;
            admin.PasswordResetTokenExpiry = DateTime.Now.AddHours(24);

            await _context.SaveChangesAsync();

            // Send email with reset link
            var resetLink = Url.Action("ResetPassword", "Login", new { token, email = admin.Email }, Request.Scheme);

            string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f5;padding:20px;'>
<div style='max-width:600px;background:white;padding:30px;border-radius:8px;border:1px solid #ddd;'>
<h2 style='color:#1f4e79;'>Password Reset Request</h2>
<hr/>
<p>Hello <strong>{admin.Username}</strong>,</p>
<p>We received a request to reset your password for the <strong>RNG Supra Villas Management System</strong>.</p>
<p>Click the link below to reset your password:</p>
<p><a href='{resetLink}' style='background:#1A4D8C;color:white;padding:12px 24px;border-radius:8px;text-decoration:none;display:inline-block;'>Reset Password</a></p>
<p>This link will expire in 24 hours.</p>
<p>If you did not request this, please ignore this email.</p>
<br/>
Regards,<br/>
<strong>RNG Supra Villas Management Team</strong>
</div>
</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(admin.Email, "Password Reset - RNG Supra Villas", body);
                ViewBag.Success = "Password reset link has been sent to your email.";
            }
            catch
            {
                ViewBag.Error = "Failed to send email. Please try again later.";
            }

            return View();
        }

        // ==========================================
        // RESET PASSWORD
        // ==========================================

        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _context.AdminLogins
                .FirstOrDefault(x =>
                    x.Email == model.Email &&
                    x.PasswordResetToken == model.Token &&
                    x.PasswordResetTokenExpiry > DateTime.Now &&
                    x.IsActive);

            if (admin == null)
            {
                ViewBag.Error = "Invalid or expired reset token.";
                return View(model);
            }

            // Hash the new password
            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            admin.PasswordResetToken = null;
            admin.PasswordResetTokenExpiry = null;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // This will show the real error
                throw new Exception($"Database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            // Notify user
            string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f9;padding:20px;'>
<div style='max-width:600px;background:white;padding:30px;border-radius:8px;border:1px solid #ddd;'>
<h2 style='color:#1f4e79;'>Password Changed Successfully</h2>
<hr/>
<p>Hello <strong>{admin.Username}</strong>,</p>
<p>Your password has been successfully changed.</p>
<p>If you did not make this change, please contact the system administrator immediately.</p>
<br/>
Regards,<br/>
<strong>RNG Supra Villas Management Team</strong>
</div>
</body>
</html>";

            await _emailService.SendEmailAsync(admin.Email, "Password Changed - RNG Supra Villas", body);

            ViewBag.Success = "Password has been reset successfully. Please login with your new password.";
            return View("ResetPasswordConfirmation");
        }

        // ==========================================
        // RESET PASSWORD CONFIRMATION
        // ==========================================

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ==========================================
        // VIEWER ACCESS
        // ==========================================

        public IActionResult Viewer()
        {
            HttpContext.Session.SetString("UserRole", "Viewer");
            HttpContext.Session.SetString("Username", "Viewer");

            return RedirectToAction("Index", "Dashboard");
        }

        // ==========================================
        // ACCESS DENIED
        // ==========================================

        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}