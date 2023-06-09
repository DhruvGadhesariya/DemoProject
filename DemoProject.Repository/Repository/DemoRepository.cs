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

        public List<City> GetCityData(long countryId, long ProductId)
        {
            var availableCityIds = _dbcontext.AvailableProducts
               .Where(a => a.ProductId == ProductId && a.CountryId== countryId)
               .Select(a => a.CityId)
               .ToList();

            var availableCities = _dbcontext.Cities
                .Where(c => availableCityIds.Contains(c.CityId))
                .ToList();

            return availableCities;
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

        public long GenerateRandomOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }

        public void CreateUserOtp(string email, long otp, DateTime createdAt, DateTime expiredAt)
        {
            var userOtp = new UserOtp()
            {
                Email = email,
                Otp = otp,
                CreatedAt = createdAt,
                ExpiredAt = expiredAt
            };

            _dbcontext.UserOtps.Add(userOtp);
            _dbcontext.SaveChanges();
        }

        public void UpdateUserOtp(UserOtp userOtp, long otp, DateTime createdAt, DateTime expiredAt)
        {
            userOtp.Otp = otp;
            userOtp.CreatedAt = createdAt;
            userOtp.ExpiredAt = expiredAt;

            _dbcontext.Update(userOtp);
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

        public string OrderProducts(OrderParams order, int? userId)
        {
            var checkShared = _dbcontext.Products.FirstOrDefault(a => a.ProductId == order.ProductId).Shared;
            string utcTime = GetCountryCityUtcTimeAsync(order.CountryId, order.CityId);
            bool isAvailable = _dbcontext.AvailableProducts.Any(a => a.CountryId == order.CountryId && a.ProductId == order.ProductId && a.CityId == order.CityId);

            TimeZoneInfo cityTimeZone = TimeZoneInfo.FindSystemTimeZoneById(utcTime);
            TimeZoneInfo indiatimezone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            TimeSpan timeDifference = cityTimeZone.BaseUtcOffset - indiatimezone.BaseUtcOffset;
            DateTime indianFromTime = GetIndianLocalTime(order.From, timeDifference);
            DateTime indianToTime = GetIndianLocalTime(order.To, timeDifference);

            if (isAvailable)
            {
                var product = _dbcontext.Products.FirstOrDefault(a => a.ProductId == order.ProductId);

                if (product.Shared == false)
                {
                    AddNewOrder(order, userId, indianFromTime, indianToTime);
                    return "true";
                }
                else
                {
                    bool hasOverlappingOrders = CheckForOverlappingOrders(order.ProductId, indianFromTime, indianToTime);

                    if (!hasOverlappingOrders)
                    {
                        AddNewOrder(order, userId, indianFromTime, indianToTime);
                        return "true";
                    }
                    else
                    {
                        return "falseTime";
                    }
                }
            }
            else
            {
                return "notAvailable";
            }
        }

        public void AddNewOrder(OrderParams order, int? userId, DateTime indianFromTime, DateTime indianToTime)
        {
            Order newOrder = new Order();
            newOrder.ProductId = order.ProductId;
            newOrder.CountryId = order.CountryId;
            newOrder.CityId = order.CityId;
            newOrder.UserId = userId;
            newOrder.FromTime = indianFromTime;
            newOrder.ToTime = indianToTime;

            _dbcontext.Orders.Add(newOrder);
            _dbcontext.SaveChanges();
        }

        public bool CheckForOverlappingOrders(long ProductId, DateTime indianFromTime, DateTime indianToTime)
        {
            bool isProductIdValid = _dbcontext.Orders.Any(order => order.ProductId == ProductId);

            if (!isProductIdValid)
            {
                return false;
            }

            bool hasOverlappingOrders = _dbcontext.Orders.Any(order =>
                order.ProductId == ProductId &&
                ((indianFromTime >= order.FromTime && indianFromTime <= order.ToTime) ||
                (indianToTime >= order.FromTime && indianToTime <= order.ToTime)));

            return hasOverlappingOrders;
        }

        public DateTime GetIndianLocalTime(DateTime utcTime, TimeSpan timeDifference)
        {
            if (timeDifference > TimeSpan.Zero)
            {
                return utcTime.Subtract(timeDifference);
            }
            else if (timeDifference < TimeSpan.Zero)
            {
                return utcTime.Add(timeDifference.Duration());
            }
            return utcTime;
        }

        public string GetCountryCityUtcTimeAsync(long countryId, long cityId)
        {
            string countryName = _dbcontext.Countries.Find(countryId).Name;
            string cityName = _dbcontext.Cities.FirstOrDefault(A => A.CityId == cityId && A.CountryId == countryId).Name;

            RegionInfo regionInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new RegionInfo(c.Name))
                .FirstOrDefault(r => r.EnglishName.Equals(countryName, StringComparison.OrdinalIgnoreCase));

            string countryCode = regionInfo?.TwoLetterISORegionName;

            TzdbDateTimeZoneSource timeZoneSource = TzdbDateTimeZoneSource.Default;

            IEnumerable<string> countryTimeZoneIds = timeZoneSource.ZoneLocations
                .Where(zone => zone.CountryCode == countryCode)
                .Select(zone => zone.ZoneId);

            IEnumerable<string> cityTimeZoneIds = timeZoneSource.ZoneLocations
                .Where(zone => zone.CountryCode == countryCode && zone.ZoneId.Contains(cityName))
                .Select(zone => zone.ZoneId);

            string timeZoneId = cityTimeZoneIds.FirstOrDefault() ?? countryTimeZoneIds.FirstOrDefault();

            return timeZoneId;
        }

        public List<Country> GetAvailableCountry(long ProductId)
        {

            var availableCountryIds = _dbcontext.AvailableProducts
                .Where(a => a.ProductId == ProductId)
                .Select(a => a.CountryId)
                .ToList();

            var availableCountries = _dbcontext.Countries
                .Where(c => availableCountryIds.Contains(c.CountryId))
                .ToList();

            return availableCountries;

        }

        public OrderDetailsForMail GetOrderDetail(OrderParams order, long userId)
        {
            var user = _dbcontext.Users.Find(userId);
            var countryName = _dbcontext.Countries.Find(order.CountryId).Name;
            var cityName = _dbcontext.Cities.Find(order.CityId).Name;
            var productName = _dbcontext.Products.Find(order.ProductId).ProductName;
          
            var model = new OrderDetailsForMail
            {
                UserId = userId,
                ProductId = order.ProductId,
                CountryId = order.CountryId,
                CityId = order.CityId,
                CountryName = countryName,
                CityName = cityName,
                FirstName = user.Fname,
                LastName = user.Lname,
                Date = order.From,
                ProductName = productName,
            };

            return model;
        }

        public OrderDetailsForMail GetOrderDetailForPreview(long orderId , string[] EmailList)
        {
            var order = _dbcontext.Orders.Find(orderId);
            var user = _dbcontext.Users.Find(order.UserId);
            var countryName = _dbcontext.Countries.Find(order.CountryId).Name;
            var cityName = _dbcontext.Cities.Find(order.CityId).Name;
            var productName = _dbcontext.Products.Find(order.ProductId).ProductName;

            var model = new OrderDetailsForMail
            {
                UserId = (long)order.UserId,
                ProductId = order.ProductId,
                CountryId = (long)order.CountryId,
                CityId = (long)order.CityId,
                CountryName = countryName,
                CityName = cityName,
                FirstName = user.Fname,
                LastName = user.Lname,
                Date = DateTime.Now,
                ProductName = productName,
                OrderId = orderId,
                
            };

            for(var i =0; i<EmailList.Length; i++)
            {
                var email = EmailList[i];
                model.Emails.Add(email);
            }

            return model;
        }
        #endregion


    }
}
