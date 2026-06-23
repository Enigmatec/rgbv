using Core.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Organisation : BaseEnity<int>, ISoftDelete
    {
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(6)]
        public string Code { get; set; } // 4 digit Unique Code

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string States { get; set; } //serialized list of state id; or state model

        [MaxLength(15)]
        public OrganisationType OrganisationType { get; set; }

        public string Acronym { get; set; }

        public string Website { get; set; }

        public string HotLine { get; set; }

        public string TypeOfService { get; set; }

        public string SocialMediaData { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();
        public bool IsSoftDeleted { get; set; }

        //public string LocalGovernmentAreaId { get; set; }

        // public State State { get; set; } public LocalGovernmentArea LocalGovernmentArea { get;
        // set; }
    }
}