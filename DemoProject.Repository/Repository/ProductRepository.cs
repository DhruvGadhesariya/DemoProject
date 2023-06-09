using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace DemoProject.Repository.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly DemoDbContext _dbContext;

        public ProductRepository(DemoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<City> GetCityData(long countryId)
        {
            return _dbContext.Cities
                .Where(city => city.CountryId == countryId)
                .ToList();
        }

        public bool AddProductByAdmin(ProductDataModel obj, Dictionary<string, List<string>> CityMappings)
        {
            var product = new Product
            {
                ProductName = obj.productName,
                Shared = obj.productShared
            };

            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            AddOrUpdateAvailableProducts(product.ProductId, CityMappings);

            return true;
        }

        public bool EditProductByAdmin(ProductDataModel obj, Dictionary<string, List<string>> CityMappings)
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == obj.productId);
            if (product == null)
                return false;

            product.ProductName = obj.productName;
            product.Shared = obj.productShared;

            _dbContext.SaveChanges();

            AddOrUpdateAvailableProducts(product.ProductId, CityMappings);

            return true;
        }

        public void AddOrUpdateAvailableProducts(long productId, Dictionary<string, List<string>> CityMappings)
        {
            var existingMappings = _dbContext.AvailableProducts
                .Where(ap => ap.ProductId == productId)
                .ToList();

            foreach (var kvp in CityMappings)
            {
                if (!long.TryParse(kvp.Key, out long countryId))
                    continue;

                foreach (var cityId in kvp.Value)
                {
                    if (!long.TryParse(cityId, out long parsedCityId))
                        continue;

                    bool existingMapping = existingMappings
                        .Any(ap => ap.CountryId == countryId && ap.CityId == parsedCityId && ap.ProductId == productId);

                    if (!existingMapping)
                    {
                        var availableProduct = new AvailableProduct
                        {
                            CountryId = countryId,
                            ProductId = productId,
                            Available = true,
                            CityId = parsedCityId
                        };

                        _dbContext.AvailableProducts.Add(availableProduct);
                    }
                }
            }

            var uncheckedMappings = existingMappings
                .Where(ap => !CityMappings.ContainsKey(ap.CountryId.ToString()) || !CityMappings[ap.CountryId.ToString()].Contains(ap.CityId.ToString()))
                .ToList();

            _dbContext.AvailableProducts.RemoveRange(uncheckedMappings);

            _dbContext.SaveChanges();
        }

        public ProductViewModel GetViewModelData(long productId)
        {
            var availableProducts = _dbContext.AvailableProducts
                .Where(ap => ap.ProductId == productId)?
                .GroupBy(ap => ap.CountryId)?
                .ToDictionary(g => g.Key, g => g.Select(ap => ap.CityId).ToList());

            var model = new ProductViewModel
            {
                ProductId = productId,
                ProductName = _dbContext.Products.Find(productId)?.ProductName,
                Shared = _dbContext.Products.Find(productId)?.Shared,
                CountryCityMap = new Dictionary<Country, List<City>>()
            };

            var allCountries = _dbContext.Countries.ToList();
            var allCities = _dbContext.Cities.ToList();

            foreach (var country in allCountries)
            {
                var cities = allCities
                    .Where(city => availableProducts.ContainsKey(country.CountryId) && availableProducts[country.CountryId].Contains(city.CityId))
                    .ToList();

                model.CountryCityMap.Add(country, cities);
            }

            return model;
        }

        public void RemoveByAdmin(long productId)
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                return;

            _dbContext.Products.Remove(product);

            var availableProducts = _dbContext.AvailableProducts.Where(ap => ap.ProductId == productId).ToList();

            _dbContext.AvailableProducts.RemoveRange(availableProducts);
            _dbContext.SaveChanges();
        }

        public List<Product> FilterProducts(ProductSearchParams obj)
        {
            var query = _dbContext.Products.AsQueryable();

            query = ApplySearchFilters(query, obj);
            query = ApplySorting(query, obj);
            query = ApplyPagination(query, obj);

            return query.ToList();
        }

        public List<Product> FilterProductsWithOutPagination(ProductSearchParams obj)
        {
            var query = _dbContext.Products.AsQueryable();

            query = ApplySearchFilters(query, obj);
            query = ApplySorting(query, obj);

            return query.ToList();
        }

        public IQueryable<Product> ApplySearchFilters(IQueryable<Product> query, ProductSearchParams obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.Name))
                query = query.Where(product => product.ProductName.ToLower().Contains(obj.Name.ToLower()));

            return query;
        }

        public IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductSearchParams obj)
        {
            if (obj.Finder == "Name")
            {
                query = (obj.Sort == "up")
                    ? query.OrderBy(product => product.ProductName)
                    : query.OrderByDescending(product => product.ProductName);
            }
            return query;
        }

        public IQueryable<Product> ApplyPagination(IQueryable<Product> query, ProductSearchParams obj)
        {
            var pageSize = obj.PageSize;
            if (obj.Pg != 0)
            {
                query = query.Skip((obj.Pg - 1) * (int)pageSize).Take((int)pageSize);
            }

            return query;
        }

        public List<OrderDetailsForMail> GetOrderDetails(List<Order> orders)
        {
            
            var orderDetails = new List<OrderDetailsForMail>();

            foreach (var order in orders)
            {
                var user = _dbContext.Users.Find(order.UserId);
                var countryName = _dbContext.Countries.Find(order.CountryId)?.Name;
                var cityName = _dbContext.Cities.Find(order.CityId)?.Name;

                var orderDetail = new OrderDetailsForMail
                {
                    OrderId = order.OrderId,
                    ProductId = order.ProductId,
                    UserName = user.Fname + " " + user.Lname,
                    From = order.FromTime ?? DateTime.MinValue,
                    To = order.ToTime ?? DateTime.MinValue,
                };

                if (order.City != null)
                {
                    orderDetail.CountryName = countryName;
                    orderDetail.CityName = cityName;
                }

                orderDetails.Add(orderDetail);
            }

            return orderDetails;
        }

        public OrderDetailsForMail GetOrderData(long OrderId)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.OrderId == OrderId);
            var user = _dbContext.Users.Find(order.UserId);
            var countryName = _dbContext.Countries.Find(order.CountryId)?.Name;
            var cityName = _dbContext.Cities.Find(order.CityId)?.Name;

            var orderDetail = new OrderDetailsForMail
            {
                OrderId = OrderId,
                ProductId = order.ProductId,
                UserName = user?.Fname + " " + user?.Lname,
                From = order.FromTime ?? DateTime.MinValue,
                To = order.ToTime ?? DateTime.MinValue
            };

            if (order.City != null)
            {
                orderDetail.CountryName = countryName;
                orderDetail.CityName = cityName;
            }

            return orderDetail;
        }

        public bool AreValidEmailAddresses(string[] emailList)
        {
            foreach (var email in emailList)
            {
                try
                {
                    var emailChecked = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        
    }
}
