using DemoProject.Entities.Data;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

namespace DemoProject.Controllers
{
    public class ProductsController : Controller
    {
        public readonly DemoDbContext _dbcontext;

        public readonly IProductRepository _product;

        public ProductsController(DemoDbContext dbcontext, IProductRepository product)
        {
            _dbcontext = dbcontext;
            _product = product;
        }
        public IActionResult Products()
        {
            var list = _dbcontext.Products.ToList();
            return View(list);
        }

        public IActionResult AddProduct()
        {
            ViewBag.Countries = _dbcontext.Countries.ToList();
            return View();
        }

        //public IActionResult EditProduct(long pid)
        //{

        //    var data = System.Text.Json.JsonSerializer.Serialize(_product.GetProductDataForAdmin(userId));
        //    ViewBag.Countries = _dbcontext.Countries.ToList();

        //    return View(data);
        //}

        public JsonResult GetCity(long countryId)
        {
            return Json(JsonConvert.SerializeObject(_product.GetCityData(countryId)));
        }

        [HttpPost]
        public void AddProductByAdmin(ProductDataModel obj, string CityMappings)
        {

            var cityMappings = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CityMappings);

            bool msg = _product.AddProductByAdmin(obj, cityMappings);

            if (msg)
            {
                TempData["success"] = "New Product has been added";
            }
            else
            {
                TempData["error"] = "Oops there is a error while adding the product.";
            }
        }
    }
}
