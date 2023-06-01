using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Entities.ViewModel
{
    public class TimeZoneApiResponse
    {
        public string ZoneName { get; set; }
        public string CountryName { get; set; }
        public string Abbreviation { get; set; }
        public int GmtOffset { get; set; }
        public string TimeFormat { get; set; }
        public string TimeZoneName { get; set; }
    }

}
