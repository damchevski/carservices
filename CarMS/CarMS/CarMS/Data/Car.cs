using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarMS.Data
{
    public class Car
    {
        public int Id { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int YearOfProduction { get; set; }
        public string Color { get; set; }
        public DateTime RegisteredUntil { get; set; }
    }
}
