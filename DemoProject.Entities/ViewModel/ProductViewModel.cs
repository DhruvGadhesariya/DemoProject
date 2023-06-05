using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class ProductViewModel
    {
        public long ProductId { get; set; }

        public string? ProductName { get; set; }

        public bool? Shared { get; set; }

        public long CountryId { get; set; }

        public string CountryName { get; set; } = null!;

        public long CityId { get; set; }

        public string CityName { get; set; } = null!;

        public bool? Available { get; set; }
    }
}
