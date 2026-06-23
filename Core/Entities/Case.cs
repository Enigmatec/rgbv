using Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Case : BaseEnity<int>
    {
        public Case()
        {
        }

        public Case(string incidentCode,
            int serialNumber,
            string contactChannel,
            string whoReportedIncident,
            int ageOfSurvior,
            string sexOfSurvior,
            string maritalStatus,
            string employmentStatus,
            string employmentStatusOfParentOrGuardian,
            string vulnerablePopulation,
            string education,
            YesOrNo doesSurviorLiveAlone,
            string whoDoesSurviorLiveWith,
            string actualLocationOfIncident,
            DateTime dateOfIncident,
            DateTime dateReported,
            TimeOfDay timeOfDay,
            YesOrNo hasSurviorReceivedService,
            YesOrNo wasViolenceFatal,
            string typeOfServiceReceivedBySurvior,
            string typeOfServiceProvidedToSurvior,
            string actualReferralServiceReceived,
            string nameOfServiceProviderReferredTo,
            string outcomeOfServiceorReferral,
            string receivingOrganisationCode,
            List<PerpetratorsInformationModel> perpetratorsInformation,
            YesOrNo isSurviorContinuousThreat,
            string numberOfPerpetrators,
            YesOrNo doestheSurviorWantJustice,
            string gbvCovid19Question1,
            string gbvCovid19Question2,
            string gbvCovid19Question3,
            string gbvCovid19Question4,
            YesOrNo hasCaseBeenClosed,
            //int caseCategoryId,
            List<CaseCategoryOrTypeOfViolence> caseCategories,
            string caseCategoriesOthers,
            int incidentStateId,
            int incidentLgaId,
            int? incidentWardId,
            string createdById,
            int organisationId,
            string whoClosedTheCase,
            string longitude,
            string latitude,
            string contactChannelOrganisation,
            string contactChannelOrganisationService,
            string otherServiceProviderName,
            string otherServiceProviderAddress,
            string otherServiceProviderIncidentCode,
            string contactChannelOrganisationIncidentCode,
            DateTime? caseClosedDate,
            //string surviorDoesNotWantJusticeReasons,
            string referralOutcome,
            string survivorEstimatedAverageMonthlyIncome,
            string outcomeOfProsecution,
            int organisationLgaId, string survivorMobileNo,
            //string survivorName,
            DateTime? dateJusticeReceived)
        {
            IncidentCode = incidentCode;
            SerialNumber = serialNumber;
            ContactChannel = contactChannel;
            WhoReportedIncident = whoReportedIncident;
            AgeOfSurvior = ageOfSurvior;
            SexOfSurvior = sexOfSurvior;
            MaritalStatus = maritalStatus;
            EmploymentStatus = employmentStatus;
            EmploymentStatusOfParentOrGuardian = employmentStatusOfParentOrGuardian;
            VulnerablePopulation = vulnerablePopulation;
            Education = education;
            DoesSurviorLiveAlone = doesSurviorLiveAlone;
            WhoDoesSurviorLiveWith = whoDoesSurviorLiveWith;
            ActualLocationOfIncident = actualLocationOfIncident;
            DateOfIncident = dateOfIncident;
            DateReported = dateReported;
            TimeOfDay = timeOfDay;
            HasSurviorReceivedService = hasSurviorReceivedService;
            WasViolenceFatal = wasViolenceFatal;
            TypeOfServiceReceivedBySurvior = typeOfServiceReceivedBySurvior;
            TypeOfServiceProvidedToSurvior = typeOfServiceProvidedToSurvior;
            ActualReferralServiceReceived = actualReferralServiceReceived;
            NameOfServiceProviderReferredTo = nameOfServiceProviderReferredTo;
            OutcomeOfServiceorReferral = outcomeOfServiceorReferral;
            ReceivingOrganisationCode = receivingOrganisationCode;
            PerpetratorsInformation = perpetratorsInformation is null ? "[]" : JsonConvert.SerializeObject(perpetratorsInformation);
            IsSurviorContinuousThreat = isSurviorContinuousThreat;
            NumberOfPerpetrators = numberOfPerpetrators;
            DoestheSurviorWantJustice = doestheSurviorWantJustice;
            GBV_COVID19_Question1 = gbvCovid19Question1;
            GBV_COVID19_Question2 = gbvCovid19Question2;
            GBV_COVID19_Question3 = gbvCovid19Question3;
            GBV_COVID19_Question4 = gbvCovid19Question4;
            HasCaseBeenClosed = hasCaseBeenClosed;
            CanBeEdited = true;
            //CaseCategoryId = caseCategoryId;
            CaseCategories = JsonConvert.SerializeObject(caseCategories.ConvertAll(c => c.ToString()));
            CaseCategoriesOthers = caseCategoriesOthers;
            IncidentStateId = incidentStateId;
            IncidentLGAId = incidentLgaId;
            IncidentWardId = incidentWardId;
            CreatedById = createdById;
            OrganisationId = organisationId;
            WhoClosedTheCase = whoClosedTheCase;
            Longitude = longitude;
            Latitude = latitude;
            ContactChannelOrganisation = contactChannelOrganisation;
            ContactChannelOrganisationService = contactChannelOrganisationService;
            OtherServiceProviderName = otherServiceProviderName;
            OtherServiceProviderAddress = otherServiceProviderAddress;
            OtherServiceProviderIncidentCode = otherServiceProviderIncidentCode;
            ContactChannelOrganisationIncidentCode = contactChannelOrganisationIncidentCode;
            CaseClosedDate = caseClosedDate;
            ReferralOutcome = referralOutcome;
            SurvivorEstimatedAverageMonthlyIncome = survivorEstimatedAverageMonthlyIncome;
            OutcomeOfProsecution = outcomeOfProsecution;
            OrganisationLgaId = organisationLgaId;
            SurvivorMobileNo = survivorMobileNo;
            //SurvivorName = survivorName;
            DateJusticeReceived = dateJusticeReceived;
        }

        [MaxLength(15)]
        public string IncidentCode { get; set; }

        [MaxLength(10)]
        public int SerialNumber { get; set; }

        [MaxLength(100)]
        public string ContactChannel { get; set; }

        [MaxLength(100)]
        public string WhoReportedIncident { get; set; }

        [MaxLength(4)]
        public int AgeOfSurvior { get; set; }

        [MaxLength(20)]
        public string SexOfSurvior { get; set; }

        [MaxLength(20)]
        public string MaritalStatus { get; set; }

        [MaxLength(20)]
        public string EmploymentStatus { get; set; }

        [MaxLength(20)]
        public string EmploymentStatusOfParentOrGuardian { get; set; }

        [MaxLength(200)]
        public string VulnerablePopulation { get; set; }

        [MaxLength(20)]
        public string Education { get; set; }

        [MaxLength(15)]
        public YesOrNo DoesSurviorLiveAlone { get; set; }

        [MaxLength(40)]
        public string WhoDoesSurviorLiveWith { get; set; }

        [MaxLength(50)]
        public string ActualLocationOfIncident { get; set; }

        [MaxLength(27)]
        public DateTime DateOfIncident { get; set; }

        [MaxLength(27)]
        public DateTime DateReported { get; set; }

        [MaxLength(15)]
        public TimeOfDay TimeOfDay { get; set; }

        [MaxLength(15)]
        public YesOrNo HasSurviorReceivedService { get; set; }

        [MaxLength(15)]
        public YesOrNo WasViolenceFatal { get; set; }

        [MaxLength(100)]
        public string FollowUpActionByCSO { get; set; } //serilized list.

        [MaxLength(200)]
        public string TypeOfServiceReceivedBySurvior { get; set; } // serilaized list.

        [MaxLength(10)]
        public string TypeOfReferralServiceRequired { get; set; } //serilaized list

        [MaxLength(2000)]
        public string TypeOfServiceProvidedToSurvior { get; set; } //Serilaized list *

        [MaxLength(200)]
        public string ActualReferralServiceReceived { get; set; } //serilaized list

        [MaxLength(200)]
        public string NameOfServiceProviderReferredTo { get; set; }

        [MaxLength(300)]
        public string OutcomeOfServiceorReferral { get; set; }

        [MaxLength(50)]
        public string ReceivingOrganisationCode { get; set; }

        [MaxLength(500)] //calculated 100 *5
        public string PerpetratorsInformation { get; set; }

        [NotMapped]
        public List<PerpetratorsInformationModel> PerpetratorsInformationList =>
            string.IsNullOrWhiteSpace(PerpetratorsInformation)
                ? new List<PerpetratorsInformationModel>() : JsonConvert.DeserializeObject<List<PerpetratorsInformationModel>>(
                    PerpetratorsInformation);

        [MaxLength(15)]
        public YesOrNo IsSurviorContinuousThreat { get; set; }

        [MaxLength(10)]
        public string NumberOfPerpetrators { get; set; }

        [MaxLength(15)]
        public YesOrNo DoestheSurviorWantJustice { get; set; }

        [MaxLength(600)]
        public string GBV_COVID19_Question1 { get; set; }

        [MaxLength(600)]
        public string GBV_COVID19_Question2 { get; set; }

        [MaxLength(2000)]
        public string GBV_COVID19_Question3 { get; set; }

        [MaxLength(700)]
        public string GBV_COVID19_Question4 { get; set; }

        [MaxLength(13)]
        public YesOrNo HasCaseBeenClosed { get; set; }

        public bool CanBeEdited { get; set; }

        /// <summary>
        /// approval by state
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Validation by Organisation
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Validation by lga
        /// </summary>
        public bool LgaValidated { get; set; } = true; //todo: is set to true temporarily bcos lga validation has not been fully implemented

        //[MaxLength(36)]
        /// <summary> State Administrator & State Supervisor
        ///
        /// if the case has approved by id and isApproved is false then the case was rejected </summary>
        public string ApprovedById { get; set; }

        [MaxLength(27)]
        public DateTime? ApprovedAt { get; set; }

        [Description($"Serialized list of {nameof(CaseCategoryOrTypeOfViolence)}")]
        [MaxLength(400)]
        public string CaseCategories { get; set; }

        [NotMapped]
        public List<int> CaseCategoriesList => !string.IsNullOrWhiteSpace(CaseCategories) ?
            JsonConvert.DeserializeObject<List<CaseCategoryOrTypeOfViolence>>(CaseCategories).ConvertAll(v => (int)v)
            : new List<int>();

        [MaxLength(350)]
        public string CaseCategoriesOthers { get; set; }

        public int? CaseCategoryId { get; set; }

        public int IncidentStateId { get; set; }
        public int IncidentLGAId { get; set; }
        public int? IncidentWardId { get; set; }
        public string CreatedById { get; set; }
        public int OrganisationId { get; set; }

        //[MaxLength(36)]
        public string LgaValidatedById { get; set; }

        [MaxLength(27)]
        public DateTime? LgaValidatedAt { get; set; }

        public ApplicationUser LgaValidatedBy { get; set; }

        //[MaxLength(36)]
        /// <summary> CSO Supervisor & Service Provider Supervisor
        ///
        /// if the case has validated by id and isValidated is false then the case was rejected </summary>
        public string ValidatedById { get; set; }

        [MaxLength(27)]
        public DateTime? ValidatedAt { get; set; }

        [MaxLength(36)]
        public string WhoClosedTheCase { get; set; }

        [MaxLength(20)]
        public string Longitude { get; set; }

        [MaxLength(20)]
        public string Latitude { get; set; }

        public ApplicationUser CreatedBy { get; set; }

        public ApplicationUser ApprovedBy { get; set; }

        public ApplicationUser ValidatedBy { get; set; }

        public CaseCategory Category { get; set; }

        public LocalGovernmentArea IncidentLGA { get; set; }

        public Ward IncidentWard { get; set; }

        public State IncidentState { get; set; }
        public Organisation Organisation { get; set; }

        public ICollection<FollowUp> FollowUpActions { get; set; } = new HashSet<FollowUp>();

        [MaxLength(100)]
        public string ContactChannelOrganisation { get; set; }

        [MaxLength(350)]
        public string ContactChannelOrganisationService { get; set; }

        [MaxLength(100)]
        public string OtherServiceProviderName { get; set; }

        [MaxLength(100)]
        public string OtherServiceProviderAddress { get; set; }

        [MaxLength(30)]
        public string OtherServiceProviderIncidentCode { get; set; }

        [MaxLength(15)]
        public string ContactChannelOrganisationIncidentCode { get; set; }

        [MaxLength(27)]
        public DateTime? CaseClosedDate { get; set; }

        [MaxLength(50)]
        public string SurviorDoesNotWantJusticeReasons { get; set; }

        [MaxLength(50)]
        public string ReferralOutcome { get; set; }

        [MaxLength(30)]
        public string SurvivorEstimatedAverageMonthlyIncome { get; set; }

        [MaxLength(36)]
        public string OutcomeOfProsecution { get; set; }

        public int? OrganisationLgaId { get; set; }

        [MaxLength(15)]
        [Phone]
        public string SurvivorMobileNo { get; set; }

        //[MaxLength(35)]
        //public string SurvivorName { get; set; }

        [MaxLength(27)]
        public DateTime? DateJusticeReceived { get; set; }

        /// <summary>
        /// list of <see cref="CaseNotes"/>
        /// </summary>
        [MaxLength(2000)]
        public string Notes { get; set; }

        [NotMapped]
        public List<CaseNotes> NotesList => !string.IsNullOrWhiteSpace(Notes)
            ? JsonConvert.DeserializeObject<List<CaseNotes>>(Notes) : new List<CaseNotes>();

        public string Stage
        {
            get
            {
                if (IsValidated)
                {
                    if (IsApproved)
                    {
                        return "State Validated";
                    }
                    else
                    {
                        if (ApprovedById != null)
                        {
                            return "State Rejected";
                        }
                    }

                    return "CSO/SP Validated";
                }
                else
                {
                    if (ValidatedById != null)
                    {
                        return "CSO/SP rejected";
                    }

                    return "Submitted";
                }
            }
        }
    }

    public class PerpetratorsInformationModel
    {
        public string SexOfPerpetrator { get; set; }

        public int AgeOfPerpetrator { get; set; }

        public string SurviorRelationWithPerpetrator { get; set; }
    }

    public class FollowUp : BaseEnity<int>
    {
        public int CaseId { get; set; }
        public YesOrNo HasClientReceivedjustice { get; set; }

        public DateTime? JusticeReceivedDate { get; set; }
        public string FinalOutcome { get; set; }

        public YesOrNo HasCaseBeenClosed { get; set; }

        public string WhoClosedTheCase { get; set; }

        public DateTime? CaseClosedDate { get; set; }

        public string CreatedById { get; set; }

        public Case Case { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public class CaseNotes
    {
        public CaseNotes(string userId, string name, string note, DateTime date)
        {
            UserId = userId;
            Name = name;
            Note = note;
            Date = date;
        }

        public string UserId { get; }

        public string Name { get; }

        public string Note { get; }
        public DateTime Date { get; }
    }
}