using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
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
            ViewBag.Countries = _dbcontext.Countries.ToList();
            return View();
        }
        public IActionResult EditProduct(long pid)
        {
            ViewBag.Countries = _dbcontext.Countries.ToList();
            var viewModel = _product.GetViewModelData(pid);
            return View(viewModel);
        }

        public JsonResult GetCity(long countryId)
        {
            return Json(JsonConvert.SerializeObject(_product.GetCityData(countryId)));
        }

        [HttpPost]
        public ActionResult AddProductByAdmin(ProductDataModel obj, string CityMappings)
        {
            var cityMappings = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CityMappings);
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
            var cityMappings = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CityMappings);
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
            _product.RemoveByAdmin(Id);
            TempData["success"] = "You have Removed successfully!!";
        }

        [HttpPost]
        public ActionResult SearchProducts(ProductSearchParams obj)
        {
            obj.Name = string.IsNullOrEmpty(obj.Name) ? "" : obj.Name;

            List<Product> usersData = _product.FilterProducts(obj);
            ViewBag.usersList = usersData;

            var list = _dbcontext.Products.Count();
            ViewBag.Totalpages1 = (int)Math.Ceiling((double)list / obj.PageSize);
            ViewBag.currentPage = obj.Pg;
            ViewBag.Finder = obj.Finder;
            ViewBag.Sort = obj.Sort;
            ViewBag.pagesize = obj.PageSize;
            ViewBag.countries = _dbcontext.Countries.ToList();
            return View("Products", usersData);
        }

        [HttpPost]
        public ActionResult Pagination(ProductSearchParams obj)
        {
            obj.Name = string.IsNullOrEmpty(obj.Name) ? "" : obj.Name;

            var usersData = _product.FilterProductsWithOutPagination(obj).Count();
            var list = _dbcontext.Products.Count();
            if (string.IsNullOrEmpty(obj.Name))
            {
                ViewBag.Totalpages1 = (int)Math.Ceiling((double)list / obj.PageSize);
            }
            else
            {
                ViewBag.Totalpages1 = (int)Math.Ceiling((double)usersData / obj.PageSize);
            }
            return PartialView("_PaginationProduct" , obj);
        }
    }
}
