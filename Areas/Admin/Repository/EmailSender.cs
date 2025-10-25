
using System.Net;
using System.Net.Mail;
namespace ecommerce_shopping.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SenderEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("phanhienhd1402@gmail.com", "ssmtcmzoagxcqkgn")
            };
            return client.SendMailAsync(
                new MailMessage(
                from: "phanhienhd1402@gmail.com",
                to: email,
                subject,
                message
                ));
        }
    }
}
