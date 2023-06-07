using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class OrderDetailsForMail
    {
        public long UserId { get; set; }

        public long ProductId { get; set; }

        public long CountryId { get; set; }

        public long CityId { get; set; }

        public string CountryName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Path { get; set; } = string.Empty ;
    }
}
