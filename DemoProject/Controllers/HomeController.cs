﻿using DemoProject.Entities.Data;
using DemoProject.Entities.Models;
using DemoProject.Entities.ViewModel;
using DemoProject.Models;
using DemoProject.Repository;
using DemoProject.Repository.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;

namespace DemoProject.Controllers
{

    public class HomeController : Controller
    {
        #region variables

        private readonly ILogger<HomeController> _logger;

        public readonly DemoDbContext _dbcontext;

        public readonly IDemoRepository _demoreppo;

        #endregion

        #region constructor , dependency inject
        public HomeController(ILogger<HomeController> logger, DemoDbContext dbcontext, IDemoRepository demoRepository)
        {
            _logger = logger;
            _dbcontext = dbcontext;
            _demoreppo = demoRepository;
        }

        #endregion

        #region Logout
        [Route("/logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        #endregion

        #region Registration

        public IActionResult Registration()
        {
            ViewBag.countries = _dbcontext.Countries.ToList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration(RegistrationModel model)
        {
            var emailChecked = _demoreppo.IsValidEmailAddress(model.Email);

            if (ModelState.IsValid && emailChecked)
            {
                HttpContext.Session.SetString("email", model.Email);
                Random random = new Random();
                long otp = random.Next(100000, 999999);

                SendEmailAsync(model.Email, otp);

                DateTime currentDateTime = DateTime.Now;
                DateTime expirationDateTime = currentDateTime.AddMinutes(5);
                var check = _dbcontext.UserOtps.FirstOrDefault(a => a.Email.ToLower() == model.Email.ToLower());
                var userExists = _demoreppo.AddUser(model);

                if (check == null)
                {
                    var myotp = new UserOtp();
                    myotp.Email = model.Email;
                    myotp.Otp = otp;
                    myotp.CreatedAt = DateTime.Now;
                    myotp.ExpiredAt = expirationDateTime;
                    _dbcontext.UserOtps.Add(myotp);
                    _dbcontext.SaveChanges();
                    TempData["success"] = "You have registered successfully.";
                    return RedirectToAction("OtpVerification", "Home");
                }
                else if (userExists == false && check == null)
                {
                    TempData["error"] = "Updated OTP Has been send..";
                    ViewBag.countries = _dbcontext.Countries.ToList();
                    return View(model);
                }
                else
                {
                    DateTime date = DateTime.Now;
                    DateTime expdate = currentDateTime.AddMinutes(5);

                    check.CreatedAt = DateTime.Now;
                    check.ExpiredAt = expdate;
                    check.Otp = otp;

                    _dbcontext.Update(check);
                    _dbcontext.SaveChanges();

                    TempData["success"] = "You have registered successfully.";
                    return RedirectToAction("OtpVerification", "Home");
                }
            }
            else if (emailChecked == false)
            {
                TempData["error"] = "Email is Not in format!!";
            }
            TempData["error"] = "Enter Appropriate Data";
            ViewBag.countries = _dbcontext.Countries.ToList();
            return View(model);
        }

        #endregion

        #region OtpVerification

        public IActionResult OtpVerification()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OtpVerification(UserOtp model)
        {
            if (model.Otp != 0)
            {
                var email = HttpContext.Session.GetString("email");
                if (email == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                var check = _dbcontext.UserOtps.Where(otp => otp.Email.ToLower() == email.ToLower() && otp.Otp == model.Otp).FirstOrDefault();


                if (check != null && email != null)
                {
                    TempData["success"] = "OTP has been verified and you have registered successfully.";
                    return RedirectToAction("Index", "Home");
                }
                else if (check == null)
                {
                    TempData["error"] = "OTP is not Correct..";
                    return RedirectToAction("Registration", "Home");
                }
                else
                {
                    if (_demoreppo.IsWithinFiveMinutes(check.CreatedAt) == false)
                    {
                        TempData["error"] = "Otp is expired!!";
                    }
                }
            }

            TempData["error"] = "Enter Appropriate OTP";
            return View(model);
        }

        #endregion

        #region SendOTP
        public Task SendEmailAsync(string email, long otp)
        {
            var subject = "Otp Verification for Registration";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("19it.dhruvgadhesariya@adit.ac.in", "Adit@884369")
            };
            return client.SendMailAsync(new MailMessage(from: "19it.dhruvgadhesariya@adit.ac.in", to: email, subject, "Your Otp is : " + otp));
        }

        #endregion

        #region Login


        public IActionResult Index()
        {
            HttpContext.SignOutAsync().Wait();
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginViewModel model)
        {

            var emailChecked = _demoreppo.IsValidEmailAddress(model.Email);

            if (ModelState.IsValid && emailChecked == true)
            {
                var check = _dbcontext.Users.FirstOrDefault(user => user.Email.ToLower() == model.Email.ToLower());

                if (check != null)
                {
                    bool verify = BCrypt.Net.BCrypt.Verify(model.Password, check.Password);
                    if (verify)
                    {
                        var claims = new List<Claim>
                        {
                        new Claim("Email", model.Email),
                        };
                        var identity = new ClaimsIdentity(claims, "AuthCookie");
                        var Principle = new ClaimsPrincipal(identity);
                        HttpContext.User = Principle;

                        var abc = HttpContext.SignInAsync(Principle);

                        HttpContext.Session.SetInt32("userid", (int)check.UserId);
                        TempData["success"] = "Login Successfully.";
                        return RedirectToAction("HomePage", "Home");
                    }
                    else
                    {
                        TempData["error"] = "Enter Valid Email or Password!!";
                        return View(model);
                    }
                }
                else
                {
                    TempData["error"] = "Enter Valid Email or Password!!";
                }
            }
            else if (emailChecked == false)
            {
                TempData["error"] = "Email is not in format!!";
            }
            TempData["error"] = "Enter Valid Email or Password!!";
            return View(model);
        }

        #endregion

        #region HomePage

        [Authorize]
        public IActionResult HomePage()
        {
            ViewBag.countries = _dbcontext.Countries.ToList();
            ViewBag.usersList = _dbcontext.Users.Where(a => a.DeletedAt == null).Skip((1 - 1) * 2).Take(2).ToList();
            ViewBag.Totalpages1 = Math.Ceiling(_dbcontext.Users.Where(a => a.DeletedAt == null).ToList().Count() / 2.0);
            ViewBag.currentPage = 1;
            return View();
        }

        public JsonResult GetCity(long countryId)
        {
            return Json(JsonSerializer.Serialize(_demoreppo.GetCityData(countryId)));
        }

        [HttpPost]
        public void AddUserByMe(User user)
        {
            var userEmail = _dbcontext.Users.Where(a => a.Email == user.Email && a.DeletedAt == null).Select(a => a.Email).FirstOrDefault();
            var emailChecked = _demoreppo.IsValidEmailAddress(user.Email);

            if (userEmail == null && emailChecked == true)
            {
                _demoreppo.addUserByMe(user);
                TempData["success"] = "User has been Added sucessfully";
            }
            else if (emailChecked == false)
            {
                TempData["error"] = "Email is InCorrect!!";
            }
            else
            {
                TempData["error"] = "User Already Exists You Can Edit Details";
            }
        }

        public string GetDataOfUser(long userId)
        {
            var data = JsonSerializer.Serialize(_demoreppo.GetUserDataForAdmin(userId));
            return data;
        }

        [HttpPost]
        public void EditUserByMe(User user)
        {
            var emailChecked = _demoreppo.IsValidEmailAddress(user.Email);

            if (emailChecked == true)
            {
                _demoreppo.editUserByAdmin(user);
                TempData["success"] = "User has been Updated sucessfully";
            }
            else
            {
                TempData["error"] = "Email is not in format!!";
            }

        }
        [HttpPost]
        public void RemoveByAdmin(long Id)
        {
            _demoreppo.RemoveByAdmin(Id);
            TempData["success"] = "You have Removed successfully!!";
        }

        [HttpPost]
        public ActionResult Search(UserSearchParams obj)
        {
            obj.Search = string.IsNullOrEmpty(obj.Search) ? "" : obj.Search;

            List<User> usersData = _demoreppo.FilterUsers(obj);
            ViewBag.usersList = usersData;
            var list = _dbcontext.Users.Where(a => a.DeletedAt == null).ToList();
            ViewBag.Totalpages1 = Math.Ceiling(list.Count() / 2.0);
            ViewBag.currentPage = obj.Pg;
            ViewBag.Finder = obj.Finder;
            ViewBag.Sort = obj.Sort;
            ViewBag.countries = _dbcontext.Countries.ToList();
            return PartialView("_UsersCRUD");
        }
        #endregion

        #region Privacy and error
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion
    }
}