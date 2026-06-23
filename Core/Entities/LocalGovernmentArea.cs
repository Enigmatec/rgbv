using System.Collections.Generic;

namespace Core.Entities
{
    public class LocalGovernmentArea : BaseEnity<int>
    {
        public string Name { get; set; }

        public string Key { get; set; }

        public int StateId { get; set; }

        public State State { get; set; }

        public ICollection<Ward> Wards { get; set; } = new HashSet<Ward>();

        public ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();
    }
}