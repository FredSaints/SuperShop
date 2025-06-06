using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;

namespace SuperShop.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Response SendEmail(string to, string subject, string body)
        {
            var nameFrom = _configuration["Mail:Namefrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var portString = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            if (!int.TryParse(portString, out int port))
            {               
                throw new FormatException($"Mail:Port configuration ('{portString}') is not a valid integer.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodybuilder = new BodyBuilder()
            {
                HtmlBody = body
            };
            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    SecureSocketOptions options = SecureSocketOptions.Auto;
                    if (port == 587)
                    {
                        options = SecureSocketOptions.StartTls;
                    }
                    else if (port == 465)
                    {
                        options = SecureSocketOptions.SslOnConnect;
                    }

                    client.Connect(smtp, port, options);

                    if (!string.IsNullOrEmpty(password))
                    {
                        client.Authenticate(from, password);
                    }

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {

                return new Response
                {
                    IsSucess = false,
                    Message = ex.ToString()
                };
            }

            return new Response
            {
                IsSucess = true
            };
        }
    }
}