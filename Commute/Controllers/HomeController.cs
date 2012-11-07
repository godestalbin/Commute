using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Commute.Models;

namespace Commute.Controllers
{
    public class HomeController : Controller
    {
        private readonly Context _context = new Context();

        public ActionResult Index()
        {
            ViewBag.Title = "Home/Index";
            if (User.Identity.IsAuthenticated) ViewBag.userName = User.Identity.Name;
            return View(_context.User);
        }

        public ActionResult Create(User user)
        {
            _context.User.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
