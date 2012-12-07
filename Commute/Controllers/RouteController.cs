using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using Commute.Models;
using Commute.Properties;

namespace Commute.Controllers
{
    public class RouteController : BaseController
    {
        private Context db = new Context();

        //Display route
        [AllowAnonymous]
        public ActionResult View(int id, int routeId)
        //id=route2 (the matching route (from other users) found), routeId=route1 (the user's 
        {
            //Route route2 = db.Route.Find(id); //Retrieve route
            //Route route1 = db.Route.Find(routeId); //Retrieve route
            //if (route == null) route = new Route(); //Create a new route if none retrieved
            //IEnumerable<RouteWayPoint> routeWayPoint = from rwp in db.RouteWayPoint
            //                                           where rwp.RouteId == id
            //                                           select rwp;
            //RouteView routeView = new RouteView(route); //, routeWayPoint, Json(routeWayPoint));
            RouteCompare routeCompare = new RouteCompare(routeId, id);
            return View(routeCompare);
        }

        //SearchAll - Search (for on the fly route) for non logged used
        //For now we just list all routes in the database
        [AllowAnonymous]
        public ActionResult SearchAll(decimal startLat = 0, decimal startLng = 0, decimal endLat = 0, decimal endLng = 0)
        //id=route ID, search route closed to the provided route ID
        {
            IQueryable<RouteSearch> routeList;
            if (startLat == 0 || startLng == 0 || endLat == 0 || endLng == 0)
            { //All routes
                routeList = from r in db.Route
                            select new RouteSearch { Id = r.Id, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = 0, EndDistance = 0 };
            }
            else //Search matching routes
            {
                decimal maxDist = 5; //Distance approximation allowed in kilometers

                routeList = from r in db.Route
                            where SqlFunctions.Acos(SqlFunctions.Sin((double) SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin((double)SqlFunctions.Radians(startLat)) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos((double)SqlFunctions.Radians(startLat)) * SqlFunctions.Cos((double)SqlFunctions.Radians(startLng) - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371 < (double)maxDist &&
                            SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin((double)SqlFunctions.Radians(endLat)) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos((double)SqlFunctions.Radians(endLat)) * SqlFunctions.Cos((double)SqlFunctions.Radians(endLng) - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 < (double)maxDist
                            select new RouteSearch { Id = r.Id, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin((double)SqlFunctions.Radians(startLat)) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos((double)SqlFunctions.Radians(startLat)) * SqlFunctions.Cos((double)SqlFunctions.Radians(startLng) - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371, EndDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin((double)SqlFunctions.Radians(endLat)) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos((double)SqlFunctions.Radians(endLat)) * SqlFunctions.Cos((double)SqlFunctions.Radians(endLng) - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 };
            }
            ViewBag.Title = Resources.Route_search;
            return View(routeList.ToList());
        }

        //Allow anonymous user to define a route to search existing route
        [AllowAnonymous]
        public ActionResult SetRoute()
        {
                RouteView routeView = new RouteView();
                return View(routeView);
        }

        //Not used - to be deleted
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SetRoute(RouteWayPointView[] routeView)
        {
            return View("SearchAll");
        }

        //Search - Search for logged used: Search route near another route
        public ActionResult Search(int id, decimal maxDist = 5)
        //id=route ID, search route close to the provided route ID
        {
            Route searchRoute = db.Route.Find(id);

            //decimal maxDist = 5; //Distance approximation allowed in kilometers
            double startLat = Math.PI * (double)searchRoute.StartLatitude / 180.0;
            double startLng = Math.PI * (double)searchRoute.StartLongitude / 180.0;
            double endLat = Math.PI * (double)searchRoute.EndLatitude / 180.0;
            double endLng = Math.PI * (double)searchRoute.EndLongitude / 180.0;

            var routeList = from r in db.Route
                            where SqlFunctions.Acos(SqlFunctions.Sin((double) SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371 < (double)maxDist &&
                            SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 < (double)maxDist &&
                            r.UserId != searchRoute.UserId
                            select new RouteSearch { Id = r.Id, UserId = r.UserId, IsOffer = r.IsOffer, Name = r.Name, StartDistance = SqlFunctions.Acos(SqlFunctions.Sin((double) SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Sin(startLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.StartLatitude)) * SqlFunctions.Cos(startLat) * SqlFunctions.Cos(startLng - (double)SqlFunctions.Radians(r.StartLongitude))) * 6371, EndDistance = SqlFunctions.Acos(SqlFunctions.Sin((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Sin(endLat) + SqlFunctions.Cos((double)SqlFunctions.Radians(r.EndLatitude)) * SqlFunctions.Cos(endLat) * SqlFunctions.Cos(endLng - (double)SqlFunctions.Radians(r.EndLongitude))) * 6371 };

            ViewBag.Title = Resources.Route_search;
            ViewBag.routeId = searchRoute.Id;
            ViewBag.isOffer = searchRoute.IsOffer.ToString();
            return View(routeList.ToList());
        }

        //Route list with jQuery mobile
        public ActionResult List() //int userId = 1) //Route list for mobile
        {
            if ( userId == 0 ) return RedirectToAction("Welcome", "Home");
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            ViewBag.Title = Resources.Route_list;
            return View(routeList.ToList());
        }

        //Update route: Update route non map data
        public ActionResult Update(int id = 0)
        {
            Route route = db.Route.Find(id); //Retrieve route
            RouteHeader routeHeader = new RouteHeader(route);

            return View(routeHeader);
        }

        [HttpPost]
        public int Update(int id, bool isOffer, string name)
        //Return route Id - useful for new route
        {
            Route route = null;
            if (id == 0) route = new Route(); //Create new route
            else route = db.Route.Find(id); //Retrieve route
            route.UserId = userId;
            route.Name = name;
            route.IsOffer = isOffer;
            if (id == 0) db.Route.Add(route);
            db.SaveChanges();
            return route.Id;
        }

        //Create or update route
        [AllowAnonymous]
        public ActionResult CreateUpdate(int id = 0)
        {
            Route route = db.Route.Find(id); //Retrieve route
            if (route == null) route = new Route(); //Create a new route if none retrieved
            IEnumerable<RouteWayPoint> routeWayPoint = from rwp in db.RouteWayPoint
                                                       where rwp.RouteId == id
                                                       select rwp;

            RouteView routeView = new RouteView(route); //, routeWayPoint, Json(routeWayPoint));
            return View(routeView);
        }

        [HttpPost]
        [AllowAnonymous]
        public string CreateUpdate(RouteWayPointView[] routeView)
        {
            //For now routeView cannot be null: it as a start and an end
            var routeId = routeView[0].RouteId; //routeId=0 this is a route creation
            
            //Retrieve the route
            Route route = null;
            if( routeId > 0) route = db.Route.Find(routeId);
            //if (route == null)
            //{
            //    //If we create a new route we need an active session to find user ID
            //    if (Session["userId"] == null) return "/User/Login/"; // RedirectToAction("Login", "User"); //Expired session, go to User/Login
            //    route = new Route();
            //    route.UserId = (int)Session["userId"];
            //}
            //Update the route
            //route.Name = routeView[0].
            route.StartLatitude = routeView[0].Latitude;
            route.StartLongitude = routeView[0].Longitude;
            route.EndLatitude = routeView[1].Latitude;
            route.EndLongitude = routeView[1].Longitude;
            if (routeId == 0) db.Route.Add(route); //routeId=0, add the new route
            db.SaveChanges();

            //Delete previous way points
            var linqWayPoint = from wp in db.RouteWayPoint
                           where wp.RouteId == route.Id
                           select wp;
            // wayPoint; // = new RouteWayPoint();
            foreach ( RouteWayPoint wayPoint in linqWayPoint ) {
                db.RouteWayPoint.Remove(wayPoint);
            }
            for (int i=2; i<routeView.Length; i++) {
                routeView[i].LineId -= 2;
                db.RouteWayPoint.Add(new RouteWayPoint(routeView[i]));
            }
            db.SaveChanges();
            //return RedirectToAction("ListMobile", "Route", new { userId = Session["userId"] }); //View(CreateUpdate(route.Id));
            return route.Id.ToString(); //Mobile?userId=" + Session["userId"];
        }

        //On post we use CreateUpdate


        //UpdateRoute (route header data)
        //Called after post CreateUpdate to save route IsOffer and Name
        [AllowAnonymous]
        [HttpPost]
        public string  UpdateRoute( int id, bool isOffer, string name ) {
            Route route = db.Route.Find(id);
            route.UserId = userId;
            route.IsOffer = isOffer;
            route.Name = name;
            db.SaveChanges();

            return "/Route/List";
        }

        //Return way points for the specified route id
        [AllowAnonymous]
        public JsonResult WayPoint(int id = 0)
        //timeStamp is milliseconds to force refresh
        {
            //Get the specified route
            var linqRoute = from r in db.Route
                            where r.Id == id
                            select r;

            //Add the start point and end point as first and second way point
            var routeWayPoint = (from r in linqRoute
                                 where r.StartLatitude != null && r.StartLongitude != null
                                select new RouteWayPointView { RouteId = r.Id, LineId = 1, Latitude = r.StartLatitude, Longitude = r.StartLongitude }).Union(from r in linqRoute
                          where r.EndLatitude != null && r.EndLongitude != null
                       select new RouteWayPointView { RouteId = r.Id, LineId = 2, Latitude = r.EndLatitude, Longitude = r.EndLongitude });
            //Add the remaining (real) way points
            routeWayPoint = routeWayPoint.Union(from rwp in db.RouteWayPoint
                                                where rwp.RouteId == id
                                                select new RouteWayPointView { RouteId = rwp.RouteId, LineId = (byte)(rwp.LineId + 2), Latitude = rwp.Latitude, Longitude = rwp.Longitude });
            JsonResult jsonResult = Json(routeWayPoint);
            return Json(Json(routeWayPoint), JsonRequestBehavior.AllowGet);
        }

        //Create or update route
        public ActionResult CreateUpdateOld(int id = 0)
        {
            //Simple route with 2 points
            //Route route = db.Route.Find(id);
            //if (route == null ) route = new Route();
            //return View(route);

            Route route = db.Route.Find(id); //Retrieve route
            if (route == null) route = new Route(); //Create a new route if none retrieved
            IEnumerable<RouteWayPoint> routeWayPoint = from rwp in db.RouteWayPoint
                                                       where rwp.RouteId == id
                                                       select rwp;
            JsonResult a = Json(routeWayPoint);

            //if (routeWayPoint == null) routeWayPoint = new RouteWayPoint(); //Create new way points if none retrieved

            RouteView routeView = new RouteView(route); //, routeWayPoint, Json(routeWayPoint));
            return View(routeView);
        }

        //List route for authenticated user
        public ActionResult ListOld(int userId)
        {
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            return View(routeList.ToList());
        }

        //Delete - Original autogenerated code
        public ActionResult Delete(int id = 0)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        //POST Delete - Original autogenerated code
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Route route = db.Route.Find(id);
            db.Route.Remove(route);
            db.SaveChanges();
            return RedirectToAction("List", new { userId=Session["userId"]});
        }

        //Test geolocation with HTML5
        public ActionResult Geolocation()
        {
            return View();
        }

        //Default controller generated automatically
        
        //
        // GET: /Route/

        public ActionResult Index()
        {
            return View(db.Route.ToList());
        }

        //
        // GET: /Route/Details/5

        public ActionResult Details(int id = 0)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        //
        // GET: /Route/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Route/Create

        [HttpPost]
        public ActionResult Create(Route route)
        {
            if (ModelState.IsValid)
            {
                db.Route.Add(route);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(route);
        }

        //
        // GET: /Route/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Route route = db.Route.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        //
        // POST: /Route/Edit/5

        [HttpPost]
        public ActionResult Edit(Route route)
        {
            if (ModelState.IsValid)
            {
                db.Entry(route).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(route);
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}