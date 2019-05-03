using AlphaFacev2.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaFacev2.Services
{
    public class AccountServices
    {
        private readonly AppDbContext _context;

        public AccountServices(AppDbContext context)
        {
            _context = context;
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

    }
}
