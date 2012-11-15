using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Commute.Models;
using Commute.Properties;

namespace Commute.Controllers
{
    public class RouteController : BaseController
    {
        private Context db = new Context();

        //Search
        [AllowAnonymous]
        public ActionResult Search(int id) //First attempt to use jQuery mobile not completed
            //id=route ID, search route closed to the provided route ID
        {
            Route searchRoute = db.Route.Find(id);
            decimal maxDist = 0.5M;
            var routeList = from r in db.Route
                            where Math.Abs((decimal)(r.StartLatitude - searchRoute.StartLatitude)) + Math.Abs((decimal)(r.StartLongitude - searchRoute.StartLongitude)) + Math.Abs((decimal)(r.EndLatitude - searchRoute.EndLatitude)) + Math.Abs((decimal)(r.EndLongitude - searchRoute.EndLongitude)) < maxDist
                            && r.UserId != searchRoute.UserId
                            select r; // new { r.Id, dist = Math.Abs((decimal)(r.StartLatitude - searchRoute.StartLatitude)) + Math.Abs((decimal)(r.StartLongitude - searchRoute.StartLongitude)) + Math.Abs((decimal)(r.EndLatitude - searchRoute.EndLatitude)) + Math.Abs((decimal)(r.EndLongitude - searchRoute.EndLongitude)) };
            ViewBag.Title = Resources.Route_search;
            return View(routeList.ToList());
        }

        //Route list with jQuery mobile
        [AllowAnonymous]
        public ActionResult ListMobile(int userId = 1) //Route list for mobile
        {
            var routeList = from r in db.Route
                            where r.UserId == userId
                            select r;
            ViewBag.Title = Resources.Route_list;
            return View(routeList.ToList());
        }

        //Create or update route
        public ActionResult CreateUpdate(int id = 0)
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

            RouteView routeView = new RouteView(route, routeWayPoint, Json(routeWayPoint));
            return View(routeView);
        }

        [HttpPost]
        public string CreateUpdate(RouteWayPointView[] routeView)
        {
            //For now routeView cannot be null: it as a start and an end
            var routeId = routeView[0].RouteId; //routeId=0 this is a route creation
            
            //Retrieve the route
            Route route = null;
            if( routeId > 0) route = db.Route.Find(routeId);
            if (route == null)
            {
                //If we create a new route we need an active session to find user ID
                if (Session["userId"] == null) return "/User/Login/"; // RedirectToAction("Login", "User"); //Expired session, go to User/Login
                route = new Route();
                route.UserId = (int)Session["userId"];
            }
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
            return "/Route/ListMobile?userId=" + Session["userId"];
        }

        //CreateUpdateMobile
        [AllowAnonymous]
        public ActionResult CreateUpdateMobile(int id = 0)
        {
            Route route = db.Route.Find(id); //Retrieve route
            if (route == null) route = new Route(); //Create a new route if none retrieved
            IEnumerable<RouteWayPoint> routeWayPoint = from rwp in db.RouteWayPoint
                                                       where rwp.RouteId == id
                                                       select rwp;

            RouteView routeView = new RouteView(route, routeWayPoint, Json(routeWayPoint));
            return View(routeView);
        }

        //On post we use CreateUpdate

        //Return way points for the specified route id
        [AllowAnonymous]
        public JsonResult WayPoint(int id = 0)
        {
            //Get the specified route
            var linqRoute = from r in db.Route
                            where r.Id == id
                            select r;

            //Add the start point and end point as first and second way point
            var routeWayPoint = (from r in linqRoute
                                select new RouteWayPointView { RouteId = r.Id, LineId = 1, Latitude = r.StartLatitude, Longitude = r.StartLongitude }).Union(from r in linqRoute
                                select new RouteWayPointView { RouteId = r.Id, LineId = 2, Latitude = r.EndLatitude, Longitude = r.EndLongitude });
            //Add the remaining (real) way points
            routeWayPoint = routeWayPoint.Union(from rwp in db.RouteWayPoint
                                                where rwp.RouteId == id
                                                select new RouteWayPointView { RouteId = rwp.RouteId, LineId = (byte)(rwp.LineId + 2), Latitude = rwp.Latitude, Longitude = rwp.Longitude });
            JsonResult jsonResult = Json(routeWayPoint);
            return Json(Json(routeWayPoint), JsonRequestBehavior.AllowGet);
        }

        //List route for authenticated user
        public ActionResult List(int userId)
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