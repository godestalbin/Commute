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
using System.Web.Http.Controllers;
using System.IO;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Globalization;

namespace Commute.Controllers
{
    /// Route API called by Commute Android

    /// The API URL is http://commute.apphb.com/api/apiroute
    /// Get request action should be prefixed by get
    public class ApiRouteController : ApiController
    {
        private Context db = new Context();

        // GET api/ApiRoute
        /// Get all routes for the specified user - not used currently (HttpActivity). Could be useful to update local database
        public IEnumerable<Route> GetRoutes(int userId)
        {
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            return routeList.AsEnumerable();
            //return db.Route.AsEnumerable();
        }

        /// Get the list of route matching the route with id matchingRouteId

        /// Example call: api/ApiRoute?matchingRouteId=xx&maxDist=xx
        /// Default maxDist is set to 5 km
        /// matchingRouteId is actuallty the route id to match
        public IEnumerable<RouteSearch> GetMatchingRoute(int matchingRouteId, int maxDist = 5)
        {
            IEnumerable<RouteSearch> routeMatchList = searchMatchingRoute(matchingRouteId, maxDist);

            ////Route searchRoute = db.Route.Find(matchingRouteId);
            //Route searchRoute = (from r in db.Route
            //                     where r.RouteId == matchingRouteId
            //                     select r).FirstOrDefault();
            //if (searchRoute == null) return null;

            ////decimal maxDist = 5; //Distance approximation allowed in kilometers
            //double startLat = Math.PI * (double)searchRoute.StartLatitude / 180.0;
            //double startLng = Math.PI * (double)searchRoute.StartLongitude / 180.0;
            //double endLat = Math.PI * (double)searchRoute.EndLatitude / 180.0;
            //double endLng = Math.PI * (double)searchRoute.EndLongitude / 180.0;

            //var routeList = from r in db.Route
            //                where SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371 < (double)maxDist &&
            //                SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 < (double)maxDist &&
            //                r.UserId != searchRoute.UserId //TMP allow to display same user route for debugging
            //                select new RouteSearch { Id = r.RouteId, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371, EndDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 };

            //Sort the result by smallest distance first
            var routeListSorted = from t in routeMatchList orderby t.StartDistance, t.EndDistance select t;

            return routeListSorted.AsEnumerable();

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

        /// Send a mail to inform a user with a matching route wish to contact

        /// Result 0=success, 1=error (sending mail failed) \n
        /// Call example: http://commute.apphb.com/api/apiroute?routeId=1&matchingRouteId=2
        public int GetSendMatchingRouteMail(int routeId, int matchingRouteId)
        {
            try
            {
                //Send welcome mail to user
                Mail mail = new Mail();
                mail.Contact(routeId, matchingRouteId).Send();
                return 0; //Success
            }
            catch ( Exception e ) {
                return 1; //Something wrong happened
            }
        }

        /// PUT api/ApiRoute/5 - Not used currently
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
        /// Create or update route: local db -> central db
        
        /// If the route is not found in database it will be created, otherwise it is updated
        public HttpResponseMessage PostRoute(Route route)
        {
            if (ModelState.IsValid)
            {
                Route dbRoute = ( from r in db.Route
                                where r.RouteId == route.RouteId
                                  select r).FirstOrDefault();
                if (dbRoute == null)
                {
                    db.Route.Add(route);
                    getRouteFromCovoiturageFr(route);
                    searchMatchingRoute(route);
                }
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

        /// Delete route

        /// DELETE api/ApiRoute/5
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

        //TMP API to call getRouteFromCovoiturageFr
        public void getRouteCovoiturage(decimal lat, decimal lng)
        {
            Route route = new Route();
            route.StartLatitude = lat; //47.90320650
            route.StartLongitude = lng; //1.90921890
            route.EndLatitude = 50.629250M; //Lille
            route.EndLongitude = 3.0572560M;

            getRouteFromCovoiturageFr(route);
        }

        /// Use the route provided to create new routes using data from covoiturage.fr
         
        /// Convert the route start/end lat/lng to city
        /// Query covoiturage.fr with this start.end city and take first non club route
        /// Save this route for UserId=0
        private void getRouteFromCovoiturageFr(Route route)
        {
            CityGeoCode startCity = getLocalityFromGoogle(route.StartLatitude ?? 0, route.StartLongitude ?? 0);
            CityGeoCode endCity = getLocalityFromGoogle(route.EndLatitude ?? 0, route.EndLongitude ?? 0);

            //We got some city as start/end
            String routeLink = "";
            if (startCity != null && endCity != null)
                routeLink = getLinkFromCovoiturageFr(startCity.city, endCity.city);
            if (routeLink != "") //Create a new covoiturage route
            {
                try
                {
                    //Get last ID
                    var userRoute = from r in db.Route where r.UserId == 0 select r;
                    int id;
                    if (userRoute.Any()) id = userRoute.Max(r => r.RouteId);
                    else id = 0;
                    if (id == 999) id = 0; //Currently we are limited to 999 route per user
                    id += 1; //increment RouteId
                    //Set new route data
                    Route newRoute = new Route();
                    newRoute.RouteId = id;
                    newRoute.Name = startCity.city + " - " + endCity.city;
                    newRoute.IsOffer = true;
                    newRoute.UserId = 0;
                    newRoute.StartLatitude = startCity.lat; //Later should the city lat
                    newRoute.StartLongitude = startCity.lng;
                    newRoute.EndLatitude = endCity.lat;
                    newRoute.EndLongitude = endCity.lng;
                    newRoute.CovoiturageLink = routeLink;
                    newRoute.Distance = 0; //Need to get a Google route to know it
                    //Add the route in dabase
                    db.Route.Add(newRoute);
                    db.SaveChanges();
                }
                catch (Exception e) { }
            }
        }

        /// Convert lat/lng to city using Google geocode
        private CityGeoCode getLocalityFromGoogle(decimal lat, decimal lng)
        {
            string url = "http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&sensor=false";
            CityGeoCode cityGeoCode = new CityGeoCode(); //result returned
            WebResponse response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                response = request.GetResponse();
                if (response != null)
                {
                    string str = null;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            str = streamReader.ReadToEnd();
                        }
                    }

                    GeoResponse geoResponse = JsonConvert.DeserializeObject<GeoResponse>(str);
                    if (geoResponse.status == "OK")
                    {
                        //Iterate the results
                        int coutResult = geoResponse.results.Length;
                        for ( int k = 0; k < coutResult; k++)
                        {
                            if (geoResponse.results[k].types[0] == "locality" ) {
                                //Iterate the address_components
                                int count = geoResponse.results[k].address_components.Length;
                                for (int i = 0; i < count; i++)
                                {
                                    //Iterate the types
                                    int typeCount = geoResponse.results[k].address_components[i].types.Length;
                                    for (int j = 0; j < typeCount; j++)
                                        if (geoResponse.results[k].address_components[i].types[j] == "locality")
                                        {
                                        
                                            cityGeoCode.city = geoResponse.results[k].address_components[i].long_name;
                                            cityGeoCode.lat = geoResponse.results[k].Geometry.Location.Lat;
                                            cityGeoCode.lng = geoResponse.results[k].Geometry.Location.Lng;
                                            return cityGeoCode;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("JSON response failed, status is '{0}'", geoResponse.status);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //If we get here it means we did not get locality from Google
            return cityGeoCode;
        }

        /// Query covoiturage.fr
        private String getLinkFromCovoiturageFr(String startLocality, String endLocality) {
                //Request to covoiturage.fr
                WebResponse response = null;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.covoiturage.fr/recherche?fc=" + startLocality + "&tc=" + endLocality + "&to=BOTH&p=1&n=20&t=tripsearch&a=searchtrip ");
                    request.Method = "GET";
                    response = request.GetResponse();
                    if (response != null)
                    {
                        string html = null;
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                html = streamReader.ReadToEnd();
                            }
                        }
                        //Keep only the part after the non club member
                        int index = html.IndexOf("search-results search-results-v2");
                        if ( index == -1 ) return "";
                        html = html.Substring(index);
                        index = html.IndexOf("href=\"/trajet"); //Find the firs href
                        if ( index == -1 ) return "";
                        html = html.Substring(index + 6); //Cut before the link
                        index = html.IndexOf("\""); //Find the first " (end of the link)
                        html = html.Substring(0, index);

                        return html;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            return "";
            }
        
        /// Search matching route for the specified route and send mail to users

        /// Search database for matching route
        /// Send mail to the user (skip route collected from covoiturage.fr)
        private void searchMatchingRoute(Route route)
        {
            IEnumerable<RouteSearch> routeMatchList = searchMatchingRoute(route.RouteId);

            //Send mail to user for all matches found
            foreach ( RouteSearch matchRoute in routeMatchList ) {
                if ( matchRoute.Id > 999 ) //Send mail only if this is not a route from covoiturage.fr
                    GetSendMatchingRouteMail(route.RouteId, matchRoute.Id);
            }
        }

        private IEnumerable<RouteSearch> searchMatchingRoute(int matchingRouteId, int maxDist = 5)
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
                            SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 < (double)maxDist &&
                            r.UserId != searchRoute.UserId
                            select new RouteSearch { Id = r.RouteId, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371, EndDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 };

            return routeList;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }

    ///Class to store Google geocode API usefull result
    public class CityGeoCode
    {
        public string city { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }

    }

    /// Class to decode Google geocode API JSON response
    public class GeoLocation
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }

    public class GeoGeometry
    {
        public GeoLocation Location { get; set; }
    }

    public class GeoTypes
    {
        public String types { get; set; }
    }

    public class GeoAddressComponents
    {
        public String long_name { get; set; }
        public String short_name { get; set; }
        public String[] types { get; set; }
    }

    public class GeoResult
    {
        public GeoAddressComponents[] address_components { get; set; }
        public GeoGeometry Geometry { get; set; }
        public String[] types { get; set; }
    }

    public class GeoResponse
    {
        public string status { get; set; }
        public GeoResult[] results { get; set; }
    }

}