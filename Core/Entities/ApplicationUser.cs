using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class ApplicationUser : IdentityUser, ISoftDelete
    {
        public string Code { get; set; }

        [MaxLength(27)]
        public DateTime CreatedAt { get; set; }

        [MaxLength(27)]
        public DateTime? ModifiedAt { get; set; }

        [MaxLength(20)]
        public string FirstName { get; set; }

        [MaxLength(20)]
        public string LastName { get; set; }

        [MaxLength(27)]
        public string Type { get; set; }

        [MaxLength(30)]
        public string Designation { get; set; }
        public int? DonorId { get; set; }
        public int? StateId { get; set; }

        public int? OrganisationId { get; set; }
        public string ProfileUrl { get; set; }

        public bool IsActivated { get; set; }

        public bool IsSoftDeleted { get; set; }

        [MaxLength(27)]
        public DateTime LastLogin { get; set; }
        public Organisation Organisation { get; set; }
        public State State { get; set; }

        public int? LocalGovernmentAreaId { get; set; }
        public LocalGovernmentArea LocalGovernmentArea { get; set; }

        // public LocalGovernmentVo LocalGovernments { get; set; }

        public string LocalGovernmentAreas { get; set; }

        [NotMapped]
        public List<LgaModel> LocalGovernments => !string.IsNullOrWhiteSpace(LocalGovernmentAreas)
            ? new List<LgaModel>()
            : JsonConvert.DeserializeObject<List<LgaModel>>(LocalGovernmentAreas);

        public ICollection<Case> CreatedCases { get; set; } = new HashSet<Case>();

        public ICollection<Case> ApprovedCases { get; set; } = new HashSet<Case>();

        public ICollection<Case> ValidatedCases { get; set; } = new HashSet<Case>();

        [NotMapped] public string FullName => $"{FirstName} {LastName}";

        public ApplicationUser()
        {
            CreatedAt = DateTime.Now.ToUniversalTime().AddHours(1);
        }
    }

    public class LgaModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}