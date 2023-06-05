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

        public bool AddProductByAdmin(ProductDataModel obj , Dictionary<string, List<string>> CityMappings)
        {
            Product product = new Product();
            product.ProductName = obj.productName;
            product.Shared = obj.productShared;

            _dbcontext.Products.Add(product);
            _dbcontext.SaveChanges();

            foreach (var kvp in CityMappings)
            {
                string countryId = kvp.Key;
                long id = long.Parse(countryId);

                foreach (var cityId in kvp.Value)
                {
                    long cityid = long.Parse(cityId);

                    AvailableProduct availableProduct = new AvailableProduct();
                    availableProduct.CountryId = id;
                    availableProduct.ProductId = product.ProductId;
                    availableProduct.Available = true;
                    availableProduct.CityId = cityid;

                    _dbcontext.AvailableProducts.Add(availableProduct);
                    _dbcontext.SaveChanges();
                }
            }
            return true;
        }

        public IQueryable GetProductDataForAdmin(long productId)
        {
           
                var query = from u in _dbcontext.Products
                            join a in _dbcontext.AvailableProducts on u.ProductId equals a.ProductId
                            where u.ProductId == productId 
                            select new
                            {
                                ProductName = u.ProductName,
                                Shared = u.Shared,
                                CountryIds = a.CountryId,
                                CityIds = a.CityId,
                            };
                return query;

        }
    }
}
