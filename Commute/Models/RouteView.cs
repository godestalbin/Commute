using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Commute.Models
{
    public class RouteView : Entity
    {
        public int UserId { get; set; }
        public bool IsOffer { get; set; } //false=offering route, true=seeking for this route
        public string Name { get; set; }
        public IEnumerable<RouteWayPoint> RouteWayPoint { get; set; }
        public JsonResult JsonRoute { get; set; }

        public RouteView(Route route, IEnumerable<RouteWayPoint> routeWayPoint, JsonResult jsonRoute)
        {
            Id = route.Id;
            UserId = route.UserId;
            IsOffer = route.IsOffer;
            Name = route.Name;
            RouteWayPoint = routeWayPoint;
            JsonRoute = jsonRoute;
        }
    }
}