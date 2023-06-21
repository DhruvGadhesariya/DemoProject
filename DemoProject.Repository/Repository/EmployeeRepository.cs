using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;



namespace DemoProject.Repository.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        #region Variables
        private readonly DemoDbContext _dbcontext;
        private const string connectionString = "Data Source=PCA140\\SQL2017;DataBase=DemoDB;TrustServerCertificate=True;User ID=sa;Password=Tatva@123";
        #endregion

        #region Constructor
        public EmployeeRepository(DemoDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        #endregion

        #region Filter Employee Using Sp 
         
        // FiterEmployees Using sp and ef core features 
        public List<Employee> FilterEmployee(UserSearchParams obj)
        {
            var employees = new List<Employee>();
            var searchFnameParam = new SqlParameter("@SearchFname", obj.SearchFname);
            var searchLnameParam = new SqlParameter("@SearchLname", obj.SearchLname);
            var searchEmailParam = new SqlParameter("@SearchEmail", obj.SearchEmail);
            var pgParam = new SqlParameter("@Pg", obj.Pg);
            var finderParam = new SqlParameter("@Finder", obj.Finder);
            var sortParam = new SqlParameter("@Sort", obj.Sort);
            var pageSizeParam = new SqlParameter("@Pagesize", obj.PageSize);

            var results = _dbcontext.EmployeeRecords.FromSql($"FilterEmployees {searchFnameParam}, {searchLnameParam}, {searchEmailParam}, {pgParam}, {finderParam}, {sortParam}, {pageSizeParam}").ToList();

            foreach (var result in results)
            {
                var employee = new Employee
                {
                    Firstname = result.firstname,
                    Lastname = result.lastname,
                    Email = result.email,
                    EmpId = result.emp_id
                };

                employees.Add(employee);
            }
            return employees;
        }

        public List<Employee> FilterWithoutPg(UserSearchParams obj)
        {
            var employees = new List<Employee>();

            var searchFnameParam = new SqlParameter("@SearchFname", obj.SearchFname);
            var searchLnameParam = new SqlParameter("@SearchLname", obj.SearchLname);
            var searchEmailParam = new SqlParameter("@SearchEmail", obj.SearchEmail);
            var finderParam = new SqlParameter("@Finder", obj.Finder);
            var sortParam = new SqlParameter("@Sort", obj.Sort);

            var results = _dbcontext.EmployeeRecords.FromSql($"FilterWithoutPagination {searchFnameParam}, {searchLnameParam}, {searchEmailParam}, {finderParam}, {sortParam}").ToList();

            foreach (var result in results)
            {
                var employee = new Employee
                {
                    Firstname = result.firstname,
                    Lastname = result.lastname,
                    Email = result.email,
                    EmpId = result.emp_id
                };

                employees.Add(employee);
            }
            return employees;
        }
        #endregion

        #region Employee CRUD Operations

        // Function to Create Employee
        public bool AddEmployee(Employee emp)
        {
            var fname = new SqlParameter("@Firstname", emp.Firstname);
            var lname = new SqlParameter("@Lastname", emp.Lastname);
            var email = new SqlParameter("@Email", emp.Email);
            var password = new SqlParameter("@Password", emp.Password);
            var createdat = new SqlParameter("@CreateAt", DateTime.Now);

           var check =  _dbcontext.Database.ExecuteSqlRaw("EXEC AddEmployee @Firstname, @Lastname, @Email, @Password, @CreateAt",
                                                          fname, lname, email, password, createdat);

            return check > 0;
        }

        // Function to Read Employee     
        public string GetEmployee(long empId)
        {
            var id = new SqlParameter("@EmpId", empId);
            var result = _dbcontext.Employees
                        .FromSql($"EXEC GetEmployee {id}")
                        .AsEnumerable()
                        .FirstOrDefault();

            if (result != null)
            {
                string json = JsonConvert.SerializeObject(result);
                return json;
            }

            return null;
        }

        // Function to Update Employee
        public bool EditEmployee(Employee emp)
        {
            var id = new SqlParameter("@EmpId", emp.EmpId);
            var fname = new SqlParameter("@Firstname", emp.Firstname);
            var lname = new SqlParameter("@Lastname", emp.Lastname);
            var email = new SqlParameter("@Email", emp.Email);
            var updatedAt = new SqlParameter("@UpdatedAt", DateTime.Now);

            var result = _dbcontext.Database.ExecuteSqlRaw("EXEC UpdateEmployee @EmpId, @Firstname, @Lastname, @Email, @UpdatedAt",
                                                           id, fname, lname, email, updatedAt);

            return result > 0;
        }

        // Function to Delete Employee
        public bool DeleteEmployee(long empId)
        {
            var id = new SqlParameter("@EmpId", empId);
            var result = _dbcontext.Database.ExecuteSqlRaw("EXEC DeleteEmployee @EmpId", id);

            return result > 0;
        }

        #endregion
    }
}
