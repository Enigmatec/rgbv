using Core.Enums;
using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class UpdateServiceProvidedRequest
    {
        public int Id { get; set; }
        public int AgeOfSurvivorInYears { get; set; }
        public string SexOfSurvivorOrVictim { get; set; }

        public string TypeOfClient { get; set; }

        public YesOrNo HasSurvivorReceivedServiceFromAnotherOrganisation { get; set; }
        public string IncidentCodeFromReferringOrganisation { get; set; }
        public List<string> TypeOfServiceReceivedAnotherOrganisation { get; set; }
        public string ReferralCode { get; set; }
        public List<string> TypeOfServiceNeeded { get; set; }
        public List<string> TypeOfServiceProvided { get; set; }
        public List<string> TypeOfServiceReferredFor { get; set; }
        public string NameOfServiceProviderOrCsoReferredTo { get; set; }
        public string ReferralOutcome { get; set; }
        public int OrganisationId { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public DateTime? DateOfServiceProvision { get; set; }

        public string GbvCovid19Question1 { get; set; }

        public string GbvCovid19Question2 { get; set; }

        public string GbvCovid19Question3 { get; set; }

        public string GbvCovid19Question4 { get; set; }

        public int? OrganisationLgaId { get; set; }

        public string IncidentCode { get; set; }

        public string ReferralToAnotherCsoOrSPcode { get; set; }
    }
}