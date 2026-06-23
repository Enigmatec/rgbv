
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.ViewModels
{
    public class StatesVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public string Code { get; set; }

        public ICollection<LocalGovernmentsVM> LocalGovernmentAreas { get; set; }
    }


    public class LocalGovernmentsVM
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Key { get; set; }
        public ICollection<WardsVM> Wards { get; set; }
    }

    public class WardsVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

    }
}
