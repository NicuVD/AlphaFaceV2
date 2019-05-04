using System;
using System.ComponentModel.DataAnnotations;

namespace AlphaFacev2.Models
{
    public class Profile
    {
        //[Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }


        public string IpAddress { get; set; }


        public byte[] ProfileImage { get; set; }


        public bool IsLoggedIn { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } // added

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }

    public enum Genders
    {
        Male = 1,
        Female = 2,
        Other = 3
    }
}
