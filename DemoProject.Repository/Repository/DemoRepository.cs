using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;

namespace DemoProject.Repository.Repository
{
    public class DemoRepository : IDemoRepository
    {
        public readonly DemoDbContext _dbcontext;

        public DemoRepository(DemoDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public bool IsWithinFiveMinutes(DateTime dateTimeToCheck)
        {
            DateTime currentDateTime = DateTime.Now;
            TimeSpan difference = currentDateTime - dateTimeToCheck;

            return difference.TotalMinutes <= 5;
        }

        public bool IsValidEmailAddress(string email)
        {
            try
            {
                var emailChecked = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddUser(RegistrationModel model)
        {
            var check = _dbcontext.Users.FirstOrDefault(a => a.Email.ToLower() == model.Email.ToLower());
            string secpass = BCrypt.Net.BCrypt.HashPassword(model.Password);
            if (check == null)
            {
                var user = new User();  
                user.Email = model.Email;
                user.Password = secpass;
                user.Fname = model.FirstName;
                user.Lname = model.LastName;
                user.Phonenumber = model.PhoneNumber;
                user.CountryId = model.CountryId;
                user.CityId = model.CityId;

                _dbcontext.Users.Add(user);
                _dbcontext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<City> GetCityData(long countryId)
        {
            List<City> cities = _dbcontext.Cities.Where(a => a.DeletedAt == null).ToList();
            cities = cities.Where(a => a.CountryId == countryId && a.DeletedAt == null).ToList();

            return cities;
        }

        public void addUserByMe(User user)
        {

            string secpass = BCrypt.Net.BCrypt.HashPassword(user.Password);

            User userdata = new User();
            userdata.Fname = user.Fname;
            userdata.Lname = user.Lname;
            userdata.Email = user.Email;
            userdata.Password = secpass;
            userdata.CountryId = user.CountryId;
            userdata.CityId = user.CityId;

            _dbcontext.Users.Add(userdata);
            _dbcontext.SaveChanges();

        }
        public IQueryable GetUserDataForAdmin(long userId)
        {
            var query = from u in _dbcontext.Users
                        join c in _dbcontext.Cities on u.CityId equals c.CityId
                        where u.UserId == userId && u.DeletedAt == null
                        select new
                        {
                            FirstName = u.Fname,
                            LastName = u.Lname,
                            Email = u.Email,
                            Password = u.Password,
                            CityId = u.CityId,
                            CountryId = u.CountryId,
                            UserId = u.UserId,
                            CityName = c.Name
                        };

            return query;
        }
        public void editUserByAdmin(User user)
        {
            var check = _dbcontext.Users.Where(a => a.DeletedAt == null).FirstOrDefault(a => a.UserId == user.UserId);

            check.UserId = user.UserId;
            check.Fname = user.Fname;
            check.Lname = user.Lname;
            check.Email = user.Email;
            check.CountryId = user.CountryId;
            check.CityId = user.CityId;

            _dbcontext.Users.Update(check);

            _dbcontext.SaveChanges();
        }

        public void RemoveByAdmin(long Id)
        {
            var remove = _dbcontext.Users.Where(a => a.DeletedAt == null).FirstOrDefault(user => user.UserId == Id);
            remove.DeletedAt = DateTime.Now;
            _dbcontext.Users.Update(remove);

            _dbcontext.SaveChanges();
        }

        public List<User> FilterUsers(UserSearchParams obj)
        {
            var pageSize = 2;
            var query = _dbcontext.Users.Where(user => user.DeletedAt == null).AsQueryable();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                query = query.Where(m =>
                    m.Fname.Contains(obj.Search) ||
                    m.Lname.Contains(obj.Search) ||
                    m.Email.Contains(obj.Search)
                );
            }

            //if (!string.IsNullOrWhiteSpace(obj.SearchFname) && obj.Finder == "Fname")
            //    query = query.Where(user => user.Fname.ToLower().Contains(obj.SearchFname.ToLower()));

            if (obj.Finder == "Fname" && obj.Sort == "up")
                query = query.OrderBy(user => user.Fname);

            if(obj.Finder == "Fname" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Fname);

            //if (!string.IsNullOrWhiteSpace(obj.SearchLname) && obj.Finder == "Lname")
            //    query = query.Where(user => user.Lname.ToLower().Contains(obj.SearchLname.ToLower()));

            if (obj.Finder == "Lname" && obj.Sort == "up")
                query = query.OrderBy(user => user.Lname);

            if (obj.Finder == "Lname" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Lname);

            //if (!string.IsNullOrWhiteSpace(obj.SearchEmail) && obj.Finder == "Lname")
            //    query = query.Where(user => user.Email.ToLower().Contains(obj.SearchEmail.ToLower()));

            if (obj.Finder == "Email" && obj.Sort == "up")
                query = query.OrderBy(user => user.Email);

            if (obj.Finder == "Email" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Email);

            if (obj.Pg != 0)
                query = query.Skip((obj.Pg - 1) * pageSize).Take(pageSize);

            return query.ToList();

        }
    }
}
