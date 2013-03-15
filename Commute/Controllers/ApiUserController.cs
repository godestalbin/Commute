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
            if (ModelState.IsValid && id == user.Id)
            {
                db.Entry(user).State = EntityState.Modified;

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

        // POST api/ApiUser
        //public HttpResponseMessage PostUser(User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.User.Add(user);
        //        db.SaveChanges();

        //        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, user);
        //        response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = user.Id }));
        //        return response;
        //    }
        //    else
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}

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
    }
}