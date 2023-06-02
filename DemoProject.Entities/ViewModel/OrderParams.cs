using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class OrderParams
    {
        public long ProductId { get; set; }
        public long CountryId { get; set; }
        public long CityId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
