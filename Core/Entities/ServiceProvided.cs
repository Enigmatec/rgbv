using Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Core.Entities
{
    public class ServiceProvided : BaseEnity<int>
    {
        private ServiceProvided()
        {
        }

        public ServiceProvided(
            int serialNumber,
            string serviceProvisionCode,
            int ageOfSurvivorInYears,
            string sexOfSurvivorOrVictim,
            string typeOfClient,
            YesOrNo hasSurvivorReceivedServiceFromAnotherOrganisation,
            string incidentCodeFromReferringOrganisation,
            List<string> typeOfServiceReceivedAnotherOrganisation,
            string referralCode,
            List<string> typeOfServiceNeeded,
            List<string> typeOfServiceProvided,
            List<string> typeOfServiceReferredFor,
            string nameOfServiceProviderOrCsoReferredTo,
            string referralOutcome,
            int organisationId,
            int stateId,
            string longitude,
            string latitude,
            DateTime? dateOfServiceProvision,
            string createdById,
            string gbvCovid19Question1,
            string gbvCovid19Question2,
            string gbvCovid19Question3,
            string gbvCovid19Question4,
            int? organisationLgaId,
            string incidentCode, string referralToAnotherCsoOrSPcode)
        {
            SerialNumber = serialNumber;
            ServiceProvisionCode = serviceProvisionCode;
            AgeOfSurvivorInYears = ageOfSurvivorInYears;
            SexOfSurvivorOrVictim = sexOfSurvivorOrVictim;
            TypeOfClient = typeOfClient;
            HasSurvivorReceivedServiceFromAnotherOrganisation = hasSurvivorReceivedServiceFromAnotherOrganisation;
            IncidentCodeFromReferringOrganisation = incidentCodeFromReferringOrganisation;
            TypeOfServiceReceivedAnotherOrganisation = typeOfServiceReceivedAnotherOrganisation == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceReceivedAnotherOrganisation);
            ReferralCode = referralCode;
            TypeOfServiceNeeded = typeOfServiceNeeded == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceNeeded);
            TypeOfServiceProvided = typeOfServiceProvided == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceProvided);
            TypeOfServiceReferredFor = typeOfServiceReferredFor == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceReferredFor);
            NameOfServiceProviderOrCsoReferredTo = nameOfServiceProviderOrCsoReferredTo;
            ReferralOutcome = referralOutcome;
            OrganisationId = organisationId;
            StateId = stateId;
            Longitude = longitude;
            Latitude = latitude;
            DateOfServiceProvision = dateOfServiceProvision;
            CreatedById = createdById;
            GbvCovid19Question1 = gbvCovid19Question1;
            GbvCovid19Question2 = gbvCovid19Question2;
            GbvCovid19Question3 = gbvCovid19Question3;
            GbvCovid19Question4 = gbvCovid19Question4;
            OrganisationLgaId = organisationLgaId;
            IncidentCode = incidentCode;
            ReferralToAnotherCsoOrSPcode = referralToAnotherCsoOrSPcode;
        }

        public int AgeOfSurvivorInYears { get; set; }

        [MaxLength(30)]
        public string IncidentCode { get; set; }

        [MaxLength(10)]
        public int SerialNumber { get; set; }

        [MaxLength(30)]
        public string ServiceProvisionCode { get; set; }

        [MaxLength(20)]
        public string SexOfSurvivorOrVictim { get; set; }

        [MaxLength(50)]
        public string TypeOfClient { get; set; }

        [MaxLength(15)]
        public YesOrNo HasSurvivorReceivedServiceFromAnotherOrganisation { get; set; }

        [MaxLength(30)]
        public string IncidentCodeFromReferringOrganisation { get; set; }

        [MaxLength(30)]
        public string ReferralCode { get; set; }

        //List<string>
        [MaxLength(1000)]
        public string TypeOfServiceReceivedAnotherOrganisation { get; set; }

        [NotMapped]
        public List<string> TypeOfServiceReceivedAnotherOrganisationList =>
            !string.IsNullOrWhiteSpace(TypeOfServiceReceivedAnotherOrganisation)
                ? JsonConvert.DeserializeObject<List<string>>(TypeOfServiceReceivedAnotherOrganisation)
                : new List<string>();

        //List<string>
        [MaxLength(1000)]
        public string TypeOfServiceNeeded { get; set; }

        [NotMapped]
        public List<string> TypeOfServiceNeededList =>
            !string.IsNullOrWhiteSpace(TypeOfServiceNeeded)
                ? JsonConvert.DeserializeObject<List<string>>(TypeOfServiceNeeded)
                : new List<string>();

        //List<string>
        [MaxLength(1000)]
        public string TypeOfServiceProvided { get; set; }

        [NotMapped]
        public List<string> TypeOfServiceProvidedList =>
            !string.IsNullOrWhiteSpace(TypeOfServiceProvided)
                ? JsonConvert.DeserializeObject<List<string>>(TypeOfServiceProvided)
                : new List<string>();

        //List<string>
        [MaxLength(1000)]
        public string TypeOfServiceReferredFor { get; set; }

        [NotMapped]
        public List<string> TypeOfServiceReferredForList =>
            !string.IsNullOrWhiteSpace(TypeOfServiceReferredFor)
                ? JsonConvert.DeserializeObject<List<string>>(TypeOfServiceReferredFor)
                : new List<string>();

        [MaxLength(200)]
        public string NameOfServiceProviderOrCsoReferredTo { get; set; }

        [MaxLength(50)]
        public string ReferralOutcome { get; set; }

        public int OrganisationId { get; set; }
        public Organisation Organisation { get; set; }

        public int StateId { get; set; }
        public State State { get; set; }

        [MaxLength(20)]
        public string Longitude { get; set; }

        [MaxLength(20)]
        public string Latitude { get; set; }

        [MaxLength(27)]
        public DateTime? DateOfServiceProvision { get; set; }

        [MaxLength(15)]
        public ValidationStatus Status { get; set; }

        //[MaxLength(36)]
        public string CreatedById { get; set; }

        public ApplicationUser CreatedBy { get; set; }

        //[MaxLength(36)]
        public string SpOrCsoApprovalById { get; set; }

        public ApplicationUser SpOrCsoApprovalBy { get; set; }

        [MaxLength(27)]
        public DateTime? SpOrCsoApprovalDate { get; set; }

        //[MaxLength(36)]
        public string StateApprovalById { get; set; }

        public ApplicationUser StateApprovalBy { get; set; }

        [MaxLength(27)]
        public DateTime? StateApprovalDate { get; set; }

        [MaxLength(250)]
        public string GbvCovid19Question1 { get; set; }

        [MaxLength(250)]
        public string GbvCovid19Question2 { get; set; }

        [MaxLength(250)]
        public string GbvCovid19Question3 { get; set; }

        [MaxLength(250)]
        public string GbvCovid19Question4 { get; set; }

        public int? OrganisationLgaId { get; set; }

        [MaxLength(50)]
        public string ReferralToAnotherCsoOrSPcode { get; set; }

        public void UpdateInfo(int ageOfSurvivorInYears,
            string sexOfSurvivorOrVictim,
            string typeOfClient,
            YesOrNo hasSurvivorReceivedServiceFromAnotherOrganisation,
            string incidentCodeFromReferringOrganisation,
            string referralCode,
            List<string> typeOfServiceReceivedAnotherOrganisation,
            List<string> typeOfServiceNeeded,
            List<string> typeOfServiceProvided,
            List<string> typeOfServiceReferredFor,
            string nameOfServiceProviderOrCsoReferredTo,
            string referralOutcome,
            string longitude,
            string latitude,
            DateTime? dateOfServiceProvision,
            string gbvCovid19Question1,
            string gbvCovid19Question2,
            string gbvCovid19Question3,
            string gbvCovid19Question4,
            int? organisationLgaId,
            string incidentCode,
            string referralToAnotherCsoOrSPcode)
        {
            AgeOfSurvivorInYears = ageOfSurvivorInYears;
            SexOfSurvivorOrVictim = sexOfSurvivorOrVictim;
            TypeOfClient = typeOfClient;
            HasSurvivorReceivedServiceFromAnotherOrganisation = hasSurvivorReceivedServiceFromAnotherOrganisation;
            IncidentCodeFromReferringOrganisation = incidentCodeFromReferringOrganisation;
            ReferralCode = referralCode;
            TypeOfServiceReceivedAnotherOrganisation = typeOfServiceReceivedAnotherOrganisation == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceReceivedAnotherOrganisation);
            TypeOfServiceNeeded = typeOfServiceNeeded == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceNeeded);
            TypeOfServiceProvided = typeOfServiceProvided == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceProvided);
            TypeOfServiceReferredFor = typeOfServiceReferredFor == null ? "[]" : JsonConvert.SerializeObject(typeOfServiceReferredFor);
            NameOfServiceProviderOrCsoReferredTo = nameOfServiceProviderOrCsoReferredTo;
            ReferralOutcome = referralOutcome;
            Longitude = longitude;
            Latitude = latitude;
            DateOfServiceProvision = dateOfServiceProvision;
            GbvCovid19Question1 = gbvCovid19Question1;
            GbvCovid19Question2 = gbvCovid19Question2;
            GbvCovid19Question3 = gbvCovid19Question3;
            GbvCovid19Question4 = gbvCovid19Question4;
            OrganisationLgaId = organisationLgaId;
            IncidentCode = incidentCode;
            ReferralToAnotherCsoOrSPcode = referralToAnotherCsoOrSPcode;
        }

        public void Approve(ValidationStatus status, string approvedById)
        {
            if (status == ValidationStatus.State)
            {
                StateApprovalById = approvedById;
                StateApprovalDate = DateTime.Now;
            }
            else if (status == ValidationStatus.SpOrCso)
            {
                SpOrCsoApprovalById = approvedById;
                SpOrCsoApprovalDate = DateTime.Now;
            }
            else if (status == ValidationStatus.StateRejected)
            {
                StateApprovalById = approvedById;
                StateApprovalDate = DateTime.Now;
            }
            else if (status == ValidationStatus.SpOrCsoRejected)
            {
                SpOrCsoApprovalById = approvedById;
                SpOrCsoApprovalDate = DateTime.Now;
            }

            Status = status;

        }

        //public void Reject(ValidationStatus status, string rejectedById)
        //{
        //    if (status == ValidationStatus.StateRejected)
        //    {
        //        StateApprovalById = rejectedById;
        //        StateApprovalDate = DateTime.Now;
        //    }
        //    else if (status == ValidationStatus.SpOrCsoRejected)
        //    {
        //        SpOrCsoApprovalById = rejectedById;
        //        SpOrCsoApprovalDate = DateTime.Now;
        //    }

        //    Status = status;
        //}

        public void UndoApproval()
        {
            if (Status != ValidationStatus.Submitted)
            {
                Status = (ValidationStatus)((int)Status) - 2;
            }
        }
    }

    public enum ValidationStatus
    {
        [EnumMember(Value = "Submitted")]
        Submitted,

        [EnumMember(Value = "CSO/SP Validated")]
        SpOrCso,

        [EnumMember(Value = "CSO/SP Rejected")]
        SpOrCsoRejected,

        [EnumMember(Value = "State Validated")]
        State,

        [EnumMember(Value = "State Rejected")]
        StateRejected
    }
}