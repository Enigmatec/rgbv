using Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name or Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string ExpiryTime { get; set; }
        public string Role { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public int? StateId { get; set; }
        public int? OrganisationId { get; set; }
        public List<LgaModel> LocalGovernments { get; set; }
        public string State { get; set; }
        public string Organisation { get; set; }
        public string Designation { get; set; }
        public List<int> DonorOrgsId { get; set; } = new List<int>();
    }

    public class ResetPasswordModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        public string Url { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string AdminUserId { get; set; }
    }
}