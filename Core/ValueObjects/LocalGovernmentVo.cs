using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.ValueObjects
{
    public class LocalGovernmentVo : ValueObject<LocalGovernmentVo>
    {
        private LocalGovernmentVo()
        {
        }

        private string LocalGovernmentAreas { get; }

        public LocalGovernmentVo(List<Data> localGovernments)
        {
            LocalGovernmentAreas = JsonConvert.SerializeObject(localGovernments);
        }

        public List<Data> Values => JsonConvert.DeserializeObject<List<Data>>(string.IsNullOrWhiteSpace(LocalGovernmentAreas) ? "[]" : LocalGovernmentAreas);

        [NotMapped]
        public class Data
        {
            private Data()
            {
            }

            public Data(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; }

            public string Name { get; }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return LocalGovernmentAreas;
        }
    }
}