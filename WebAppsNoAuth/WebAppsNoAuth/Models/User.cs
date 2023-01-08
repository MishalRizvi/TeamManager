using System;
using System.ComponentModel.DataAnnotations;

namespace WebAppsNoAuth.Models
{
	public class User
	{
		public User()
		{
		}


        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter email address")]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter phone number")]
        [Display(Name = "Phone Number")]
        [Phone]
        public string ContactNumber { get; set; }

        public bool Admin { get; set; }
        public int InstitutionId { get; set; }
        public string Institution { get; set; }
        public int ManagerUserId { get; set; }
        public string ManagerUserName { get; set; }
        public bool Manager { get; set; }
    }
}

