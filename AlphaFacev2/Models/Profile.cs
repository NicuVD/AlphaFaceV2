using System;
using System.ComponentModel.DataAnnotations;

namespace AlphaFacev2.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string IpAdress { get; set; }
        public byte[] ProfileImage { get; set; }
        public bool IsLoggedIn { get; set; }
        public string Password { get; set; } // added
    }
}
