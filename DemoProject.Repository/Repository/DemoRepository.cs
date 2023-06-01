using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Repository.Interface;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using System.Drawing;
using TimeZoneNames;
using NodaTime;
using NodaTime.TimeZones;
using System.Globalization;

namespace DemoProject.Repository.Repository
{
    public class DemoRepository : IDemoRepository
    {
        #region Variables
        public readonly DemoDbContext _dbcontext;
        #endregion

        #region Constructor
        public DemoRepository(DemoDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        #endregion

        #region Validate date and email
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
        #endregion

        #region User
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
        #endregion

        #region Filters
        public List<User> FilterUsers(UserSearchParams obj)
        {
            var query = _dbcontext.Users.AsQueryable().Where(user => user.DeletedAt == null);

            query = ApplySearchFilters(query, obj);
            query = ApplySorting(query, obj);
            query = ApplyPagination(query, obj);

            return query.ToList();
        }

        public IQueryable<User> ApplySearchFilters(IQueryable<User> query, UserSearchParams obj)
        {
            if (!string.IsNullOrWhiteSpace(obj.SearchFname))
                query = query.Where(user => user.Fname.ToLower().Contains(obj.SearchFname.ToLower()));

            if (!string.IsNullOrWhiteSpace(obj.SearchLname))
                query = query.Where(user => user.Lname.ToLower().Contains(obj.SearchLname.ToLower()));

            if (!string.IsNullOrWhiteSpace(obj.SearchEmail))
                query = query.Where(user => user.Email.ToLower().Contains(obj.SearchEmail.ToLower()));

            return query;
        }

        public IQueryable<User> ApplySorting(IQueryable<User> query, UserSearchParams obj)
        {
            if (obj.Finder == "Fname")
            {
                query = (obj.Sort == "up")
                    ? query.OrderBy(user => user.Fname)
                    : query.OrderByDescending(user => user.Fname);
            }

            if (obj.Finder == "Lname")
            {
                query = (obj.Sort == "up")
                    ? query.OrderBy(user => user.Lname)
                    : query.OrderByDescending(user => user.Lname);
            }

            if (obj.Finder == "Email")
            {
                query = (obj.Sort == "up")
                    ? query.OrderBy(user => user.Email)
                    : query.OrderByDescending(user => user.Email);
            }

            return query;
        }

        public IQueryable<User> ApplyPagination(IQueryable<User> query, UserSearchParams obj)
        {
            var pageSize = obj.PageSize;
            if (obj.Pg != 0)
            {
                query = query.Skip((obj.Pg - 1) * (int)pageSize).Take((int)pageSize);
            }

            return query;
        }

        public List<User> FilterUsersForDownload(UserSearchParams obj)
        {
            var pageSize = obj.PageSize;
            var query = _dbcontext.Users.AsQueryable().Where(user => user.DeletedAt == null);

            query = ApplySearchFilters(query, obj);
            query = ApplySorting(query, obj);

            return query.ToList();
        }

        #endregion


        #region Download pdf and excel logic
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

        public void DownLoadPdf(int? userId, UserSearchParams obj)
        {
            string filename = "filtered_" + userId + "-.pdf";

            var userData = FilterUsersForDownload(obj);

            var filteredData = GetFilteredData(userData);

            string filePath = "C:\\Users\\pca140\\source\\repos\\DemoProject\\DemoProject\\wwwroot\\Downloads\\" + filename;

            Document document = new Document();
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            PdfPTable table = new PdfPTable(filteredData.Columns.Count);
            table.WidthPercentage = 100;
            table.SetWidths(GetColumnWidths(filteredData));
            table.DefaultCell.Border = PdfPCell.NO_BORDER;
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;
            iTextSharp.text.Font cellFont = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 10, BaseColor.WHITE);

            foreach (DataColumn column in filteredData.Columns)
            {
                PdfPCell cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5f;
                cell.BackgroundColor = BaseColor.GRAY;
                cell.Phrase = new Phrase(column.ColumnName, cellFont);
                table.AddCell(cell);
            }

            foreach (DataRow row in filteredData.Rows)
            {
                foreach (DataColumn column in filteredData.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(row[column].ToString()));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
            }
            document.Add(table);
            document.Close();
        }

