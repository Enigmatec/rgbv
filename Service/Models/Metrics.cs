using Core.Enums;
using Service.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class BaseMetrics
    {
        public int NewCases { get; set; }

        public int FollowUpCases { get; set; }

        public int TotalCases { get; set; }

        public int TotalCasesToday { get; set; }
        public int TotalCasesThisMonth { get; set; }

        public int TotalApprovedCases { get; set; }

        public int TotalUnApprovedCases { get; set; }

        public int TotalUnValidatedCases { get; set; }

        public int TotalValidatedCases { get; set; }

        public int TotalCasesByFemale { get; set; }

        public int TotalCasesByMale { get; set; }

        public int TotalFatalCases { get; set; }

        public int ClosedCases { get; set; }

        public int OpenCases { get; set; }

        public int CaseAgeLessThan18 { get; set; }

        public int CaseAgeGreatThanOrEqual18 { get; set; }

        public int CaseFemaleLestThan18 { get; set; }
        public int CaseFemaleGreatThanOrEqual18 { get; set; }

        public int CaseMaleLestThan18 { get; set; }
        public int CaseMaleGreatThanOrEqual18 { get; set; }
        public int ConvictedPerpetuators { get; set; }

        public List<CaseBySubject> CaseByLGA { get; set; }
        public IEnumerable<CaseBySubject> CaseByReportedYear { get; set; }
        public IEnumerable<CaseBySubject> CaseByMonthOfPresentYear { get; set; }
        public List<CaseBySubject> CaseByIncidentType { get; set; }
        public IEnumerable<CaseBySubject> CaseByIncidentYear { get; set; }
        public List<CaseBySubject> CaseByVulnerable { get; set; }

        public List<CaseBySubjectBySex> CasesByAgeGroupBySex { get; set; }

        public IEnumerable<CaseBySubject> CasesByTimeOfViolience { get; set; }

        public IEnumerable<CaseBySubjectBySex> PerpetratorsByRelationshipBySex { get; set; }

        public List<CaseBySubjectBySex> AccessToJusticeByTypeOfViolenceBySex { get; set; }

        public List<CaseBySubjectBySex> AccessToJusticeByAgeGroupBySex { get; set; }
        public List<CaseBySubject> ConvictedPerpetratorsByTypeOfViolence { get; set; }

        public List<CaseBySubjectBySex> CasesByWhoClosedCaseBySex { get; set; }

        public List<CaseBySubjectBySex> RecievedServiceOfPolice { get; set; }

        public List<CaseBySubjectBySex> RecievedLegalService { get; set; }

        public List<CaseByYear> CasesByYearByMonths { get; set; }

        public int NoOfPeopleWhoWantJustice { get; set; }

        public List<CaseBySubject> CaseByTypeOfServiceProvided { get; set; }
        public UserViewModel User { get; set; }
    }

    public class DashBoardSearch
    {
        public List<int> CaseCategories { get; set; }

        public List<int> Year { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }

        public string Gender { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public List<int> Lgas { get; set; }
    }

    public class SearchModel : DashBoardSearch
    {
        public List<int> States { get; set; }

        public string IsCaseValidated { get; set; }

        public string OrganisationName { get; set; }

        public bool? HasSurvivorReceivedService { get; set; }

        public OrganisationType? OrganisationType { get; set; }
        //public CaseCategoryOrTypeOfViolence? ViolenceType { get; set; }

    }

    public class CSOandSPDashBoardModel : BaseMetrics
    {
        public int TotalCasesEnteredByUser { get; set; }
    }

    public class SuperVisorDashBoardModel : BaseMetrics
    {
        public int NumberOfCSO { get; set; }
        public int NumberOfSP { get; set; }
        public int? NumberOfPart { get; set; }
        public int NumberOfOrganisations { get; set; }
        public IEnumerable<CaseBySubject> CaseByState { get; set; }
        public int TotalCasesEnteredByUser { get; set; }
    }

    public class AdminDashBoardModel : SuperVisorDashBoardModel
    {
        public int NoOfUsers { get; set; }
        public PropOfCasessWithConvictedPerp PropOfCasessWithConvictedPerp { get; set; }
    }

    public class HomeMetricsModel : SuperVisorDashBoardModel
    {
    }

    public class CaseBySubject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }
    }

    public class CaseBySubjectBySex
    {
        public string Subject { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int OtherCount { get; set; }

        public int TotalNewCases { get; set; }

        public int TotalFollowUpCases { get; set; }

        public int TotalCount => MaleCount + FemaleCount + OtherCount;
    }

    public class CaseByYear
    {
        public string Month { get; set; }
        public List<CaseBySubject> Years { get; set; }
    }

    public class CaseByStateAndLgaFilter
    {
        public List<int> Lgas { get; set; }

        public List<int> States { get; set; }
    }

    public class MonthlyCasesByWardsFilter
    {
        public List<int> Lgas { get; set; }

        public List<int> States { get; set; }

        public List<int> Wards { get; set; }
    }
}