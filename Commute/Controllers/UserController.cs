using System;
using System.Configuration;
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
using Cloudinary;
using System.Security.Cryptography;
using System.Text;

namespace Commute.Controllers
{
    public class UserController : BaseController
    {
        //private Context db = new Context();
        
        //Login
        [AllowAnonymous]
        public ActionResult LoginOld()
        {
            return View();
        }

        //LoginMobile
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(User userLogin)
        {
            User user;
            try
            {
                user = (from u in db.User
                        where u.Account == userLogin.Account
                        select u).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new Error("User", "Login", ex.Message + ex.InnerException.Message));
            }
            if (user == null) ModelState.AddModelError("Account", Resources.Error_unknown_account);
            //Check password is right
            else if (user.Password == Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(userLogin.Password ?? ""))))
            {
                FormsAuthentication.SetAuthCookie(user.Account,true); //true=Persistent cookie
                Session["userId"] = user.Id;
                //Check if we have a return URL (user attempted to access screen without beeing authenticated)
                string[] returnUrl = HttpUtility.UrlDecode(Request.UrlReferrer.Query).Split('=');
                string[] controllerAction = null;
                if (returnUrl.Length == 2) controllerAction = returnUrl[1].Split('/');
                //Go back to the return URL
                if (controllerAction != null && controllerAction.Length > 1) return RedirectToAction(controllerAction[2], controllerAction[1]);
                //No return URL go to the Start screen to redirect user
                else return RedirectToAction("Start", "Home"); //, new { userId = user.Id }); //Later should be Home/Index
            }
            else ModelState.AddModelError("Password", Resources.Error_wrong_password);
            return View();
        }

        //Logoff
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Welcome", "Home");
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
            if ( count > 0 && user.Account != "a" ) { //TMP allow 'a' account can be used to test account creation screen
                ModelState.AddModelError("Account", Resources.Error_duplicate_account);
                return View();
            }
            if (ModelState.IsValid)
            {
                if (user.Account != "a") //TMP 'a' account is not re-created
                {
                    //Computer password hash
                    user.Password = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(user.Password ?? "")));
                    db.User.Add(user);
                    db.SaveChanges();
                }
                else user.Id = 1; //TMP need to set user Id for 'a' account

                //Authenticate user
                FormsAuthentication.SetAuthCookie(user.Account, true); //true=Persistent cookie
                Session["userId"] = user.Id;

                //TMP
                //Go to /User/WelcomeRegistered screen
                //return RedirectToAction("WelcomeRegistered", new { mailJustSent = 1 });

                //Send welcome mail to user
                Mail mail = new Mail();
                mail.Welcome(user.Id).Send();

                //Go to /User/WelcomeRegistered screen
                return RedirectToAction("WelcomeRegistered", new { mailJustSent = 1 });
            }
            return View();
        }

        //Welcome message for registred users
        public ActionResult WelcomeRegistered(int mailJustSent = 0)
        {
            if (userId == 0) RedirectToAction("Welcome", "Home");
            User user = db.User.Find(userId);
            ViewBag.MailJustSent = mailJustSent; //Tell the view to display message that mail was just sent
            return View(user);
        }

        //Edit
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Edit()
        {
            User user = db.User.Find(userId);
            if (user == null)
            {
                return HttpNotFound(); //HTTP Error 404 - Not very friendly
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Edit(User postUser)
        {
            if (ModelState.IsValid)
            {
                //Retrieve current user
                //In the postUser we don't have all user's data
                User user;
                try
                {
                    user = (from u in db.User
                            where u.Account == postUser.Account
                            select u).FirstOrDefault();
                    //db.Entry(user).State = EntityState.Modified;
                    user.Name = postUser.Name;
                    user.EmailAddress = postUser.EmailAddress;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error", "Home", new Error("User", "Edit", ex.Message + ex.InnerException.Message));
                }
 
                return View(user);  //RedirectToAction("Index");
            }
            return View(postUser);
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

                //Upload to Cloudinary
                string version = UploadToCloudinary(postFile, String.Format("{0:00000000}", entity.Id) + Path.GetExtension(postFile.FileName));
                //Save picture version
                User user;
                user = db.User.Find(entity.Id);
                user.PictureVersion = version;
                db.SaveChanges();

                //Now save to Amazon S3
                //UploadToAmazonService(postFile, String.Format("{0:00000000}", entity.Id) + Path.GetExtension(postFile.FileName));
            }
            return RedirectToAction("Edit");
        }

        private string UploadToCloudinary(HttpPostedFileBase file, string filename)
        {
            var settings = ConfigurationManager.AppSettings;
            var configuration = new AccountConfiguration(settings["Cloudinary.CloudName"],
                                                         settings["Cloudinary.ApiKey"],
                                                         settings["Cloudinary.ApiSecret"]);
            var uploader = new Uploader(configuration);
            string publicId = Path.GetFileNameWithoutExtension(filename);
            // Upload the file
            var destroyResult = uploader.Destroy(publicId);
            var uploadResult = uploader.Upload(new UploadInformation(filename, file.InputStream)
                                {
                                    // explicitly specify a public id (optional)
                                    PublicId = publicId,
                                    // set the format, (default is jpg)
                                    Format = "png",
                                    // Specify some eager transformations                                                        
                                    Eager = new[]
                                    {
                                        new Transformation(100, 100) { Format = "png", Crop = CropMode.Thumb, Gravity = Gravity.Face, Radius = 8 }, //, Angle = new Angle(90)
                                    }
                                });
            return uploadResult.Version;
        }

        private void UploadToAmazonService(HttpPostedFileBase file, string filename)
        {
            string bucketName = System.Configuration.ConfigurationManager.AppSettings["AWSPublicFilesBucket"]; //commute bucket

            string publicFile = "Pictures/" + filename; //We have Pictures folder in the bucket
            Session["publicFile"] = publicFile;

            PutObjectRequest request = new PutObjectRequest();
            request.WithBucketName(bucketName);
            request.WithKey(publicFile);
            request.WithInputStream(file.InputStream);
            request.AutoCloseStream = true;
            request.CannedACL = S3CannedACL.PublicRead; //Read access for everyone

            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(); //uses AWSAccessKey and AWSSecretKey defined in Web.config
            using (S3Response r = client.PutObject(request)) { }
        }

        //UserPicture
        public ActionResult UserPicture() {
            User user = new User();
            return View(user);
        }

        public ActionResult UploadAgain()
        {
            User user = new User();
            ViewBag.UploadAgain = "Yes";
            return View("UserPicture", user);
        }

        public ActionResult SetLocation()
        {
            if (userId == 0) RedirectToAction("Welcome", "Home");
            User user = db.User.Find(userId);
            return View(user);
        }

        [HttpPost]
        public ActionResult SetLocation(User postUser) //int id, decimal locationLatitude, decimal locationLongitude) //
        {
            User user = db.User.Find(postUser.Id);
            user.LocationLatitude = postUser.LocationLatitude;
            user.LocationLongitude = postUser.LocationLongitude;
            db.SaveChanges();

            return RedirectToAction("List", "Route");
        }

        //Get user default location
        public string Location(int id)
        {
            User user = db.User.Find(id);
            return user.LocationLatitude.ToString() + "/" + user.LocationLongitude.ToString();
        }

        //Change password
        public ActionResult Password()
        {
            User user;
            try
            {
                user = db.User.Find(userId);
            }
            catch( Exception ex) {
                return RedirectToAction("Error", "Home", new Error("User", "Password", ex.Message + ex.InnerException.Message));
            }
            if (user == null) return RedirectToAction("Error", "Home", new Error("User", "Password", Resources.Msg_error_db_user));

            UserPassword userPassword = new UserPassword();
            userPassword.Id = user.Id;
            userPassword.Account = user.Account;
            return View(userPassword);
        }

        [HttpPost]
        public ActionResult Password(UserPassword userPassword)
        {
            //Retrieve current user
            User user;
            try
            {
                user = db.User.Find(userId);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new Error("User", "Password", ex.Message + ex.InnerException.Message));
            }
            if (user == null) return RedirectToAction("Error", "Home", new Error("User", "Password", Resources.Msg_error_db_user));

            //Control current password
            if (user.Password != Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(userPassword.OldPassword ?? ""))))
            {
                ModelState.AddModelError("OldPassword", Resources.Error_wrong_password);
                return View(userPassword);
            }
            
            //Save updated password to database
            user.Password = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(userPassword.Password)));
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new Error("User", "Password", ex.Message + (ex.InnerException != null  ? ex.InnerException.Message : "")));
            }

            //Confirm password update to user
            return RedirectToAction("PasswordUpdated");
        }

        //Confirm password update to user
        public ActionResult PasswordUpdated()
        {
            return View();
        }

        //Reset user's password
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            //Retrieve current user
            User user = new User();
            
            return View(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(User postUser)
        {
            //Retrieve current user
            User user;
            try
            {
                user = (from u in db.User
                        where u.Account == postUser.Account
                        select u).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new Error("User", "ResetPassword", ex.Message + ex.InnerException.Message));
            }
            //Account not found
            if (user == null) ModelState.AddModelError("Account", Resources.Error_unknown_account);

            //Control mail match the one registered for this account
            else if (user.EmailAddress != postUser.EmailAddress) ModelState.AddModelError("EmailAddress", Resources.Error_wrong_mail);

            //Password is mandatory we removed from ModelState
            ModelState.Remove("Password");

            //Generate a new password - password is mandatory in the model
            string password = Membership.GeneratePassword(12, 1);

            if (ModelState.IsValid)
            {
                //Update user password
                try
                {
                    user.Password = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(password)));
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error", "Home", new Error("User", "ResetPassword", ex.Message + ex.InnerException.Message));
                }

                //Send new reset password mail
                user.Password = password; //we need to send to user the password not the hash we saved to database
                Mail mail = new Mail();
                mail.Password(user).Send();
                return RedirectToAction("Login");
                //return RedirectToAction("Password", "Mail", user);
            }
            else return View(user); //Cannot send mail
        }

        //-----------------------------------------
        //Default controller action auto generated - not used

        //
        // GET: /User/

        public ActionResult List()
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