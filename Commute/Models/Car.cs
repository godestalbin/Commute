using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Commute.Models
{
    public class Car : Entity
    {
        public decimal Consumption  { get; set; }
        public decimal FuelCost { get; set; }
        public decimal Co2Emission { get; set; }
    }

    public class CostCalculation : Car
    {
        public decimal Distance { get; set; }
        public decimal Toll { get; set; }
        public decimal PersonCount { get; set; }
    }
}