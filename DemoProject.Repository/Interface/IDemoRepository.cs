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
    public interface IDemoRepository
    {
        public bool IsWithinFiveMinutes(DateTime dateTimeToCheck);

        public bool IsValidEmailAddress(string email);

        public bool AddUser(UserVerifyViewmodel model);

        public List<City> GetCityData(long countryId);

        public void addUserByMe(User user);

        public IQueryable GetUserDataForAdmin(long userId);

        public void editUserByAdmin(User user);

        public void RemoveByAdmin(long Id);

        public List<User> FilterUsers(UserSearchParams obj);

        public DataTable GetFilteredData(List<User> filteredUsers);

        public int[] GetColumnWidths(DataTable table);

        public void DownLoadPdf(int? userId, List<User> usersData);

        public void DownLoadExcel(int? userId, List<User> usersData);
    }
}
