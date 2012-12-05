using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Commute.Models
{
    public class RouteHeader : Entity
    {
        public int UserId { get; set; }
        public bool IsOffer { get; set; } //false=offering route, true=seeking for this route
        public string Name { get; set; }

        public RouteHeader(Route route)
        {
            Id = route.Id;
            UserId = route.UserId;
            IsOffer = route.IsOffer;
            Name = route.Name;
        }
    }

    public class RouteView : Entity
        {
        public int UserId { get; set; }
        public bool IsOffer { get; set; } //false=offering route, true=seeking for this route
        public string Name { get; set; }
        //public IEnumerable<RouteWayPoint> RouteWayPoint { get; set; }
        //public JsonResult JsonRoute { get; set; }

        public RouteView()
        {
        }

        public RouteView(Route route) //, IEnumerable<RouteWayPoint> routeWayPoint, JsonResult jsonRoute)
        {
            Id = route.Id;
            UserId = route.UserId;
            IsOffer = route.IsOffer;
            Name = route.Name;
            //RouteWayPoint = routeWayPoint;
            //JsonRoute = jsonRoute;
        }
    }

    public class RouteSearch : Entity
    {
        public int UserId { get; set; }
        public bool IsOffer { get; set; } //false=offering route, true=seeking for this route
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? StartDistance { get; set; } //Distance between start point
        [DisplayFormat(DataFormatString = "{0:0.00}")]
        public double? EndDistance { get; set; } //Distance between end point
    }

    //Used to display 2 routes in Google maps
    public class RouteCompare : Entity
    {
        private Context db = new Context();
        public int RouteId1;
        public int RouteId2;
        public string User1;
        public string User2;
        public string UserMail1;
        public string UserMail2;

        public RouteCompare(int routeId1, int routeId2)
        {
        RouteId1 = routeId1;
        RouteId2 = routeId2;
        Route route1 = db.Route.Find(routeId1);
        Route route2 = db.Route.Find(routeId2);
        User user1 = db.User.Find(route1.UserId);
        User user2 = db.User.Find(route2.UserId);
        User1 = user1.Account;
        User2 = user2.Account;
        UserMail1 = user1.EmailAddress;
        UserMail2 = user2.EmailAddress;
        }
    }

}