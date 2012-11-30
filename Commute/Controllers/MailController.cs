using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc.Mailer;
using Commute.Models;
using System.Net.Mail;

namespace Commute.Controllers
{
    public class MailController : BaseController
    {
        //Display mail sent as view - for testing purpose only
        public ActionResult Welcome(int userId)
        {
            //Send mail as test
            //Mail mail = new Mail();
            //mail.Welcome().Send();
            User user = db.User.Find(userId);
            return View(user); //Display /Views/Welcome the view used to generate mail
        }

        //Send manually a mail
        public ActionResult Send(string to = "godestalbin@leroymerlin.cn", string title = "mail title", string body = "mail body")
        {

            try
            {
                var message = new MailMessage(); //from is taken from the web.config
                message.Subject = title;
                message.Body = body;
                message.To.Add(to);

                var client = new SmtpClient();
                //client.EnableSsl = true; //moved to Web.config
                client.Timeout = 900000; //Default time out is 100000 (=100 seconds);
                client.Send(message);
                ViewBag.ErrorMessage = "Success: Mail sent to " + message.To[0] + " by " + message.From;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }

            return View();
        }

    }

    public class Mail : MailerBase
    {
        private Context db = new Context();
        //Generate welcome mail
        public virtual MvcMailMessage Welcome(int userId)
        {
            User user = db.User.Find(userId); //Retrieve user
            ViewBag.account = user.Account;
            ViewData.Model = user; //Set the strongly typed object for the view
            return Populate(x =>
            {
                x.Subject = "Welcome";
                x.ViewName = "Welcome"; //Views/Mail/Welcome
                x.To.Add(user.EmailAddress);
            });
        }

        ////Welcome mail
        //public virtual MvcMailMessage Welcome() {
        //return Populate(x =>{
        //  x.viewName = "Welcome";
        //  x.To.Add("sohan39@example.com");
        //});

    }
}
