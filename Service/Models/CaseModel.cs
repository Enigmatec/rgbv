using Core.Entities;
using Core.Enums;
using Service.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Service.Models
{
    public class CaseCategoryCreationModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }

    public class CaseCategoryListModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class CaseCategoryViewModel : CaseCategoryCreationModel
    {
        public int Id { get; set; }
        public int Cases { get; set; }
    }

    //public class CaseCreationCSOModel
    //{
    //    [Required]
    //    public string UserId { get; set; }

    // [Required] [ValueCannotBeZero] public int OrganisationId { get; set; }

    // [Required] public int CaseCategoryId { get; set; }

    // public string OtherCategory { get; set; }

    // [Required] [ValueCannotBeZero] public int IncidentStateId { get; set; }

    // [Required] [ValueCannotBeZero] public int IncidentLGAId { get; set; }

    // [Required] public string WhoReportedIncident { get; set; }

    // [ValueCannotBeZero] public int AgeOfSurvior { get; set; }

    // public int? IncidentWardId { get; set; } public string SexOfSurvior { get; set; }

    // public DateTime DateOfIncident { get; set; }

    // public DateTime DateReported { get; set; }

    // public TimeOfDay TimeOfDay { get; set; }

    // public YesOrNo HasSurviorReceivedService { get; set; }

    // public YesOrNo WasViolenceFatal { get; set; } public List<string>
    // TypeOfServiceReceivedBySurvior { get; set; }

    // public List<string> FollowUpActionByCSO { get; set; }

    // public List<string> TypeOfReferralServiceRequired { get; set; }

    // public List<string> ActualReferralServiceReceived { get; set; } public string
    // NameOfServiceProviderReferredTo { get; set; }

    // public string ReferralOutcome { get; set; }

    // public string ReceivingOrganisationCode { get; set; }

    // public string GBV_COVID19_Question1 { get; set; }

    // public string GBV_COVID19_Question2 { get; set; }

    //    public string GBV_COVID19_Question4 { get; set; }
    //}

    public class CaseCreationSPModel
    {
        [Required]
        public string UserId { get; set; }

        public string ContactChannel { get; set; }

        public string MaritalStatus { get; set; }

        public string EmploymentStatus { get; set; }

        public string EmploymentStatusOfParentOrGuardian { get; set; }

        public List<string> VulnerablePopulation { get; set; }

        public string Education { get; set; }

        public YesOrNo? DoesSurviorLiveAlone { get; set; }

        public string WhoDoesSurviorLiveWith { get; set; }

        public string ActualLocationOfIncident { get; set; }

        [Required]
        [ValueCannotBeZero]
        public int OrganisationId { get; set; }

        //[Required]
        //public int CaseCategoryId { get; set; }

        [Required]
        public List<CaseCategoryOrTypeOfViolence> CaseCategories { get; set; }

        public string CaseCategoriesOther { get; set; }

        public string OtherCategory { get; set; }

        [Required]
        [ValueCannotBeZero]
        public int IncidentStateId { get; set; }

        [Required]
        [ValueCannotBeZero]
        public int IncidentLGAId { get; set; }

        public int? IncidentWardId { get; set; }

        [Required]
        public string WhoReportedIncident { get; set; }

        //[ValueCannotBeZero]
        public int AgeOfSurvior { get; set; }

        public string SexOfSurvior { get; set; }

        public DateTime DateOfIncident { get; set; }

        public DateTime DateReported { get; set; }

        public TimeOfDay TimeOfDay { get; set; }
        public YesOrNo? WasViolenceFatal { get; set; }
        public YesOrNo? HasSurviorReceivedService { get; set; }

        public List<string> TypeOfServiceReceivedBySurvior { get; set; }

        public List<PerpetratorsInformationModel> PerpetratorsInformation { get; set; }

        public YesOrNo? IsSurviorContinuousThreat { get; set; }

        public string NumberOfPerpetrators { get; set; }
        public YesOrNo? DoestheSurviorWantJustice { get; set; }

        public List<string> TypeOfServiceProvidedToSurvior { get; set; }

        public string OutcomeOfSerivce { get; set; }

        public string ReceivingOrganisationCode { get; set; }

        public string NameOfServiceProviderReferredTo { get; set; }

        public List<string> ActualReferralServiceReceived { get; set; }

        public YesOrNo? HasCaseBeenClosed { get; set; }
        public string WhoClosedTheCase { get; set; }
        public string GBV_COVID19_Question1 { get; set; }

        public string GBV_COVID19_Question2 { get; set; }

        public string GBV_COVID19_Question3 { get; set; }

        public string GBV_COVID19_Question4 { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public string ContactChannelOrganisation { get; set; }
        public List<string> ContactChannelOrganisationService { get; set; }

        public string OtherServiceProviderName { get; set; }

        public string OtherServiceProviderAddress { get; set; }

        public string OtherServiceProviderIncidentCode { get; set; }

        public string ContactChannelOrganisationIncidentCode { get; set; }

        public DateTime? CaseClosedDate { get; set; }

        public List<string> SurviorDoesNotWantJusticeReasons { get; set; }

        public string ReferralOutcome { get; set; }

        public string SurvivorEstimatedAverageMonthlyIncome { get; set; }

        public string OutcomeOfProsecution { get; set; }

        [Required]
        [ValueCannotBeZero]
        public int OrganisationLgaId { get; set; }

        public string SurvivorMobileNo { get; set; }

        //public string SurvivorName { get; set; }

        public DateTime? DateJusticeReceived { get; set; }
    }

    public class EntryModel
    {
        public string Value { get; set; }

        public Field Field { get; set; }

        public EntryType Type { get; set; }
    }

    public enum SortByDate
    {
        DateReported,
        DateSubmitted,
        DateOfIncident,
        DateModified
    }

    public class CaseIncidentCode
    {
        public string IncidentCode { get; set; }
    }
    public class CaseSearchModel : PaginationModel
    {
        public int? OrganisationId { get; set; }
        public OrganisationType? OrganisationType { get; set; }
        public string Organisation { get; set; }
        public int? CaseCategoryId { get; set; }

        public TimeOfDay? TimeOfDay { get; set; }

        public int? StateId { get; set; }

        public SortByDate? SortByDate { get; set; }

        public int? IncidentLGAId { get; set; }

        public YesOrNo? IsCaseClosed { get; set; }
        public string IncidentCode { get; set; }

        public bool? IsApproved { get; set; }

        public bool? IsValidated { get; set; }

        public bool? IsLgaValidated { get; set; }

        public string ValidateByRole { get; set; }

        public string Gender { get; set; }
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public string VulnerablePopulation { get; set; }

        public string TypeOfServiceProvided { get; set; }

        public string CurrentUserStateId { get; set; }

        public string CurrentUserOrganisationId { get; set; }

        public bool? IsRejected { get; set; }

        public ValidationStatus? ValidationStatus { get; set; }
    }

    public class CaseViewBackground
    {
        public UserViewModel User { get; set; }
        public List<CaseViewModel> Cases { get; set; }
    }

    public class CaseViewModel
    {
        public int Id { get; set; }
        public string IncidentCode { get; set; }
        public int SerialNumber { get; set; }
        public string ContactChannel { get; set; }
        public string WhoReportedIncident { get; set; }

        public int AgeOfSurvior { get; set; }

        public string SexOfSurvior { get; set; }

        public string MaritalStatus { get; set; }

        public string EmploymentStatus { get; set; }

        public string EmploymentStatusOfParentOrGuardian { get; set; }

        public List<string> VulnerablePopulation { get; set; }

        public string Education { get; set; }

        public YesOrNo DoesSurviorLiveAlone { get; set; }

        public string WhoDoesSurviorLiveWith { get; set; }

        public string ActualLocationOfIncident { get; set; }
        public DateTime DateOfIncident { get; set; }

        public DateTime DateReported { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public YesOrNo HasSurviorReceivedService { get; set; }

        public YesOrNo WasViolenceFatal { get; set; }

        public List<string> FollowUpActionByCSO { get; set; }

        public List<string> TypeOfServiceReceivedBySurvior { get; set; } // serilaized list.
        public List<string> TypeOfReferralServiceRequired { get; set; } //serilaized list
        public List<string> TypeOfServiceProvidedToSurvior { get; set; } //Serilaized list
        public List<string> ActualReferralServiceReceived { get; set; } //serilaized list

        public string NameOfServiceProviderReferredTo { get; set; }

        public string OutcomeOfServiceorReferral { get; set; }

        public YesOrNo IsSurviorContinuousThreat { get; set; }

        public string NumberOfPerpetrators { get; set; }
        public YesOrNo DoestheSurviorWantJustice { get; set; }

        public string GBV_COVID19_Question1 { get; set; }

        public string GBV_COVID19_Question2 { get; set; }

        public string GBV_COVID19_Question3 { get; set; }

        public string GBV_COVID19_Question4 { get; set; }

        public YesOrNo HasCaseBeenClosed { get; set; }

        public bool CanBeEdited { get; set; }
        //public int CaseCategoryId { get; set; }

        public string CaseCategoriesOthers { get; set; }

        public List<int> CaseCategories { get; set; }

        public int IncidentStateId { get; set; }
        public int IncidentLGAId { get; set; }
        public int? IncidentWardId { get; set; }
        public string CreatedById { get; set; }

        public int OrganisationId { get; set; }
        public OrganisationType OrganisationType { get; set; }
        public string Category { get; set; }
        public string Organisation { get; set; }
        public string OrganisationLGA { get; set; }
        public string IncidentState { get; set; }
        public string IncidentLGA { get; set; }
        public string IncidentWard { get; set; }

        public string CreatedByName { get; set; }

        public bool IsApproved { get; set; }
        public string ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovedByName { get; set; }
        public bool IsValidated { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public string ValidatedById { get; set; }
        public string ValidatedByName { get; set; }

        public string WhoClosedTheCase { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public string UserState { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public string ContactChannelOrganisation { get; set; }
        public List<string> ContactChannelOrganisationService { get; set; }

        public string OtherServiceProviderName { get; set; }

        public string OtherServiceProviderAddress { get; set; }

        public string OtherServiceProviderIncidentCode { get; set; }

        public string ContactChannelOrganisationIncidentCode { get; set; }

        public DateTime? CaseClosedDate { get; set; }

        public List<string> SurviorDoesNotWantJusticeReasons { get; set; }

        public string ReferralOutcome { get; set; }

        public string ReceivingOrganisationCode { get; set; }

        public IEnumerable<FollowUpViewModel> FollowUps { get; set; }

        public DateTime? LgaValidatedAt { get; set; }
        public string LgaValidatedByName { get; set; }

        public List<PerpetratorsInformationModel> PerpetratorsInformationList { get; set; }

        public string SurvivorEstimatedAverageMonthlyIncome { get; set; }

        public string OutcomeOfProsecution { get; set; }

        public int? OrganisationLgaId { get; set; }

        public List<CaseNotes> NotesList { get; set; }

        public string Stage { get; set; }

        public string SurvivorMobileNo { get; set; }

        public string SurvivorName { get; set; }

        public DateTime? DateJusticeReceived { get; set; }
    }



    public class CaseViewModelExport
    {
        public int Id { get; set; }
        public string IncidentCode { get; set; }
        public int SerialNumber { get; set; }
        public string ContactChannel { get; set; }
        public string WhoReportedIncident { get; set; }

        public int AgeOfSurvior { get; set; }

        public string SexOfSurvior { get; set; }

        public string MaritalStatus { get; set; }

        public string EmploymentStatus { get; set; }

        public string EmploymentStatusOfParentOrGuardian { get; set; }

        public string VulnerablePopulation { get; set; }

        public string Education { get; set; }

        public YesOrNo DoesSurviorLiveAlone { get; set; }

        public string WhoDoesSurviorLiveWith { get; set; }

        public string ActualLocationOfIncident { get; set; }
        public DateTime DateOfIncident { get; set; }

        public DateTime DateReported { get; set; }

        public TimeOfDay TimeOfDay { get; set; }

        public YesOrNo HasSurviorReceivedService { get; set; }

        public YesOrNo WasViolenceFatal { get; set; }

        public List<string> FollowUpActionByCSO { get; set; }

        public List<string> TypeOfServiceReceivedBySurvior { get; set; } // serilaized list.
        public List<string> TypeOfReferralServiceRequired { get; set; } //serilaized list
        public List<string> TypeOfServiceProvidedToSurvior { get; set; } //Serilaized list
        public List<string> ActualReferralServiceReceived { get; set; } //serilaized list

        public string NameOfServiceProviderReferredTo { get; set; }

        public string OutcomeOfServiceorReferral { get; set; }

        public YesOrNo IsSurviorContinuousThreat { get; set; }

        public string NumberOfPerpetrators { get; set; }
        public YesOrNo DoestheSurviorWantJustice { get; set; }

        public string GBV_COVID19_Question1 { get; set; }

        public string GBV_COVID19_Question2 { get; set; }

        public string GBV_COVID19_Question3 { get; set; }

        public string GBV_COVID19_Question4 { get; set; }

        public YesOrNo HasCaseBeenClosed { get; set; }

        public bool CanBeEdited { get; set; }
        //public int CaseCategoryId { get; set; }

        public string CaseCategoriesOthers { get; set; }

        public List<int> CaseCategories { get; set; }

        public int IncidentStateId { get; set; }
        public int IncidentLGAId { get; set; }
        public int? IncidentWardId { get; set; }
        public string CreatedById { get; set; }

        public int OrganisationId { get; set; }
        public OrganisationType OrganisationType { get; set; }
        public string Category { get; set; }
        public string Organisation { get; set; }
        public string IncidentState { get; set; }
        public string IncidentLGA { get; set; }
        public string IncidentWard { get; set; }

        public string CreatedByName { get; set; }

        public bool IsApproved { get; set; }
        public string ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovedByName { get; set; }
        public bool IsValidated { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public string ValidatedById { get; set; }
        public string ValidatedByName { get; set; }

        public string WhoClosedTheCase { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public string UserState { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public string ContactChannelOrganisation { get; set; }
        public List<string> ContactChannelOrganisationService { get; set; }

        public string OtherServiceProviderName { get; set; }

        public string OtherServiceProviderAddress { get; set; }

        public string OtherServiceProviderIncidentCode { get; set; }

        public string ContactChannelOrganisationIncidentCode { get; set; }

        public DateTime? CaseClosedDate { get; set; }

        public List<string> SurviorDoesNotWantJusticeReasons { get; set; }

        public string ReferralOutcome { get; set; }

        public string ReceivingOrganisationCode { get; set; }

        public IEnumerable<FollowUpViewModel> FollowUps { get; set; }

        public DateTime? LgaValidatedAt { get; set; }
        public string LgaValidatedByName { get; set; }

        public List<PerpetratorsInformationModel> PerpetratorsInformationList { get; set; }

        public string SurvivorEstimatedAverageMonthlyIncome { get; set; }

        public string OutcomeOfProsecution { get; set; }

        public int? OrganisationLgaId { get; set; }

        public List<CaseNotes> NotesList { get; set; }

        public string Stage { get; set; }

        public string SurvivorMobileNo { get; set; }

        public string SurvivorName { get; set; }

        public DateTime? DateJusticeReceived { get; set; }
    }

    public class ApproveCase
    {
        [Required]
        [ListCannotBeEmpty]
        public List<int> Ids { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public bool? IsLga { get; set; }
    }

    public class FollowUpCreationModel
    {
        [Required]
        public YesOrNo HasClientReceivedjustice { get; set; }

        public DateTime? JusticeReceivedDate { get; set; }

        [Required]
        public string FinalOutcome { get; set; }

        [Required]
        public YesOrNo HasCaseBeenClosed { get; set; }

        public string WhoClosedTheCase { get; set; }

        public DateTime CaseClosedDate { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public class FollowUpViewModel : FollowUpCreationModel
    {
        public int CaseId { get; set; }

        public int Id { get; set; }

        public string IncidentCode { get; set; }
        public string CreatedById { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CaseSummaryModel
    {
        public int OrganisationId { get; set; }
        public string Organisation { get; set; }
        public OrganisationType OrganisationType { get; set; }

        public int StateId { get; set; }

        public string State { get; set; }

        public int CaseCategoryId { get; set; }

        public string CaseCategory { get; set; }

        public string StateReported { get; set; }
    }

    public class DateModel
    {
        //[ValueCannotBeZero]
        //public int Year { get; set; }

        //[ValueCannotBeZero]
        //public int Month { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsApproved { get; set; }

        public bool? IsValidated { get; set; }

        public int? StateId { get; set; }
    }

    public class CaseReportModel
    {
        public List<CaseBySubject> CaseByOrganisation { get; set; }
        public List<CaseBySubject> CaseByState { get; set; }
        public List<CaseBySubject> CaseByCategory { get; set; }
        public List<CaseBySubject> CaseByStateReported { get; set; }
    }

    public class DataByIds
    {
        public List<int> Ids { get; set; }

    }

    public class CaseEmailModel
    {
        public byte[] Stream { get; set; }
        public string Filename { get; set; }
    }

}