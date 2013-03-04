using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects.SqlClient;
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
        //get all routes for the specified user - not used currently (HttpActivity). Could be useful to update local database
        public IEnumerable<Route> GetRoutes(int userId)
        {
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            return routeList.AsEnumerable();
            //return db.Route.AsEnumerable();
        }

        //Get the list of route matching the route with id matchingRouteId
        //api/ApiRoute?matchingRouteId=xx
        public IEnumerable<RouteSearch> GetMatchingRoute(int matchingRouteId, int maxDist = 500)
        {
            //Route searchRoute = db.Route.Find(matchingRouteId);
            Route searchRoute = (from r in db.Route
                                 where r.RouteId == matchingRouteId
                                 select r).FirstOrDefault();
            if (searchRoute == null) return null;

            //decimal maxDist = 5; //Distance approximation allowed in kilometers
            double startLat = Math.PI * (double)searchRoute.StartLatitude / 180.0;
            double startLng = Math.PI * (double)searchRoute.StartLongitude / 180.0;
            double endLat = Math.PI * (double)searchRoute.EndLatitude / 180.0;
            double endLng = Math.PI * (double)searchRoute.EndLongitude / 180.0;

            var routeList = from r in db.Route
                            where SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371 < (double)maxDist &&
                            SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 < (double)maxDist //&&
                            //r.UserId != searchRoute.UserId //TMP allow to display same user route for debugging
                            select new RouteSearch { Id = r.RouteId, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371, EndDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 };

            return routeList.AsEnumerable();

        }

        // GET api/ApiRoute/5 - not used currently
        //api/ApiRoute/xx - Maybe it should be moved before GetMatchingRoute to get it working
        //api/ApiRoute?matchingRouteId=xx
        public Route GetRoute(int id)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return route;
        }

        // PUT api/ApiRoute/5 - Not used currently
        public HttpResponseMessage PutRoute(int id, Route route)
        {
            if (ModelState.IsValid && id == route.RouteId)
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
        //Create or update route: local db -> central db
        public HttpResponseMessage PostRoute(Route route)
        {
            if (ModelState.IsValid)
            {
                Route dbRoute = ( from r in db.Route
                                where r.RouteId == route.RouteId
                                  select r).FirstOrDefault();
                if (dbRoute == null)
                    db.Route.Add(route);
                else //Update existing route    
                {
                    dbRoute.Name = route.Name;
                    dbRoute.IsOffer = route.IsOffer;
                    //We do not update distance when it is zero. Current db value might be right and new value might be wrong (issue to access Google API)
                    if (route.Distance > 0)
                        dbRoute.Distance = route.Distance;
                    dbRoute.StartLatitude = route.StartLatitude;
                    dbRoute.StartLongitude = route.StartLongitude;
                    dbRoute.EndLatitude = route.EndLatitude;
                    dbRoute.EndLongitude = route.EndLongitude;
                }
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, route);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = route.RouteId }));
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