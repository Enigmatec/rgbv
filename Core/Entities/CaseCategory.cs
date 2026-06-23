using System.Collections.Generic;

namespace Core.Entities
{
    public class CaseCategory : BaseEnity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Case> Cases = new HashSet<Case>();
    }
}