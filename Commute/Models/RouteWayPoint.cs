using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commute.Models
{
    public class RouteWayPoint //Only real way points
    {
        [Key, Column(Order = 0  )]
        public int RouteId { get; set; }
        [Key, Column(Order = 1)]
        public byte LineId { get; set; }
        [Column( TypeName = "decimal")]
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> Latitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> Longitude { get; set; }

        public RouteWayPoint() { int i = 0; }
        public RouteWayPoint(RouteWayPointView routeWayPointView)
        {
            RouteId = routeWayPointView.RouteId;
            LineId = routeWayPointView.LineId;
            Latitude = routeWayPointView.Latitude;
            Longitude = routeWayPointView.Longitude;
        }
    }

    public class RouteWayPointView
    {
        public int RouteId{get;set;}
        public byte LineId { get; set; } //1=start, 2=end, 3...=real way points
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        //public google.maps.LatLng LatLng { get; set; }
    }
}