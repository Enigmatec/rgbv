using System;
using System.Collections.Generic;
using Core.Entities;
using Core.Enums;

namespace Service.Models
{
    public class AdminDashboardProjectModel
    {
        public int AgeOfSurvior { get; set; }

        public string SexOfSurvior { get; set; }

        public string VulnerablePopulation { get; set; }

        public DateTime DateOfIncident { get; set; }

        public DateTime DateReported { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public YesOrNo HasSurviorReceivedService { get; set; }

        public string TypeOfServiceProvidedToSurvior { get; set; } //Serilaized list *

        public string PerpetratorsInformation { get; set; }

        public YesOrNo DoestheSurviorWantJustice { get; set; }

        public int CaseCategoryId { get; set; }

        public int IncidentStateId { get; set; }
        public int IncidentLGAId { get; set; }

        public string WhoClosedTheCase { get; set; }

        public State IncidentState { get; set; }

        public ICollection<FollowUpProjection> FollowUpActions { get; set; } = new HashSet<FollowUpProjection>();

        //public CaseCategoryProjection Category { get; set; }

        public List<int> CaseCategoriesList { get; set; }

        public string CaseCategoriesOthers { get; set; }

        public string OutcomeOfProsecution { get; set; }

        public string SurvivorEstimatedAverageMonthlyIncome { get; set; }

        public string EmploymentStatus { get; set; }

        public string Education { get; set; }

        public string WhoReportedIncident { get; set; }
    }

    public class FollowUpProjection
    {
        public YesOrNo HasClientReceivedjustice { get; set; }
    }

    public class CaseCategoryProjection
    {
        public string Name { get; set; }
    }

    public class HomePageMetricsProjectionModel
    {
        public DateTime DateReported { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public YesOrNo DoestheSurviorWantJustice { get; set; }

        public int CaseCategoryId { get; set; }

        public int IncidentStateId { get; set; }

        public State IncidentState { get; set; }

        public string ReferralOutcome { get; set; }

        public YesOrNo WasViolenceFatal { get; set; }

        public bool IsApproved { get; set; }

        public YesOrNo HasCaseBeenClosed { get; set; }

        public string OutcomeOfServiceorReferral { get; set; }
        public string OutcomeOfProsecution { get; set; }
        public CaseCategoryProjection Category { get; set; }

        public List<int> CaseCategoriesList { get; set; }
    }
}