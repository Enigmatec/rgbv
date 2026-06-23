using CSharpFunctionalExtensions;
using Humanizer;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Service
{
    /// <summary>
    /// Handles the email service
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly SendGridClient _sendGridClient;
        private readonly IRazorViewToStringRenderer _renderer;

        public EmailService(IOptions<SendGridSettings> options, IRazorViewToStringRenderer renderer)
        {
            _sendGridClient = new SendGridClient(options.Value.Key);
            _renderer = renderer;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
            var to = new EmailAddress(email, " ");

            var msg = MailHelper.CreateSingleEmail(from: from, to: to, subject: subject, htmlContent: htmlMessage, plainTextContent: "");
            await _sendGridClient.SendEmailAsync(msg);
        }

        public async Task<(bool, string)> CustomClientMail(string Email, List<string> CCList, string subject, string htmlMessage)
        {
            var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
            var to = new EmailAddress(Email, " ");

            var CCEmails = new List<EmailAddress>();
            foreach (var email in CCList)
            {
                CCEmails.Add(new EmailAddress(email));
            }

            var message = new SendGridMessage
            {
                From = from,
                ReplyTo = to,
                HtmlContent = htmlMessage,
                Subject = subject,
                Personalizations = new List<Personalization>
                {
                    new Personalization
                    {
                       Ccs = CCEmails,
                       Subject = subject,
                    }
                }
            };

            try
            {
                var result = await _sendGridClient.SendEmailAsync(message);

                if (result.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    return (false, "Email was not sent");
                }
            }
            catch (Exception e)
            {
                return (false, $"Email was not sent, {e.Message}");
            }

            return (true, "Email sent");
        }

        public async Task<(bool, string)> TemplateSendAsync<T>(string recipient, string subject, string templateName, TemplateModel<T> model)
        {
            var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
            var to = new EmailAddress(recipient, "");

            try
            {
                var htmlBody = await GetHtmlTemplateAsync<T>(templateName, model);

                var message = MailHelper.CreateSingleEmail(from: from, to: to, subject: subject, htmlContent: htmlBody, plainTextContent: "");

                var result = await _sendGridClient.SendEmailAsync(message);

                if (result.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    return (false, "Email was not sent");
                }
            }
            catch (Exception e)
            {
                return (false, $"Email was not sent, {e.Message}");
            }

            return (true, "Email sent");
        }

        public async Task<(bool, string)> SendCases(UserViewModel recipient, byte[] stream, string filename)
        {
            var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
            var to = new EmailAddress(recipient.Email, "");
            try
            {
                var message = new SendGridMessage()
                {
                    From = from,
                    Subject = "Cases",
                    PlainTextContent = "Hello",
                    HtmlContent = $"Hello {recipient.FirstName}, <br>This is the information that you requested for."

                };
                message.AddTo(to);
                var file = Convert.ToBase64String(stream);
                message.AddAttachment(filename, file, "application/xlsx");

                var result = await _sendGridClient.SendEmailAsync(message);

                if (result.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    return await SendErrorToAdmin(recipient, result.StatusCode.ToString());
                }
            }
            catch (Exception e)
            {
                return (false, $"Email was not sent, {e.Message}");
            }

            return (true, "Email sent");
        }

        public async Task<(bool, string)> SendErrorToAdmin(UserViewModel recipient, string errorMessage)
        {
            try
            {
                var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
                var adminEmail = new EmailAddress(EmailKeys.AdminEmail, "");
                var failedMessage = new SendGridMessage()
                {
                    From = from,
                    Subject = "Request for cases failure",
                    PlainTextContent = "Hello",
                    HtmlContent = $"User {recipient.FirstName} with  email {recipient.Email} requested for cases and there was a failure  <br>Error: {errorMessage}"
                };
                failedMessage.AddTo(adminEmail);
                //await _sendGridClient.SendEmailAsync(failedMessage);
                return (false, "Email was not sent");
            }
            catch (Exception e)
            {
                return (false, $"Email was not sent, {e.Message}");
            }
        }
        public async Task<(bool, string)> TemplateSendAsync<T>(string recipient, List<string> CCList, string subject, string templateName, TemplateModel<T> model)
        {
            var from = new EmailAddress(EmailKeys.NoReplyEmail, StartupKeys.ReportGBV);
            var to = new EmailAddress(recipient, " ");

            var CCEmails = new List<EmailAddress>();
            foreach (var email in CCList)
            {
                CCEmails.Add(new EmailAddress(email));
            }

            try
            {
                var htmlBody = await GetHtmlTemplateAsync<T>(templateName, model);

                var message = new SendGridMessage
                {
                    From = from,
                    HtmlContent = htmlBody,
                    Subject = subject,
                    Personalizations = new List<Personalization>
                   {
                        new Personalization
                        {
                           Ccs = CCEmails,
                           Subject = subject,
                           Tos = new List<EmailAddress> {to},
                        }
                    }
                };

                var result = await _sendGridClient.SendEmailAsync(message);

                if (result.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    return (false, "Email was not sent");
                }
            }
            catch (Exception e)
            {
                return (false, $"Email was not sent, {e.Message}");
            }

            return (true, "Email sent");
        }

        private async Task<string> GetHtmlTemplateAsync<T>(string templateName, TemplateModel<T> model)
        {
            string view = $"~/Views/MailTemplates/{templateName}.cshtml";
            var htmlBody = await _renderer.RenderViewToStringAsync(view, model);

            return htmlBody;
        }
    }
}