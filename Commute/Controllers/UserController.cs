using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security; //Form authentication
using System.IO; //Upload file (write file on server)
using Commute.Models;
using Commute.Properties;
using Amazon.S3.Model;
using Amazon.S3;

namespace Commute.Controllers
{
    public class UserController : BaseController
    {
        private Context db = new Context();
        
        //Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LoginMobile(User userLogin)
        {
            User user = (from u in db.User
                         where u.Account == userLogin.Account
                         select u).FirstOrDefault();
            if (user == null) ModelState.AddModelError("Account", Resources.Error_unknown_account);
            else if (user.Password == userLogin.Password)
            {
                FormsAuthentication.SetAuthCookie(user.Account,false); //false no persistent cookie
                Session["userId"] = user.Id;
                return RedirectToAction("ListMobile", "Route", new { userId = user.Id }); //Later should be Home/Index
            }
            else ModelState.AddModelError("Password", Resources.Error_wrong_password);
            return View();
        }

        //LoginMobile
        [AllowAnonymous]
        public ActionResult LoginMobile()
        {
            return View();
        }

        //Logoff
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(User user)
        {
            //Check account is free
            int count = db.User.Count(u => u.Account == user.Account);
            if ( count > 0 ) {
                ModelState.AddModelError("Account", Resources.Error_duplicate_account);
                return View();
            }
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
        public ActionResult UploadFile(Entity entity)
        {
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase postFile = Request.Files[file] as HttpPostedFileBase;
                if (postFile.ContentLength == 0)
                    continue;
                //Save on the server - Cannot be used for App Harbour
                //string savedFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + Resources.Dir_image_upload, String.Format("{0:00000000}", entity.Id ) + Path.GetExtension(postFile.FileName));
                //postFile.SaveAs(savedFileName);

                //Need to save the extension in database if we want to display different type of files.

                //Now save to Amazon S3
                UploadToAmazonService(postFile, String.Format("{0:00000000}", entity.Id) + Path.GetExtension(postFile.FileName));
            }
            return RedirectToAction("Edit", new {id=entity.Id});
        }

        private void UploadToAmazonService(HttpPostedFileBase file, string filename)
        {
            string bucketName = System.Configuration.ConfigurationManager.AppSettings["AWSPublicFilesBucket"]; //commute bucket

            string publicFile = "Pictures/" + filename; //We have Pictures folder in the bucket

            PutObjectRequest request = new PutObjectRequest();
            request.WithBucketName(bucketName);
            request.WithKey(publicFile);
            request.WithInputStream(file.InputStream);
            request.AutoCloseStream = true;
            request.CannedACL = S3CannedACL.PublicRead; //Read access for everyone

            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(); //uses AWSAccessKey and AWSSecretKey defined in Web.config
            using (S3Response r = client.PutObject(request)) { }
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