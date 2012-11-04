using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Commute.Models;

namespace Commute.Controllers
{
    public class LocationController : Controller
    {
        private Context db = new Context();

        //Partial (modal) to create new location
        public ActionResult Create()
        {
            Location location = new Location();
            return PartialView(location);
        }

        [HttpPost()]
        [ActionName("Create")]
        public ActionResult CreatePost(Location loc)
        {
            if (ModelState.IsValid)
            {
                Location location = new Location();
                location.Name = loc.Name;
                location.Latitude = loc.Latitude;
                location.Longitude = loc.Longitude;
                db.Locations.Add(location);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }

        //Partial (modal) to create or update location
        public ActionResult CreateUpdate(int id = 0)
        {
            Location a = db.Locations.Find(id);
            Location location = db.Locations.Find(id);
            //Location not found, create new location
            if (location == null) location = new Location();

            return PartialView("CreateUpdate", location);
        }

        [HttpPost()]
        [ActionName("CreateUpdate")]
        public ActionResult CreateUpdatePost(Location loc)
        {
            if (ModelState.IsValid)
            {
                Location location;
                if (loc.Id == 0) location = new Location();
                else location = db.Locations.Find(loc.Id);
                location.Name = loc.Name;
                location.Latitude = loc.Latitude;
                location.Longitude = loc.Longitude;
                if (loc.Id == 0) db.Locations.Add(location);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }


        //List all location - TMP for testing
        public ActionResult List()
        {
            IEnumerable<Location> lIE = from l in db.Locations
                               select l;
            return View( lIE);
        }

        //Delete
        public ActionResult Delete(int id)
        {
            Location location = db.Locations.Find(id);
            db.Locations.Remove(location);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }



    }
}
