using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models.ViewModels
{
    public class StateDashVM
    {
        public string Name { get; set; }
        public int TotalNumber { get; set; }
    }

    public class ServicesDashboard
    {
        public List<CaseBySubject> ServicesBySubject { get; set; }=new List<CaseBySubject>();
        public List<CaseBySubjectBySex> ServicesBySex { get; set; }=new List<CaseBySubjectBySex>();
    }

    public class StateHomeVM
    {
        public List<StateDashVM> StateDashVMs { get; set; } = new List<StateDashVM>();
    }
    public class CSOProviders
    {
        public string State { get; set; }
        public int CSOsProviders { get; set; }
        //public int Providers { get; set; }
    }

    public enum DashboardDetails
    {
        Incident, Services
    }

    public class CSOVM
    {
        public string Name { get; set; }

    }
    public class SPVM
    {
        public string Name { get; set; }

    }

    public class CSOSPVM
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Type { get; set; }

    }

    public class TotalNumberofCSOSP
    {
        public int Total { get; set; }

    }




}
