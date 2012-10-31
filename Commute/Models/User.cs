using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Commute.Models
{
    public class User : Entity
    {
        [StringLength(5)]
        public string Name { get; set; }
        [StringLength(100)]
        public string EmailAddress { get; set; }
    }
}