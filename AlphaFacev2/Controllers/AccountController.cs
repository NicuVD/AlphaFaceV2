using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlphaFacev2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AlphaFacev2.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAsync([Bind("FirstName,LastName,DateOfBirth,Gender,Email,UserName,Password")] Profile user)
        {
            if (ModelState.IsValid)
            {
                var newProfile = new Profile
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Email = user.Email,
                    Password = user.Password,
                    UserName = user.UserName
                };

                var loginTime = DateTime.Now;
                bool loginSucces = false;
                var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;

                LogInHistory(user, loginTime, loginSucces, ipAddress);

                _context.Profile.Add(newProfile);
                await _context.SaveChangesAsync();

                ModelState.Clear();
                ViewBag.Message = $"{user.FirstName} {user.LastName} was succesfully registered";
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Email,Password")] Profile user)
        {
            var loginTime = DateTime.Now;
            bool loginSucces = false;
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;

            var account = _context.Profile.FirstOrDefault(p => p.Email == user.Email && p.Password == user.Password);
            if (account != null)
            {
                loginSucces = true;
                LogInHistory(user, loginTime, loginSucces, ipAddress);

                HttpContext.Session.SetString("UserID", account.Id.ToString());
                HttpContext.Session.SetString("UserName", account.UserName);

                return RedirectToAction("Welcome");
            }
            else
            {
                LogInHistory(user, loginTime, loginSucces, ipAddress);

                ModelState.AddModelError("", "Username or password is incorrect!");
            }

            return View();
        }

        private static void LogInHistory(Profile user, DateTime loginTime, bool loginSucces, System.Net.IPAddress ipAddress)
        {
            var historyEntry = new History
            {
                Username = user.UserName,
                Password = user.Password,
                LoginTime = loginTime,
                IsLoginSuccess = loginSucces,
                IpAddress = ipAddress.ToString()
            };
        }

        public IActionResult Welcome()
        {
            if(HttpContext.Session.GetString("UserID") != null)
            {
                ViewBag.Username = HttpContext.Session.GetString("UserName");
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }

    #region OldCode
    //[HttpGet]
    //public IActionResult Register()
    //{
    //    return View();
    //}

    //[HttpPost]
    //public ActionResult Register(UserAccount user)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        var newProfile = new Profile
    //        {
    //            FirstName = user.FirstName,
    //            LastName = user.LastName,
    //            DateOfBirth = user.DateOfBirth,
    //            Gender = user.Gender,
    //            Email = user.Email,
    //            Password = user.Password,
    //            UserName = user.UserName
    //        };

    //        _context.Profile.Add(newProfile);
    //        _context.SaveChanges();

    //        ModelState.Clear();
    //        ViewBag.Message = $"{user.FirstName} {user.LastName} was succesfully registered";
    //    }

    //    return View();
    //}

    //[HttpGet]
    //public IActionResult Login()
    //{
    //    return View();
    //}

    //[HttpPost]
    //public IActionResult Login(UserAccount user)
    //{
    //    var account = _context.Profile.FirstOrDefault(p => p.UserName == user.UserName && p.Password == user.Password);
    //    if (account != null)
    //    {
    //        HttpContext.Session.SetString("UserID", account.Id.ToString());
    //        HttpContext.Session.SetString("UserName", account.UserName);

    //        return RedirectToAction("Welcome");
    //    }
    //    else
    //    {
    //        ModelState.AddModelError("", "Username or password is incorrect!");
    //    }

    //    return View();
    //}

    //public IActionResult Welcome()
    //{
    //    if (HttpContext.Session.GetString("UserID") != null)
    //    {
    //        ViewBag.Username = HttpContext.Session.GetString("UserName");
    //        return View();
    //    }
    //    else
    //    {
    //        return RedirectToAction("Login");
    //    }
    //}

    //public IActionResult Logout()
    //{
    //    HttpContext.Session.Clear();

    //    return RedirectToAction("Index");
    //}
    #endregion
}
