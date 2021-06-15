using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceMS.Data
{
    public class Service
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public int TotalPrice { get; set; }
    }
}
