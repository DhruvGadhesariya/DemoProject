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
    public interface IEmployeeRepository
    {
        public string GetEmployee(long empId);

        public bool AddEmployee(Employee emp);

        public bool EditEmployee(Employee emp);

        public bool DeleteEmployee(long empId);

    }
}
