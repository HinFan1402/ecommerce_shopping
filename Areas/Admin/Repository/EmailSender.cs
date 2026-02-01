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

            var mail = new MailMessage
            {
                From = new MailAddress("phanhienhd1402@gmail.com", "Support Team"), // có thể đổi tên hiển thị
                Subject = subject,
                Body = message,
                IsBodyHtml = true // ✅ Thêm dòng này
            };

            mail.To.Add(email);

            return client.SendMailAsync(mail);
        }
    }
}
