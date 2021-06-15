using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceMS.Data
{
    public class ServiceDto
    {
        public int UserId { get; set; }
        public int CarId { get; set; }
        public List<int> ServiceItemsIds { get; set; }

    }
}
