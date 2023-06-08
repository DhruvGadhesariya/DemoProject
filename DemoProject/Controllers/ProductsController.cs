using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace DemoProject.Controllers
{
    public class ProductsController : Controller
    {
        #region Private Fields

        private readonly DemoDbContext _dbcontext;
        private readonly IProductRepository _product;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructor

        public ProductsController(DemoDbContext dbcontext, IProductRepository product, ICompositeViewEngine viewEngine, IHttpContextAccessor httpContextAccessor)
        {
            _dbcontext = dbcontext;
            _product = product;
            _viewEngine = viewEngine;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Actions

        public IActionResult Products()
        {
            // Get products list
            var list = _dbcontext.Products.ToList();

            // Populate ViewBag data
            ViewBag.countries = _dbcontext.Countries.ToList();
            ViewBag.Totalpages1 = Math.Ceiling(_dbcontext.Products.ToList().Count() / 3.0);
            ViewBag.currentPage = 1;
            ViewBag.Finder = "Name";
            ViewBag.Sort = "up";
            ViewBag.pagesize = 3;
            ViewBag.Products = _dbcontext.Products.ToList();

            return View(list);
        }

        public IActionResult AddProduct()
        {
            // Get countries list
            ViewBag.Countries = _dbcontext.Countries.ToList();
            return View();
        }

        public IActionResult EditProduct(long pid)
        {
            // Get countries list
            ViewBag.Countries = _dbcontext.Countries.ToList();

            // Get ViewModelData
            var viewModel = _product.GetViewModelData(pid);
            return View(viewModel);
        }

        public JsonResult GetCity(long countryId)
        {
            // Get city data for the specified country
            return Json(JsonConvert.SerializeObject(_product.GetCityData(countryId)));
        }

        [HttpPost]
        public ActionResult AddProductByAdmin(ProductDataModel obj, string CityMappings)
        {
            // Deserialize CityMappings from JSON
            var cityMappings = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CityMappings);

            // Add product by admin
            bool success = _product.AddProductByAdmin(obj, cityMappings);

            if (success)
            {
                return Json(new { success = true, message = "New product has been added." });
            }
            else
            {
                return Json(new { success = false, message = "Oops, there was an error while adding the product." });
            }
        }

        [HttpPost]
        public ActionResult EditProductByAdmin(ProductDataModel obj, string CityMappings)
        {
            // Deserialize CityMappings from JSON
            var cityMappings = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CityMappings);

            // Edit product by admin
            bool success = _product.EditProductByAdmin(obj, cityMappings);

            if (success)
            {
                return Json(new { success = true, message = "Product has been updated." });
            }
            else
            {
                return Json(new { success = false, message = "Oops, there was an error while updating the product." });
            }
        }

        [HttpPost]
        public void RemoveByAdmin(long Id)
        {
            // Remove product by admin
            _product.RemoveByAdmin(Id);
            TempData["success"] = "You have Removed successfully!!";
        }

        public IActionResult Orders()
        {
            // Get order list
            var orderList = _dbcontext.Orders.ToList();

            // Get order details
            var newList = _product.GetOrderDetails(orderList);
            return View(newList);
        }

        #endregion
    }
}
