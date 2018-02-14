using System.Net;
using System.Net.Mail;

namespace Identity.Models
{
    public static class Utility
    {
        public static void SendMail(MailModel objModelMail)
        {
            var mail = new MailMessage();
            mail.To.Add(objModelMail.To);
            mail.From = new MailAddress("email");
            mail.Subject = objModelMail.Subject;
            mail.Body = objModelMail.Body;
            mail.IsBodyHtml = true;
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("email", "password")
            };
            smtp.Send(mail);
        }
    }
}
