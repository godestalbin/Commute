using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Commute.Models
{
    public class Route : Entity
    {
        [DisplayFormat(DataFormatString = "{0:###}")]
        public int UserId { get; set; }
        public bool IsSeek { get; set; } //false=offering route, true=seeking for this route
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> StartLatitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> StartLongitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> EndLatitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> EndLongitude { get; set; }
    }
}