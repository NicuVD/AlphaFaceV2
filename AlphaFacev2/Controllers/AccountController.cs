using System;
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(User user)
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

                _context.Profile.Add(newProfile);
                await _context.SaveChangesAsync();

                ModelState.Clear();
                ViewBag.Message = $"{user.FirstName} {user.LastName} was succesfully registered";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var account = _context.Profile.FirstOrDefault(p => p.UserName == user.UserName && p.Password == user.Password);
            if(account != null)
            {
                HttpContext.Session.SetString("UserID", account.Id.ToString());
                HttpContext.Session.SetString("UserName", account.UserName);

                return RedirectToAction("Welcome");
            }
            else
            {
                ModelState.AddModelError("", "Username or password is incorrect!");
            }

            return View();
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
