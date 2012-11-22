using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Commute.Models;
using Commute.Properties;

namespace Commute.Controllers
{
    public class BaseController : Controller
    {
        public readonly Context db = new Context();
        protected int userId;
        protected string userName;

        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            if (User.Identity.IsAuthenticated)
            {
                userName = User.Identity.Name;
                Session["userName"] = User.Identity.Name;
                ViewBag.userName = Session["userName"];
                if (Session["userId"] == null) //Need to get user ID from database
                {
                    User user = (from u in db.User
                                 where u.Account == User.Identity.Name
                                 select u).FirstOrDefault();
                    Session["userId"] = user.Id;
                }
                ViewBag.userId = Session["userId"];
                userId = (int)Session["userId"]; ;
            }
            else {
                userId = 0;
                userName = "";
                //Session["userName"] = "a"; //null;
                //Session["userId"] = 1; //null;
                //ViewBag.userName = Session["userName"];
                //ViewBag.userId = Session["userId"];
            }
        }
    }

    public class HomeController : BaseController
    {
        //private readonly Context _context = new Context();

        [AllowAnonymous]
        public ActionResult Welcome()
        {
            //ViewBag.Title = Resources.Welcome; done in the view
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home/Index";
            //if (User.Identity.IsAuthenticated) ViewBag.userName = User.Identity.Name;
            return View(db.User);
        }

        [AllowAnonymous]
        public ActionResult NotImplemented()
        {
            return PartialView();
        }

        public ActionResult Create(User user)
        {
            db.User.Add(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
