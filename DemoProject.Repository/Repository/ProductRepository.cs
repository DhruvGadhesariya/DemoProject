using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using System.Drawing;
using TimeZoneNames;
using NodaTime;
using NodaTime.TimeZones;
using System.Globalization;
using System.Net;
using Newtonsoft.Json.Linq;


namespace DemoProject.Repository.Repository
{
    public class ProductRepository : IProductRepository
    {
        public readonly DemoDbContext _dbcontext;

        public ProductRepository(DemoDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public List<City> GetCityData(long countryId)
        {
            List<City> cities = _dbcontext.Cities.ToList();
            cities = cities.Where(a => a.CountryId == countryId).ToList();

            return cities;
        }
    }
}
