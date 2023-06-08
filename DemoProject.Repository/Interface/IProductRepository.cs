using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Repository.Interface
{
    public interface IProductRepository
    {
        public List<City> GetCityData(long countryId);

        public bool AddProductByAdmin(ProductDataModel obj , Dictionary<string, List<string>> CityMappings);

        public bool EditProductByAdmin(ProductDataModel obj, Dictionary<string, List<string>> CityMappings);

        public void AddOrUpdateAvailableProducts(long productId, Dictionary<string, List<string>> CityMappings);

        public ProductViewModel GetViewModelData(long pid);

        public void RemoveByAdmin(long Id);

        public List<Product> FilterProducts(ProductSearchParams obj);

        public IQueryable<Product> ApplySearchFilters(IQueryable<Product> query, ProductSearchParams obj);

        public IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductSearchParams obj);

        public IQueryable<Product> ApplyPagination(IQueryable<Product> query, ProductSearchParams obj);

        public List<Product> FilterProductsWithOutPagination(ProductSearchParams obj);

        public List<OrderDetailsForMail> GetOrderDetails(List<Order> orders);

        public OrderDetailsForMail GetOrderData(long OrderId);

        public bool AreValidEmailAddresses(string[] emailList);


    }
}
