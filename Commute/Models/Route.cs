using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commute.Models
{
    public class Route
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RouteId { get; set; } //UserId * 1000 + Route id from Android
        [DisplayFormat(DataFormatString = "{0:###}")]
        public int UserId { get; set; }
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
        public string CovoiturageLink { get; set; } ///Link to 3rd party coivoiturage.fr, if set it all tells this is not user created
    }
}