using System;

namespace AlphaFacev2.Models
{
    public class History
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsActionSuccess { get; set; }
        public bool IsUserLoggedIn { get; set; }
        public string IpAddress { get; set; }
    }
}
