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
                if ( userId == 0 ) Session["userId"] = 0;
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

        //Start screen used to redirect user
        //Not logged -> /Home/Welcome
        //Logged profile not completed -> /User/WelcomeRegistered
        //Logged profile completed -> /Route/List
        [AllowAnonymous]
        public ActionResult Start()
        {
            //Non authenticated user -> Welcome
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Welcome");

            //Identified user 0, to see covoiturage route
            if (userId == 0) return RedirectToAction("List", "Route");

            //User is authenticated
            //Retrieve user's data
            User user;
            try
            {
                user = db.User.Find(userId);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new Error("Home", "Start", ex.Message + ex.InnerException.Message));
            }
            if (user == null) return RedirectToAction("Error", "Home", new Error("Home", "Start", Resources.Msg_error_db_user));

            //Incomplete user's profile -> WelcomeRegitered
            if (user.LocationLatitude == null || user.PictureVersion == null) return RedirectToAction("WelcomeRegistered", "User");

            //User's location set and user's picture loaded
            return RedirectToAction("List", "Route");
            
            //return View(); //View does not exist because this point is never reached
        }

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

        //public ActionResult Error()
        //{
        //    Error error = new Error("C", "A", "M");
        //    return View(error);
        //}

        [AllowAnonymous]
        public ActionResult Error(Error error)
        {
            return View(error);
        }

        public ActionResult Create(User user)
        {
            db.User.Add(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
