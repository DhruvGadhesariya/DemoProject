using DemoProject.Entities.Data;
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

        public ProductsController(DemoDbContext dbcontext , IProductRepository product)
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

        
        public JsonResult GetCity(long countryId)
        {
            return Json(JsonConvert.SerializeObject(_product.GetCityData(countryId)));
        }
    }
}
