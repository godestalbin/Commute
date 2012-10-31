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
                //return Json(new { Success = true });
            }
            return RedirectToAction("List");
        }

        //Partial (modal) to update existing location
        public ActionResult Update(int locationId = 0)
        {
            Location a = db.Locations.Find(1);
            Location location = db.Locations.Find(locationId);

            var linq = from l in db.Locations
                       orderby l.Name
                       select new Location { Id = l.Id, Name = l.Name, Latitude = l.Latitude, Longitude = l.Longitude };
            return PartialView("Create", location);
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
