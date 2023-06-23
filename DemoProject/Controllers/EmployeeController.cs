using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoProject.Controllers
{
    public class EmployeeController : Controller
    {
        #region Private Fields

        private readonly IEmployeeRepository _employee;
        private readonly DemoDbContext _dbcontext;

        #endregion

        #region Constructor

        public EmployeeController(DemoDbContext dbcontext, IEmployeeRepository employee)
        {
            _dbcontext = dbcontext;
            _employee = employee;
        }

        #endregion

        #region Actions

        public IActionResult Employee()
        {
            int pageSize = 5;

            // the total number of employees
            var list = _dbcontext.Employees.Count(a => a.DeletedAt == null);

            // the total number of pages based on the page size
            ViewBag.TotalPages1 = Math.Ceiling(list / (double)pageSize);

            // default values for viewbag properties
            ViewBag.CurrentPage = 1;
            ViewBag.Finder = "Firstname";
            ViewBag.Sort = "up";
            ViewBag.PageSize = pageSize;

            return View();
        }

        #endregion

        #region Filters Using SP

        [HttpPost]
        public ActionResult Search(UserSearchParams obj)
        {
            // null-coalescing assignment operator 
            obj.SearchFname ??= string.Empty;
            obj.SearchLname ??= string.Empty;
            obj.SearchEmail ??= string.Empty;

            // employees matching the search parameters
            List<Employee> empData = _employee.FilterEmployee(obj);

            // the total count of employees for pagination
            var list = _dbcontext.Employees.Count(a => a.DeletedAt == null);

            // viewbag properties for view
            ViewBag.TotalPages1 = Math.Ceiling(list / (double)obj.PageSize);
            ViewBag.CurrentPage = obj.Pg;
            ViewBag.Finder = obj.Finder;
            ViewBag.Sort = obj.Sort;
            ViewBag.PageSize = obj.PageSize;

            return PartialView("_EmployeeCRUD", empData);
        }

        [HttpPost]
        public ActionResult Pagination(UserSearchParams obj)
        {
            // empty strings for null search parameters
            obj.SearchFname ??= string.Empty;
            obj.SearchLname ??= string.Empty;
            obj.SearchEmail ??= string.Empty;

            // the count of all employees or filtered employees based on search parameters
            var empData = _employee.FilterWithoutPg(obj);

            // the total count of employees for pagination
            var list = _dbcontext.Employees.Count(a => a.DeletedAt == null);

            // the total number of pages based on the page size
            ViewBag.TotalPages1 = string.IsNullOrEmpty(obj.SearchEmail) && string.IsNullOrEmpty(obj.SearchLname) && string.IsNullOrEmpty(obj.SearchFname)
                ? Math.Ceiling(list / (double)obj.PageSize)
                : Math.Ceiling(empData.Count() / (double)obj.PageSize);

            // viewbag properties for view
            ViewBag.CurrentPage = obj.Pg;
            ViewBag.Finder = obj.Finder;
            ViewBag.Sort = obj.Sort;
            ViewBag.PageSize = obj.PageSize;

            return PartialView("_PaginationEmployee");
        }

        #endregion

        #region Employee CRUD Operations

        // Function to Create Employee
        public void AddEmp(Employee emp)
        {

           bool isAdded = _employee.AddEmployee(emp);

           if (isAdded)
           {
               TempData["success"] = "Employee added successfully!!";
           }
           else
           {
               TempData["error"] = "There is an error while saving entities!!";
           }    
        }

        // Function to Read Employee 
        public string GetEmp(long EmpId)
        {
            return _employee.GetEmployee(EmpId);
        }

        // Function to Update Employee
        public void EditEmp(Employee emp)
        {
            bool isUpdated = _employee.EditEmployee(emp);

            if (isUpdated)
            {
                TempData["success"] = "Employee updated successfully!!";
            }
            else
            {
                TempData["error"] = "There is an error while updating entities!!";
            }
        }

        // Function to Delete Employee
        public void DeleteEmp(long empId)
        {
            bool isDeleted = _employee.DeleteEmployee(empId);

            if (isDeleted)
            {
                TempData["success"] = "Employee deleted successfully!!";
            }
            else
            {
                TempData["error"] = "There is an error while deleting entities!!";
            }
        }

        #endregion
    }
}
