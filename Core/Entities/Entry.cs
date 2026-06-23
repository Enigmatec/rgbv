using Core.Enums;

namespace Core.Entities
{
    public class Entry : BaseEnity<int>
    {
        public Field Field { get; set; }

        public string Value { get; set; }

        public string Key { get; set; }

        public EntryType Type { get; set; }
    }
}