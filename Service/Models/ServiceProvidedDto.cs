using Core.Entities;
using Core.Enums;
using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class ServiceProvidedDto
    {
        public int Id { get; set; }

        public int SerialNumber { get; set; }

        public string ServiceProvisionCode { get; set; }

        public int AgeOfSurvivorInYears { get; set; }

        public string SexOfSurvivorOrVictim { get; set; }

        public string TypeOfClient { get; set; }

        public YesOrNo HasSurvivorReceivedServiceFromAnotherOrganisation { get; set; }

        public string IncidentCodeFromReferringOrganisation { get; set; }

        public string ReferralCode { get; set; }

        public List<string> TypeOfServiceReceivedAnotherOrganisationList { get; set; }

        public List<string> TypeOfServiceNeededList { get; set; }

        public List<string> TypeOfServiceProvidedList { get; set; }

        public List<string> TypeOfServiceReferredForList { get; set; }

        public string NameOfServiceProviderOrCsoReferredTo { get; set; }

        public string ReferralOutcome { get; set; }

        public int OrganisationId { get; set; }
        public string OrganisationName { get; set; }

        public int StateId { get; set; }
        public string StateName { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public DateTime? DateOfServiceProvision { get; set; }

        public ValidationStatus Status { get; set; }

        public string CreatedById { get; set; }

        public string CreatedByName { get; set; }

        public string SpOrCsoApprovalById { get; set; }

        public string SpOrCsoApprovalByName { get; set; }

        public DateTime? SpOrCsoApprovalDate { get; set; }

        public string StateApprovalById { get; set; }

        public string StateApprovalByName { get; set; }

        public DateTime? StateApprovalDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string GbvCovid19Question1 { get; set; }

        public string GbvCovid19Question2 { get; set; }

        public string GbvCovid19Question3 { get; set; }

        public string GbvCovid19Question4 { get; set; }

        public int? OrganisationLgaId { get; set; }
        public string LGA { get; set; }
        public string IncidentCode { get; set; }

        public string ReferralToAnotherCsoOrSPcode { get; set; }
    }

    public class ServiceViewBackground
    {
        public UserViewModel User { get; set; }
        public List<ServiceProvidedDto> Services { get; set; }
    }

}