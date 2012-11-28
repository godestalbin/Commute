using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Commute.Controllers
{
    public class MailController : Controller
    {
        public ActionResult Send(string to="godestalbin@leroymerlin.cn", string title="mail title", string body="mail body") {

            try
            {
                var message = new MailMessage("godestalbin@gmail.com", to, "Mail title", "Body");
                // here is an important part:
                message.From = new MailAddress("godestalbin@gmail.com", "Commute");
                // it's superfluous part here since from address is defined in .config file
                // in my example. But since you don't use .config file, you will need it.

                var client = new SmtpClient();
                client.EnableSsl = true;
                client.Timeout = 900000;
                client.Send(message);
                ViewBag.ErrorMessage = "Success: Mail sent";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }

            return View();
        }

    }
}