        public void DownLoadExcel(int? userId, UserSearchParams obj)
        {
            var userData = FilterUsersForDownload(obj);

            var filteredData = GetFilteredData(userData);

            string fileName = "filtered_" + userId + "-.xlsx";

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Downloads/", fileName);

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Filtered Data");

                int columnCount = 1;
                foreach (DataColumn column in filteredData.Columns)
                {
                    worksheet.Cells[1, columnCount].Value = column.ColumnName;
                    worksheet.Cells[1, columnCount].Style.Font.Bold = true;
                    worksheet.Cells[1, columnCount].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, columnCount].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
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

                worksheet.Cells[worksheet.Dimension.Address].Style.Font.Size = 12;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                byte[] fileContents = package.GetAsByteArray();
                System.IO.File.WriteAllBytes(filePath, fileContents);
            }
        }
        #endregion

        #region OrderProducts 

        public void OrderProducts(long ProductId, long CountryId, long CityId, int? userId, DateTime From, DateTime To)
        {
            // Convert "From" time to Indian UTC time
            DateTime fromTimeUtc = TimeZoneInfo.ConvertTimeToUtc(From, TimeZoneInfo.Utc);
            DateTime indianFromTime = TimeZoneInfo.ConvertTime(fromTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            // Convert "To" time to Indian UTC time
            DateTime toTimeUtc = TimeZoneInfo.ConvertTimeToUtc(To, TimeZoneInfo.Utc);
            DateTime indianToTime = TimeZoneInfo.ConvertTime(toTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            bool hasOverlappingOrders = _dbcontext.Orders.Any(order =>
                                        order.CountryId == CountryId &&
                                        order.CityId == CityId &&
                                        ((indianFromTime >= order.FromTime && indianFromTime <= order.ToTime) ||
                                         (indianToTime >= order.FromTime && indianToTime <= order.ToTime)));

            if (!hasOverlappingOrders)
            {
                // Get the UTC time from the country and city
                DateTime utcTime = GetCountryCityUtcTimeAsync(CountryId, CityId);

                Order order = new Order();
                order.ProductId = ProductId;
                order.CountryId = CountryId;
                order.CityId = CityId;
                order.UserId = userId;
                order.UtcTime = utcTime;
                order.FromTime = indianFromTime;
                order.ToTime = indianToTime;

                _dbcontext.Orders.Add(order);
                _dbcontext.SaveChanges();
            }
            
        }

        public DateTime GetCountryCityUtcTimeAsync(long countryId, long cityId)
        {

            string countryName = _dbcontext.Countries.Find(countryId).Name;
            string cityName = _dbcontext.Cities.FirstOrDefault(A => A.CityId == cityId && A.CountryId == countryId).Name;

            string englishCountryName = new RegionInfo("en-US").EnglishName;

            RegionInfo regionInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                               .Select(c => new RegionInfo(c.Name))
                                               .FirstOrDefault(r => r.EnglishName.Equals(countryName, StringComparison.OrdinalIgnoreCase));

            IEnumerable<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones()
                                                  .Where(tz => tz.DisplayName.Contains(cityName) || tz.DisplayName.Contains(countryName));

            string countryCode = regionInfo?.TwoLetterISORegionName;

            TzdbDateTimeZoneSource timeZoneSource = TzdbDateTimeZoneSource.Default;

            // Get the time zone IDs for the specified country
            IEnumerable<string> timeZoneIds = timeZoneSource.ZoneLocations
                                                                            .Where(zone => zone.CountryCode == countryCode)
                                                                            .Select(zone => zone.ZoneId);

            if(timeZoneIds.Count() == 1)
            {
                string timeZoneId = timeZoneIds.FirstOrDefault(id => id.Contains(cityName) || id.Contains(countryName));

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                return localTime;
            }
            else
            {
                string timeZoneId = timeZoneIds.FirstOrDefault(id => id.Contains(cityName));

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                return localTime;
            }
        }
        #endregion
    }
}
