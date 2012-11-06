using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Commute.Models;
using Commute.Properties;

namespace Commute.Controllers
{
    public class UserController : Controller
    {
        private Context db = new Context();
        
        //Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User userLogin)
        {
            User user = (from u in db.User
                         where u.Account == userLogin.Account
                         select u).FirstOrDefault();
            if (user != null && user.Password == userLogin.Password)
            {
                System.Web.Security.FormsAuthentication.SetAuthCookie(user.Account,false); //false no persistent cookie
            }
            return View();
        }

        //Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Edit", "User", user.Id); //Show user data in Edit view
            }
            return View();
        }

        //Edit
        public ActionResult Edit(int id = 0)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound(); //HTTP Error 404 - Not very friendly
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return View(user);  //RedirectToAction("Index");
            }
            return View(user);
        }

        //Upload (post only)
        [HttpPost]
        public ActionResult UploadFile()
        {
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file] as HttpPostedFileBase;
                if (postFile.ContentLength == 0)
                    continue;
                string savedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Resources.Dir_image_upload, String.Format("{0:00000000}", Request.RequestContext.RouteData.Values["id"]) + Path.GetExtension(postFile.FileName));
                postFile.SaveAs(savedFileName);

                //Need to save the extension in database if we want to display different type of files.
            }
            return RedirectToAction("Edit", Request.RequestContext.RouteData.Values["id"]);
        }


        //-----------------------------------------
        //Default controller action auto generated - not used

        //
        // GET: /User/

        public ActionResult Index()
        {
            return View(db.User.ToList());
        }

        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }


        //
        // GET: /User/Delete/5

        public ActionResult Delete(int id = 0)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.User.Find(id);
            db.User.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}