using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AlphaFacev2.Models;
using Microsoft.AspNetCore.Http;

namespace AlphaFacev2.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (user.IsLoggedIn == true)
            {
                HttpContext.Session.SetString("UserID", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.UserName);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
