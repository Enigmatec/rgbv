using System.Collections.Generic;

namespace Core.Entities
{
    public class Donor: BaseEnity<int>
    {
        public List<Organisation> Organisations { get; set; }=new List<Organisation>();
        public List<ApplicationUser> Users { get; set; }=new List<ApplicationUser>();
        public string Acronym { get; set; }
        public string Name { get; set; }
        public string CreatedById { get; set; }

    }
}
