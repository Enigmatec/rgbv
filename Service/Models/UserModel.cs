using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.Models
{
    public class RolesListModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class UserCreationModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int? DonorId { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "CSO/Service Provider")]
        public int? OrganisationId { get; set; }

        [Required]
        public string Role { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        // [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string Designation { get; set; }

        public int? StateId { get; set; }

        public List<int> LocalGovernmentIds { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Organisation { get; set; }

        public string State { get; set; }
        public string StateCode { get; set; }

        public DateTime LastLogin { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public bool IsActivated { get; set; }

        public string Role { get; set; }
        public string Designation { get; set; }
        public int? StateId { get; set; }

        public int? OrganisationId { get; set; }


        public List<LgaModel> LocalGovernments { get; set; }
    }

    public class LocalGovernmentModel
    {
        public int? LocalGovernmentId { get; set; }
        public string LocalGovernmentName { get; set; }
    }

    public class UserProfileUpdateModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public string Designation { get; set; }
    }

    public class UserProfileViewModel : UserProfileUpdateModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }
        public int? StateId { get; set; }

        public int? OrganisationId { get; set; }

        public string State { get; set; }
        public string Organisation { get; set; }
        public string ProfileUrl { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int NumberOfCaseEntered { get; set; }
        public List<LgaModel> LocalGovernments { get; set; }
    }

    public class UserSearchModel : PaginationModel
    {
        public int? StateId { get; set; }

        public string Organisation { get; set; }

        public int? OrganisationId { get; set; }

        public string Role { get; set; }

        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}