using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using System.Drawing;
using NodaTime.TimeZones;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Web.Mvc;

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

        #region Employee CRUD Operations

        // Function to Create Employee
        public bool AddEmployee(Employee emp)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("AddEmployee", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Firstname", emp.Firstname);
                        command.Parameters.AddWithValue("@Lastname", emp.Lastname);
                        command.Parameters.AddWithValue("@Email", emp.Email);
                        command.Parameters.AddWithValue("@Password", emp.Password);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        command.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return false;
            }
        }

        // Function to Read Employee    
        public string GetEmployee(long empId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("GetEmployee", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmpId", empId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var employeeData = new Employee
                        {
                            Firstname = reader["firstname"].ToString(),
                            Lastname = reader["lastname"].ToString(),
                            Email = reader["email"].ToString(),
                            EmpId = empId
                        };

                        string json = JsonConvert.SerializeObject(employeeData);

                        return json;
                    }
                }
            }

            return null; // When no data is found
        }

        // Function to Update Employee
        public bool EditEmployee(Employee emp)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UpdateEmployee", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@EmpId", emp.EmpId);
                        command.Parameters.AddWithValue("@Firstname", emp.Firstname);
                        command.Parameters.AddWithValue("@Lastname", emp.Lastname);
                        command.Parameters.AddWithValue("@Email", emp.Email);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                        command.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle exception 
                return false;
            }
        }

        // Function to Delete Employee
        public bool DeleteEmployee(long empId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("DeleteEmployee", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@EmpId", empId);

                connection.Open();

                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        #endregion
    }
}
