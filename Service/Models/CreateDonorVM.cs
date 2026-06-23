using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models
{
    public class CreateDonorVM
    {
        public string Name { get; set; }
        public string Acronym { get; set; }
        public List<int> OrganisationIds { get; set; }
    }

    public class EditDonorVM
    {
        public int DonorId { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public List<int> OrganisationIds { get; set; }
    }

    public class AddUserToDonor
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Designation")]
        public string Designation { get; set; }

        public int DonorId { get; set; }
    }

    public class EditUserDonor
    {
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Designation")]
        public string Designation { get; set; }

        public string userId { get; set; }
    }

    public class DonorVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public DateTime DateCreated { get; set; }
        public int NumberOfOrganisations { get; set; }
        public List<DonorUsersVM> DonorUsers { get; set; } = new List<DonorUsersVM>();
        public List<int> OrganisationIds { get; set; }
    }
    public class DonorUsersVM
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public string Designation { get; set; }
    }

    public class DonoSearchModel : PaginationModel
    {
        public string Name { get; set; }
        public string Acronym { get; set; }
    }

    public class DonorUsersScreenVM
    {
        public int NumberOfOrganisations { get; set; }
        public int NumberOfIncidentsFromAllOrganisations { get; set; }
        public int NumberOfServicesFromAllOrganisations { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public int MembersCount { get; set; }
        public DateTime DateCreated { get; set; }

    }

    public class NumberOfIncidentsFromAllOrganisationsVM
    {
        public int OrganisationId { get; set; }
    }

    public class DonorInformationVM
    {
        public string OrganisationName { get; set; }
        public int OrganisationId { get; set; }
        public int NumberOfReportedIncidents { get; set; }
        public int NoOfServices { get; set; }
        public DateTime DateCreated { get; set; }
    }

}
