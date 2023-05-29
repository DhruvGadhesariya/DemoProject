using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using System.Data;
using Azure;

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

        public bool AddUser(UserVerifyViewmodel model)
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
            var pageSize = obj.PageSize;
            var query = _dbcontext.Users.AsQueryable().Where(user => user.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(obj.SearchFname))
                query = query.Where(user => user.Fname.ToLower().Contains(obj.SearchFname.ToLower()));

            if (!string.IsNullOrWhiteSpace(obj.SearchLname))
                query = query.Where(user => user.Lname.ToLower().Contains(obj.SearchLname.ToLower()));

            if (!string.IsNullOrWhiteSpace(obj.SearchEmail))
                query = query.Where(user => user.Email.ToLower().Contains(obj.SearchEmail.ToLower()));

            if (obj.Finder == "Fname" && obj.Sort == "up")
                query = query.OrderBy(user => user.Fname);

            if (obj.Finder == "Fname" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Fname);

            if (obj.Finder == "Lname" && obj.Sort == "up")
                query = query.OrderBy(user => user.Lname);

            if (obj.Finder == "Lname" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Lname);

            if (obj.Finder == "Email" && obj.Sort == "up")
                query = query.OrderBy(user => user.Email);

            if (obj.Finder == "Email" && obj.Sort == "down")
                query = query.OrderByDescending(user => user.Email);

            if (obj.Pg != 0)
                query = query.Skip((obj.Pg - 1) * (int)pageSize).Take((int)pageSize);

            return query.ToList();

        }
        public DataTable GetFilteredData(List<User> filteredUsers)
        {
            DataTable table = new DataTable();

            table.Columns.Add("First Name", typeof(string));
            table.Columns.Add("Last Name", typeof(string));
            table.Columns.Add("Email", typeof(string));

            foreach (var user in filteredUsers)
            {
                DataRow row = table.NewRow();
                row["First Name"] = user.Fname;
                row["Last Name"] = user.Lname;
                row["Email"] = user.Email;
                table.Rows.Add(row);
            }
            return table;
        }

        public int[] GetColumnWidths(DataTable table)
        {
            int[] columnWidths = new int[table.Columns.Count];

            for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                int maxLength = table.Columns[columnIndex].ColumnName.Length;

                foreach (DataRow row in table.Rows)
                {
                    if (!row.IsNull(columnIndex))
                    {
                        int cellLength = row[columnIndex].ToString().Length;
                        maxLength = Math.Max(maxLength, cellLength);
                    }
                }
                columnWidths[columnIndex] = (maxLength + 2) * 7;
            }
            return columnWidths;
        }

        public void DownLoadPdf(int? userId, List<User> usersData)
        {
            string filename = "filtered_" + userId + "-.pdf";

            var filteredData = GetFilteredData(usersData);
            string filePath = "C:\\Users\\pca140\\source\\repos\\DemoProject\\DemoProject\\wwwroot\\Downloads\\" + filename;

            Document document = new Document();
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            PdfPTable table = new PdfPTable(filteredData.Columns.Count);
            table.WidthPercentage = 100;
            table.SetWidths(GetColumnWidths(filteredData));

            foreach (DataColumn column in filteredData.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                table.AddCell(cell);
            }

            foreach (DataRow row in filteredData.Rows)
            {
                foreach (DataColumn column in filteredData.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(row[column].ToString()));
                    table.AddCell(cell);
                }
            }
            document.Add(table);
            document.Close();
        }

        public void DownLoadExcel(int? userId, List<User> usersData)
        {
            var filteredData = GetFilteredData(usersData);

            string fileName = "filtered_" + userId + "-.xlsx";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Downloads/", fileName);

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Filtered Data");

                int columnCount = 1;
                foreach (DataColumn column in filteredData.Columns)
                {
                    worksheet.Cells[1, columnCount].Value = column.ColumnName;
                    columnCount++;
                }

                int rowCount = 2;
                foreach (DataRow row in filteredData.Rows)
                {
                    int colCount = 1;
                    foreach (DataColumn column in filteredData.Columns)
                    {
                        worksheet.Cells[rowCount, colCount].Value = row[column];
                        colCount++;
                    }
                    rowCount++;
                }
                byte[] fileContents = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(filePath, fileContents);
            }
        }
    }
}
