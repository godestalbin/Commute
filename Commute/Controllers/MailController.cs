using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc.Mailer;
using System.Net.Mail;
using Commute.Models;
using Commute.Properties;

namespace Commute.Controllers
{
    public class MailController : BaseController
    {
        //Display mail sent as view - for testing purpose only

        //Welcome - test mail sent
        public ActionResult Welcome(int userId)
        {
            //Send mail as test
            //Mail mail = new Mail();
            //mail.Welcome(userId).Send();
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            User user = db.User.Find(userId);
            return View(user); //Display /Views/Welcome the view used to generate mail
        }

        //Contact - test mail sent
        public ActionResult Contact(int fromRouteId, int toRouteId)
        {
            //Send mail as test
            //Mail mail = new Mail();
            //mail.Contact(fromRouteId, toRouteId).Send();
            RouteCompare routeCompare = new RouteCompare(fromRouteId, toRouteId);
            //Path where picture are stored
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            return View(routeCompare); //Display /Views/Welcome the view used to generate mail
        }

        //Password - Password reset, send new password to user
        [AllowAnonymous]
        public ActionResult Password(User user)
        {
            //Send mail as test
            //Mail mail = new Mail();
            //mail.Contact(fromRouteId, toRouteId).Send();
            //Path where picture are stored
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            return View(user);
        }

        //Send contact mail
        public string MailContact(int fromRouteId, int toRouteId)
        {
            RouteCompare routeCompare = new RouteCompare(fromRouteId, toRouteId);
            Mail mail = new Mail();
            mail.Contact(fromRouteId, toRouteId).Send();
            return "OK";
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
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            return Populate(x =>
            {
                x.Subject = Resources.Welcome;
                x.ViewName = "Welcome"; //Views/Mail/Welcome
                x.To.Add(user.EmailAddress);
                x.Bcc.Add("godestalbin@leroymerlin.cn"); //send me a copy of the mail
            });
        }

        //Generate reset password mail
        [AllowAnonymous]
        public virtual MvcMailMessage Password(User user)
        {
            ViewData.Model = user; //Set the strongly typed object for the view
            //Path where picture are stored
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            return Populate(x =>
            {
                x.Subject = Resources.Password_reset;
                x.ViewName = "Password"; //Views/Mail/Pasword
                x.To.Add(user.EmailAddress);
                x.Bcc.Add("godestalbin@leroymerlin.cn"); //send me a copy of the mail
            });
        }

        //Generate contact mail
        public virtual MvcMailMessage Contact(int fromRouteId, int toRouteId)
        {
            RouteCompare routeCompare = new RouteCompare(fromRouteId, toRouteId);
            ViewData.Model = routeCompare; //Set the strongly typed object for the view
            //Path where picture are stored
            ViewBag.HttpPicture = System.Configuration.ConfigurationManager.AppSettings["Http.Picture"];
            return Populate(x =>
            {
                x.Subject = Resources.Mail_contact_mail_title;
                x.ViewName = "Contact"; //Views/Mail/Contact
                x.ReplyToList.Add(routeCompare.UserMail1); //from user
                x.To.Add(routeCompare.UserMail2); //to user
                x.Bcc.Add("godestalbin@leroymerlin.cn"); //send me a copy of the mail
            });

            //Example on how to use differente smtp client
            //http://stackoverflow.com/questions/9130277/how-set-up-different-stmpclient-instances-in-web-config
        }

    }
}
