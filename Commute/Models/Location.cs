using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Commute.Models
{
    public class Location : Entity
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Location_name", ResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> Latitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:###.##############}")]
        public Nullable<decimal> Longitude { get; set; } 
    }
}