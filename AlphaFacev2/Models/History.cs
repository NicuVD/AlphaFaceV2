using System;

namespace AlphaFacev2.Models
{
    public class History
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsLoginSuccess { get; set; }
        public string IpAddress { get; set; }
    }
}
