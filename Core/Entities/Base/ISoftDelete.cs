namespace Core.Entities
{
    public interface ISoftDelete
    {
        public bool IsSoftDeleted { get; set; }
    }
}