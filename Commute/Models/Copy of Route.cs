using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Commute.Models
{
    public class CopyRoute : Entity
    {
        [DisplayFormat(DataFormatString = "{0:###}")]
        public int UserId { get; set; }
        public int RouteId { get; set; } //UserId * 1000 + Route id from Android
        public bool IsOffer { get; set; } //false=seeking for this route, true=offering route
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> StartLatitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> StartLongitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> EndLatitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> EndLongitude { get; set; }
        public Nullable<int> Distance { get; set; } //in kilometers
    }
}