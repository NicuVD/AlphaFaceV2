using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlphaFacev2.Models;
using AlphaFacev2.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using System.Data;

namespace AlphaFacev2.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AccountServices _accountServices;
        private readonly CognitiveServices _cognitiveServices;

        [TempData]
        public string StatusMessage { get; set; }

        public ProfilesController(AppDbContext context, AccountServices accountServices, CognitiveServices cognitiveServices)
        {
            _context = context;
            _accountServices = accountServices;
            _cognitiveServices = cognitiveServices;
        }

        // GET: Profiles
        public async Task<IActionResult> Index()
        {
            var user = _accountServices.GetCurrentUser();

            return View(await _context.Profile.ToListAsync());
        }

        //public async Task<IActionResult> Details()
        //{
        //    History lastLogin = _context.History.LastOrDefault(l => l.IsActionSuccess == true);
        //    var user = await _context.Profile.FirstOrDefaultAsync(p => p.UserName == lastLogin.Username);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        // GET: Profiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Profile user;

            if (id == null)
            {
                History lastLogin = _context.History.LastOrDefault(l => l.IsActionSuccess == true);
                user = await _context.Profile.FirstOrDefaultAsync(p => p.UserName == lastLogin.Username);
                if (user.IsLoggedIn == false)
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                user = await _context.Profile.FirstOrDefaultAsync(p => p.Id == id);
            }

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
            if (profile.IsLoggedIn == false)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Gender,Email,Password,ConfirmPassword")] Profile profile)
        {
            if (id != profile.Id)
            {
                return NotFound();
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                var user = await _context.Profile.FirstOrDefaultAsync(u => u.Id == profile.Id);
                if (user.IsLoggedIn)
                {
                    if (profile.Password == user.Password)
                    {
                        user.FirstName = profile.FirstName;
                        user.LastName = profile.LastName;
                        user.DateOfBirth = profile.DateOfBirth;
                        user.Gender = profile.Gender;
                        user.Email = profile.Email;
                        user.UserName = profile.Email;

                        await Logout();
                        await LoginAsync(user);

                        try
                        {
                            _context.Profile.Update(user);
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
                        return RedirectToAction(nameof(Details));
                    }
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return RedirectToAction("Details");
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
            if (profile.IsLoggedIn == false)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profile = await _context.Profile.FindAsync(id);

            if (profile.IsLoggedIn)
            {
                var histories = await _context.History.ToListAsync();

                var faces = await _context.Face.ToListAsync();

                if (profile.IsLoggedIn == true)
                {
                    await Logout();
                }

                foreach (var item in histories)
                {
                    if (item.Username == profile.UserName)
                    {
                        _context.History.Remove(item);
                    }
                }

                foreach (var item in faces)
                {
                    if (item.Id == profile.Id)
                    {
                        _context.Face.Remove(item);
                    }
                }

                _context.Profile.Remove(profile);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), "Home");
            }
            else
            {
                return RedirectToAction(nameof(Login));
            }
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
        public async Task<IActionResult> RegisterAsync([Bind("FirstName,LastName,DateOfBirth,Gender,Email,Password,ConfirmPassword")] Profile user)
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
                    //ConfirmPassword = user.ConfirmPassword,
                    UserName = user.Email,
                    IsLoggedIn = true // this must be set to false when logging out
                };

                var loginTime = DateTime.Now;
                var ipAddress = GetIpAdress();
                bool loginSucces = true;
                bool isUserLoggedIn = true;
                var historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLoggedIn);

                _context.History.Add(historyEntry);
                _context.Profile.Add(newProfile);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), "Home");
            }

            return RedirectToAction("Register", "Profiles");

        }


        private static History LogHistory(Profile user, DateTime loginTime, bool loginSucces, string ipAddress, bool isUserLogedIn)
        {
            History historyEntry = new History
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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync([Bind("Email, Password")]Profile user)
        {
            if (_context.Profile.Count(p => p.Email == user.Email) > 0)
            {
                var loginTime = DateTime.Now;
                bool loginSucces = false;
                var ipAddress = GetIpAdress();
                bool isUserLogedIn = false;
                History historyEntry = new History();

                Profile account = _context.Profile.FirstOrDefault(p => p.Email == user.Email && p.Password == user.Password);
                if (account != null)
                {
                    if (account.IsLoggedIn == false)
                    {
                        user = account;
                        user.IsLoggedIn = true;
                        loginSucces = true;
                        isUserLogedIn = true;
                        historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

                        _context.History.Add(historyEntry);
                        _context.Profile.Update(user);
                        await _context.SaveChangesAsync();

                        SetContextOnLoginOrRegister(account);

                        return RedirectToAction(nameof(Index), "Home");
                    }
                }
                else
                {
                    historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);
                    _context.History.Add(historyEntry);
                    await _context.SaveChangesAsync();

                    ModelState.AddModelError("", "Username or password is incorrect!");
                }
            }

            return RedirectToAction("Login");
        }

        private void SetContextOnLoginOrRegister(Profile user)
        {
            HttpContext.Session.SetString("FirstName", user.FirstName);
            HttpContext.Session.SetString("UserName", user.UserName);
        }

        [HttpGet]
        public IActionResult LoginAFace()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAFace([Bind("Email")]Profile user)
        {
            // check if account with input email exists
            if (_context.Profile.Count(p => p.Email == user.Email) > 0)
            {
                // prepare data for history log
                var loginTime = DateTime.Now;
                bool loginSucces = false;
                var ipAddress = GetIpAdress();
                bool isUserLogedIn = false;
                History historyEntry = new History();
                ImageStore webcamImage = await _context.ImageStore.LastOrDefaultAsync();
                var profileImage = _context.Profile.FirstOrDefault(p => p.Email == user.Email).ProfileImage;

                if (profileImage == null)
                {
                    return RedirectToAction("Login");
                }

                // get result from Microsoft Face API
                var result = await AFaceCompareUserPictureToWebcamPicture(webcamImage, profileImage);

                if (result != null)
                {
                    // get user account with corresponding email only if Face API returns
                    // that the faces are identical with a confidence of over 0.6
                    Profile account = _context.Profile.FirstOrDefault(
                        p => (p.Email == user.Email) &&
                            (result.IsIdentical == true) &&
                                (result.Confidence >= 0.6));

                    if (account != null)
                    {
                        if (account.IsLoggedIn == false)
                        {
                            user = account;
                            user.IsLoggedIn = true;
                            loginSucces = true;
                            isUserLogedIn = true;
                            historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

                            _context.History.Add(historyEntry);
                            _context.Profile.Update(user);
                            _context.ImageStore.Remove(webcamImage);
                            await _context.SaveChangesAsync();

                            SetContextOnLoginOrRegister(account);

                            return RedirectToAction(nameof(Index), "Home");
                        }
                    }
                    else
                    {
                        historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);
                        _context.History.Add(historyEntry);
                        await _context.SaveChangesAsync();

                        ModelState.AddModelError("", "Username is incorrect or image comparison did not work properly!");
                    }
                }
                else
                {
                    return RedirectToAction("LoginAFace");
                }
            }

            return RedirectToAction("LoginAFace");
        }


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

        public async Task<IActionResult> Logout()
        {
            var loginTime = DateTime.Now;
            bool loginSucces = false;
            var ipAddress = GetIpAdress();
            History historyEntry = new History();
            bool isUserLogedIn = false;

            var lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (user.IsLoggedIn == true)
            {
                user.IsLoggedIn = false;
                loginSucces = true;
                lastLogin.IsUserLoggedIn = false;
            }

            historyEntry = LogHistory(user, loginTime, loginSucces, ipAddress, isUserLogedIn);

            _context.History.Update(lastLogin);
            _context.History.Add(historyEntry);
            _context.Profile.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Index), "Home");
        }

        public Profile GetCurrentUser()
        {
            History lastLogin = _context.History.LastOrDefault(l => l.IsActionSuccess == true);

            if (lastLogin != null)
            {
                lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
                var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

                if (user.IsLoggedIn == true)
                {
                    SetContextOnLoginOrRegister(user);

                    return user;
                }
                else
                {
                    return new Profile();
                }
            }
            else
            {
                return new Profile();
            }
        }

        [HttpPost]
        public async Task<VerifiedFace> AFaceCompareUserPictureToWebcamPicture(ImageStore webcamImage, byte[] profileImage)
        {
            Stream userImage = new MemoryStream(webcamImage.ImageByteArray);
            Stream uploadedImage = new MemoryStream(profileImage);

            return await _cognitiveServices.VerifyAsync(userImage, uploadedImage);
        }



        public async Task<IActionResult> UpdateProfilePicture()
        {
            var lastLogin = _context.History.Last(l => l.IsActionSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);
            var profilePicture = await _context.ImageStore.LastOrDefaultAsync();

            // Added for reading the file as byte array an updating the database
            byte[] imageByteArray = profilePicture.ImageByteArray;

            var profileToUpdate = _context.Profile.FirstOrDefault(p => p.UserName == user.UserName);
            profileToUpdate.ProfileImage = imageByteArray;
            _context.Update<Profile>(profileToUpdate);
            await _context.SaveChangesAsync();
            _context.ImageStore.Remove(profilePicture);  // ADDED!!!

            StatusMessage = "Your profile picture has been updated";
            return RedirectToAction("Index", "Profiles");
        }



        public static string GetIpAdress()
        {
            IPHostEntry server = Dns.GetHostEntry(Dns.GetHostName());
            var ip = server.AddressList[1].ToString();

            return ip;
        }

    }
}
