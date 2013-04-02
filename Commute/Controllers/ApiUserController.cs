using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Commute.Models;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;

namespace Commute.Controllers
{
    /// Commute API for users

    /// This API is called by Commute android
    public class ApiUserController : ApiController
    {
        private Context db = new Context();

        // GET api/ApiUser
        //public IEnumerable<User> GetUsers()
        //{
        //    return db.User.AsEnumerable();
        //}

        //Check user password
        //0=user/password does not match, otherwise return User.Id
        [HttpGet]
        public int CheckUser(String userName, String password)
        {
            User user;
            try
            {
                user = (from u in db.User
                        where u.Account == userName
                        select u).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return 0; //Database failure or other
            }

            if (user == null) return 0; //Unknown account
            else if (user.Password == Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new System.Text.UTF8Encoding().GetBytes(password ?? "")))) return user.Id; 
            else return 0; //Wrong password
        }

        // GET api/ApiUser/5
        public User GetUser(int id)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return user;
        }

        // PUT api/ApiUser/5
        public HttpResponseMessage PutUser(int id, User user)
        {
            //Retrieve user in database
            User dbUser = db.User.Find(id);
            if (dbUser != null)
            {
                //Currently only these 2 fields can be updated in Commute android
                dbUser.Name = user.Name;
                dbUser.EmailAddress = user.EmailAddress;
            }
            //Original code
            if (id == user.Id) //ModelState.IsValid &&
            {
                db.Entry(dbUser).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public int CreateUser(User user)
            ///xx=Success, account created, return user id
            ///-1=Failure, account already exists
            ///-2=Data model validation failed
        {
            //Check account does not already exist
            if (ModelState.IsValid)
            {
                try
                {
                    //Encode user password
                    user.Password = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(new System.Text.UTF8Encoding().GetBytes(user.Password ?? "")));
                    db.User.Add(user);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return -1; //Account already exists or other database error
                }
                try
                {
                    //Send welcome mail to user
                    Mail mail = new Mail();
                    mail.Welcome(user.Id).Send();
                }
                catch (Exception e)
                {
                }

                return user.Id; //Success
            }

            return -2; //Data model validation failed
        }

        /// Upload user picture
        /// Sample call: /api/apiuser/1
        public Task<string> PostUploadFile(int id)
        //<IEnumerable<string>>
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(Request.CreateResponse(
                HttpStatusCode.NotAcceptable,
                "This request is not properly formatted"));
            }

            try
            {
                var streamProvider = new MultipartMemoryStreamProvider();
                var task = Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith<string>(t =>
                      {
                          if (t.IsFaulted || t.IsCanceled)
                          {
                              throw new HttpResponseException(HttpStatusCode.InternalServerError);
                          }

                          string addedId = streamProvider.Contents.Select(i =>
                          {
                              Stream stream = i.ReadAsStreamAsync().Result;
                            string version = Upload2Cloudinary(stream, String.Format("{0:00000000}", id));
                            User user;
                            user = db.User.Find(id);
                            user.PictureVersion = version;
                            db.SaveChanges();
                            return version;
                          }).First();
                          return addedId;
                      });
                return task;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateResponse(
                HttpStatusCode.NotAcceptable,
                "Error: " + ex.Message));
            }
        }

        // DELETE api/ApiUser/5
        public HttpResponseMessage DeleteUser(int id)
        {
            User user = db.User.Find(id);
            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.User.Remove(user);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private string Upload2Cloudinary(Stream file, string filename)
        {
            var settings = ConfigurationManager.AppSettings;
            CloudinaryDotNet.Account cloudinaryAccount = new CloudinaryDotNet.Account(settings["Cloudinary.CloudName"],
                                                                                      settings["Cloudinary.ApiKey"],
                                                                                      settings["Cloudinary.ApiSecret"]);
            string PublicId = Path.GetFileNameWithoutExtension(filename);

            CloudinaryDotNet.Transformation et = new CloudinaryDotNet.Transformation().Angle(new string[] { "exif" }).Width(100).Height(100).Crop("thumb").Gravity("face").Radius(8);
            CloudinaryDotNet.Transformation et2 = new CloudinaryDotNet.Transformation().Effect("grayscale");
            CloudinaryDotNet.Actions.ImageUploadParams uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams()
            {
                File = new CloudinaryDotNet.Actions.FileDescription(filename, file),
                PublicId = PublicId,
                //Format = "png", //That makes the file much bigger 3,5Kb -> 55Kb
                //EagerTransforms = new List<CloudinaryDotNet.Transformation>{ et },
                Transformation =  et
                //Exif = true //Rotate automatically according to EXIF data
            };
            CloudinaryDotNet.Cloudinary cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryAccount);
            cloudinary.DeleteResources(new string[] { PublicId });
            CloudinaryDotNet.Actions.ImageUploadResult uploadResult = cloudinary.Upload(uploadParams);
            return uploadResult.Version;
        }

    }
}