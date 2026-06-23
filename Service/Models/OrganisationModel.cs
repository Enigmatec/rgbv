using Core.Enums;
using Service.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Service.Models
{
    public class OrganisationCreationModel
    {
        [Phone]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [ListCannotBeEmpty(ErrorMessage = "Must have at least one state")]
        public List<StateList> States { get; set; }

        //[Required]
        public string Address { get; set; }

        [Required]
        public OrganisationType OrganisationType { get; set; }

        public string Acronym { get; set; }

        public string Website { get; set; }

        public string HotLine { get; set; }

        public List<string> TypeOfService { get; set; }

        public IEnumerable<SocialMediaData> SocialMediaData { get; set; }
    }

    public class OrganisationViewModel : OrganisationCreationModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int NumberOfUsers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // public List<SocialMediaData> SocialMediaData { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
    }

    public class OrganisationListModel
    {
        public int Id { get; set; }
        public OrganisationType OrganisationType { get; set; }

        public string Name { get; set; }

        public List<StateList> States { get; set; }

        public IEnumerable<SocialMediaData> SocialMediaData { get; set; }
    }

    public class OrganisationSearchModel : PaginationModel
    {
        public OrganisationType? OrganisationType { get; set; }
        public int? StateId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class StateListModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public List<LGAListModel> LocalGovernmentAreas { get; set; } = new List<LGAListModel>();
    }

    public class StateList
    {
        [Required]
        [ValueCannotBeZero]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class StateComparer : IEqualityComparer<StateList>
    {
        public bool Equals([AllowNull] StateList x, [AllowNull] StateList y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] StateList obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class LGAListModel
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public List<WardListModel> Wards { get; set; } = new List<WardListModel>();
    }

    public class WardListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SocialMediaData
    {
        public string Name { get; set; }

        public string Handle { get; set; }

        public string Url { get; set; }
    }
}