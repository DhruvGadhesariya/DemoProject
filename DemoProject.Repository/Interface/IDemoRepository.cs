using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoProject.Repository.Interface
{
    public interface IDemoRepository
    {
        public bool IsWithinFiveMinutes(DateTime dateTimeToCheck);

        public bool IsValidEmailAddress(string email);

        public bool AddUser(RegistrationModel model);

        public List<City> GetCityData(long countryId);

        public void addUserByMe(User user);

        public IQueryable GetUserDataForAdmin(long userId);

        public void editUserByAdmin(User user);

        public void RemoveByAdmin(long Id);

        public List<User> FilterUsers(UserSearchParams obj);
    }
}
