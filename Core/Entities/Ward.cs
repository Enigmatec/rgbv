namespace Core.Entities
{
    public class Ward : BaseEnity<int>
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public int LocalGovernmentAreaId { get; set; }

        public LocalGovernmentArea LocalGovernmentArea { get; set; }
    }
}