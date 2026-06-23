using System.Collections.Generic;

namespace Core.Entities
{
    public class State : BaseEnity<int>
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public string Code { get; set; }

        public ICollection<LocalGovernmentArea> LocalGovernmentAreas { get; set; } = new HashSet<LocalGovernmentArea>();
    }
}