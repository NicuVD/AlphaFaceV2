using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaFacev2.Models
{
    public class AccountServices
    {
        private readonly AppDbContext _context;
        public List<History> histories { get; set; }
        public bool IsSignedIn { get; set; }

        public AccountServices(AppDbContext context)
        {
            _context = context;
            IsSignedIn = false;
        }

        public void Login()
        {
            var lastLogin = histories.FindLast(l => l.IsLoginSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (VerifyLoginData() == true)
            {
                if (user.IsLoggedIn == false)
                {
                    IsSignedIn = true;
                    var newUserLogin = new History();
                    // populate History table
                    // update current user profile login status
                }
            }
            else
            {
                // return error
            }
        }

        public void Logout()
        {
            var lastLogin = histories.FindLast(l => l.IsLoginSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (user.IsLoggedIn == true)
            {
                IsSignedIn = false;
                user.IsLoggedIn = false;
            }
        }

        public Profile GetCurrentUser()
        {
            var lastLogin = histories.FindLast(l => l.IsLoginSuccess == true);
            var user = _context.Profile.FirstOrDefault(p => p.UserName == lastLogin.Username);

            if (user.IsLoggedIn == true)
            {
                IsSignedIn = true;
                return user;
            }
            else
            {
                return new Profile();
            }
        }

        public bool VerifyLoginData()
        {
            // verify user input
            //HttpContent content = new StreamContent(); ???
            return false;
        }
    }
}
