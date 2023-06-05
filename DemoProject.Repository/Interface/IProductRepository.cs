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

        public IQueryable GetProductDataForAdmin(long productId);

    }
}
