using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlphaFacev2.Models;
using Microsoft.AspNetCore.Http;

namespace AlphaFacev2.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly AppDbContext _context;

        public ProfilesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Profiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Profile.ToListAsync());
        }

        // GET: Profiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //History lastLogin = _context.History.LastOrDefault(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Profiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Profiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth,Gender,Email,UserName,IpAdress,ProfileImage")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        // GET: Profiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profile.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,Email,UserName,IpAdress,ProfileImage")] Profile profile)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(profile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profile = await _context.Profile.FindAsync(id);
            _context.Profile.Remove(profile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfileExists(int id)
        {
            return _context.Profile.Any(e => e.Id == id);
        }

        [HttpGet]
        public IActionResult Register()
        {
            //var dummyProfile = new Profile
            //{
            //    FirstName = "Vlad",
            //    LastName = "Deac",
            //    DateOfBirth = DateTime.Now,
            //    Gender = "male",
            //    Email = "vlad@vlad.com",
            //    Password = "password",
            //    UserName = "vladd"
            //};
            return View();
        }

        // POST: Profiles/RegisterAsync
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAsync([Bind("FirstName,LastName,DateOfBirth,Gender,Email,Password")] Profile user)
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
                    UserName = user.Email,
                    IsLoggedIn = true // this must be set to false when logging out
                };

                var loginTime = DateTime.Now;
                var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
                bool loginSucces = true;
                bool isUserLogedIn = true;
                var historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

                _context.History.Add(historyEntry);
                _context.Profile.Add(newProfile);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // return View(); ???
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            //var dummyLoginProfile = new Profile
            //{
            //    UserName = "vladd",
            //    Password = "password"
            //};
            //return View(dummyLoginProfile);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync([Bind("UserName, Password")]Profile user)
        {
            var loginTime = DateTime.Now;
            bool loginSucces = false;
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            bool isUserLogedIn = false;
            var historyEntry = new History();

            var account = _context.Profile.FirstOrDefault(p => p.UserName == user.UserName && p.Password == user.Password);
            if (account != null)
            {
                user = account;
                user.IsLoggedIn = true;
                loginSucces = true;
                isUserLogedIn = true;
                historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

                _context.History.Add(historyEntry);
                _context.Profile.Update(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserID", account.Id.ToString());
                HttpContext.Session.SetString("UserName", account.UserName);

                return RedirectToAction("Welcome");
            }
            else
            {
                historyEntry = LogHistory(account, loginTime, loginSucces, ipAddress, isUserLogedIn);
                _context.History.Add(historyEntry);
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Username or password is incorrect!");
            }

            return View();
        }

        private static History LogHistory(Profile user, DateTime loginTime, bool loginSucces, System.Net.IPAddress ipAddress, bool isUserLogedIn)
        {
            var historyEntry = new History
            {
                Username = user.Email,
                Password = user.Password,
                LoginTime = loginTime,
                IsActionSuccess = loginSucces,
                IpAddress = ipAddress.ToString(),
                IsUserLoggedIn = isUserLogedIn
            };

            return historyEntry;
        }

        public IActionResult Welcome()
        {
            if (HttpContext.Session.GetString("UserID") != null)
            {
                ViewBag.Username = HttpContext.Session.GetString("UserName");
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public async Task<IActionResult> Logout()
        {
            var loginTime = DateTime.Now;
            bool loginSucces = false;
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            var historyEntry = new History();
            bool isUserLogedIn = false;

            var lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (user.IsLoggedIn == true)
            {
                var IsSignedIn = false; // need to update history with this property
                user.IsLoggedIn = false;
                loginSucces = true;
            }
            // need to update Db entry in History() and Profile(change user status to not logged in)

            historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

            _context.History.Add(historyEntry);
            _context.Profile.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        //public async void LogoutAllUsers()
        //{
        //    List<Profile> users = await _context.Profile.ToListAsync();
        //    foreach (var user in users)
        //    {
        //        user.IsLoggedIn = false;
        //        _context.Profile.Update(user);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        //public async void LogoutExpiredSession()
        //{
        //    List<Profile> users = await _context.Profile.ToListAsync();
            
        //    foreach (var user in users)
        //    {
        //        History history = await _context.History.FirstOrDefaultAsync(h => h.Username == user.UserName && user.IsLoggedIn == true);

        //        if((history.LoginTime - DateTime.Now).TotalMinutes >= 15)
        //        {
        //            user.IsLoggedIn = false;
        //            _context.Profile.Update(user);
        //            await _context.SaveChangesAsync();
        //        }
        //    }
        //}
    }
}
