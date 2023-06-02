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

        public void DownLoadPdf(int? userId, UserSearchParams obj);

        public void DownLoadExcel(int? userId, UserSearchParams obj);

        public List<User> FilterUsersForDownload(UserSearchParams obj);

        public IQueryable<User> ApplySearchFilters(IQueryable<User> query, UserSearchParams obj);

        public IQueryable<User> ApplySorting(IQueryable<User> query, UserSearchParams obj);

        public IQueryable<User> ApplyPagination(IQueryable<User> query, UserSearchParams obj);

        public bool OrderProducts(OrderParams order, int? userId);

        public string GetCountryCityUtcTimeAsync(long countryId, long cityId);

        public DateTime GetIndianLocalTime(DateTime utcTime, TimeSpan timeDifference);

        public bool CheckForOverlappingOrders(long ProductId, DateTime indianFromTime, DateTime indianToTime);

        public long GenerateRandomOtp();

        public void CreateUserOtp(string email, long otp, DateTime createdAt, DateTime expiredAt);

        public void UpdateUserOtp(UserOtp userOtp, long otp, DateTime createdAt, DateTime expiredAt);

        public void AddNewOrder(OrderParams order, int? userId, DateTime indianFromTime, DateTime indianToTime);
    }
}
