﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;


namespace Abdulhalim.Pages
{
    public class IndexModel : PageModel
    {
            public class ContactFormModel
            {
                [Required]
                public string Name { get; set; }

                [Required]
                [EmailAddress]
                public string Email { get; set; }

                [Required]
                [Phone]
                public string PhoneNumber { get; set; }

                [Required]
                public string Message { get; set; }
            }

            public class EmailService
            {
                private readonly ILogger<EmailService> _logger;

                public EmailService(ILogger<EmailService> logger)
                {
                    _logger = logger;
                }

                public async Task<bool> SendEmailAsync(string fromEmail, string fromName, string fromPhoneNumber, string toEmail, string subject, string message)
                {
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
                    emailMessage.To.Add(new MailboxAddress("", toEmail));
                    emailMessage.Subject = subject;

                    var body = $"Name: {fromName}\nEmail: {fromEmail}\nPhone Number: {fromPhoneNumber}\n\nMessage:\n{message}";

                    emailMessage.Body = new TextPart("plain")
                    {
                        Text = body
                    };

                    using (var client = new SmtpClient())
                    {
                        try
                        {
                            await client.ConnectAsync("smtp.your-email-provider.com", 587, false);
                            await client.AuthenticateAsync("your-email@example.com", "your-email-password");
                            await client.SendAsync(emailMessage);
                            await client.DisconnectAsync(true);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while sending the email.");
                            return false;
                        }
                    }
                }
            }

            private readonly EmailService _emailService;

            public IndexModel(ILogger<EmailService> logger)
            {
                _emailService = new EmailService(logger);
            }

            [BindProperty]
            public ContactFormModel Contact { get; set; }

            public void OnGet()
            {
                // This method is intentionally left empty
            }

            public async Task<IActionResult> OnPostAsync()
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var emailSent = await _emailService.SendEmailAsync(
                    Contact.Email,
                    Contact.Name,
                    Contact.PhoneNumber,
                    "recipient@example.com",
                    "Contact Form Message",
                    Contact.Message);

                if (emailSent)
                {
                    return RedirectToPage("ThankYou");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "There was an error sending your email. Please try again later.");
                    return Page();
                }
            }
        }


    
}
