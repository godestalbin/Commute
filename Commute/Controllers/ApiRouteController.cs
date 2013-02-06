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

namespace Commute.Controllers
{
    public class ApiRouteController : ApiController
    {
        private Context db = new Context();

        // GET api/ApiRoute
        //get all routes for the specified user
        public IEnumerable<Route> GetRoutes(int userId)
        {
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            return routeList.AsEnumerable();
            //return db.Route.AsEnumerable();
        }

        // GET api/ApiRoute/5
        public Route GetRoute(int id)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return route;
        }

        // PUT api/ApiRoute/5
        public HttpResponseMessage PutRoute(int id, Route route)
        {
            if (ModelState.IsValid && id == route.Id)
            {
                db.Entry(route).State = EntityState.Modified;

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

        // POST api/ApiRoute
        public HttpResponseMessage PostRoute(Route route)
        {
            if (ModelState.IsValid)
            {
                db.Route.Add(route);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, route);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = route.Id }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/ApiRoute/5
        public HttpResponseMessage DeleteRoute(int id)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Route.Remove(route);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, route);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}