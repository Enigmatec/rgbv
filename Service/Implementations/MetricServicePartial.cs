using ClosedXML.Excel;
using Core.Entities;
using Core.Enums;
using Curiosity.SPSS.DataReader;
using Curiosity.SPSS.SpssDataset;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Service.Extensions;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Service.Implementations
{
    public partial class MetricsService
    {
        /// <summary>
        /// Creates the EXCEL code dictionary with the code dictionay passed in
        /// </summary>
        /// <param name="CodeDictionary"></param>
        /// <returns></returns>
        private async Task<ExcelPackage> GetCodeDictionaryExcelSheet(Dictionary<string, Dictionary<int, string>> CodeDictionary)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new ExcelPackage();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add("Code Dictionary");

            var row = 1;

            WorkSheet.Cells[$"A{row}"].Value = "Entry";
            WorkSheet.Cells[$"B{row}"].Value = "Value";
            WorkSheet.Cells[$"C{row}"].Value = "Key";

            WorkSheet.Cells[$"A1:C{row}"].Style.Font.Bold = true;

            foreach (var Dictionary in CodeDictionary)
            {
                row++;
                WorkSheet.Cells[$"A{row}"].Value = Dictionary.Key;

                foreach (var item in Dictionary.Value)
                {
                    WorkSheet.Cells[$"B{row}"].Value = item.Value;
                    WorkSheet.Cells[$"C{row}"].Value = item.Key;
                    row++;
                }
            }

            await Excelpackage.SaveAsync();

            return Excelpackage;
        }

        /// <summary>
        /// Creates the Excel sheet with data of cases either in coded or uncoded format
        /// </summary>
        /// <param name="model">the case model</param>
        /// <param name="IsFullText">check if data is coded or uncoded</param>
        /// <returns>Excel package</returns>
        private async Task<ExcelPackage> GetCaseExcelSheet(IEnumerable<CaseViewModel> model, bool IsFullText)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new ExcelPackage();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add($"Incident Report_{DateTime.Now:dd-MMM-yy}");

            var row = 1;
            var orgInfoHeader = WorkSheet.Cells[$"E{row}:M{row}"];
            orgInfoHeader.Value = "ORGANISATION'S INFORMATION";
            orgInfoHeader.Merge = true;
            orgInfoHeader.Style.Font.Bold = true;
            orgInfoHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var survInfo = WorkSheet.Cells[$"N{row}:AN{row}"];
            survInfo.Value = "SURVIVOR/VICTIM'S INFORMATION";
            survInfo.Merge = true;
            survInfo.Style.Font.Bold = true;
            survInfo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var caseDesc = WorkSheet.Cells[$"AO{row}:BI{row}"];
            caseDesc.Value = "CASE DESCRIPTION";
            caseDesc.Merge = true;
            caseDesc.Style.Font.Bold = true;
            caseDesc.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var apInfo = WorkSheet.Cells[$"BJ{row}:BP{row}"];
            apInfo.Value = "ALLEGED PERPETRATOR'S INFORMATION";
            apInfo.Merge = true;
            apInfo.Style.Font.Bold = true;
            apInfo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //var actionTaken = WorkSheet.Cells[$"BI{row}:CN{row}"];
            //actionTaken.Value = "ACTION TAKEN";
            //actionTaken.Merge = true;
            //actionTaken.Style.Font.Bold = true;
            //actionTaken.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //var co19 = WorkSheet.Cells[$"CO{row}:CR{row}"];
            //co19.Value = "GBV-COVID 19";
            //co19.Merge = true;
            //co19.Style.Font.Bold = true;
            //co19.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var finOut = WorkSheet.Cells[$"BQ{row}:BV{row}"];
            finOut.Value = "FINAL OUTCOME INFORMATION";
            finOut.Merge = true;
            finOut.Style.Font.Bold = true;
            finOut.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var valid = WorkSheet.Cells[$"BW{row}:CB{row}"];
            valid.Value = "VALIDATION";
            valid.Merge = true;
            valid.Style.Font.Bold = true;
            valid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //row = 2;

            //var refer = WorkSheet.Cells[$"K{row}:W{row}"];
            //refer.Value = "REFERRED IN";
            //refer.Merge = true;
            //refer.Style.Font.Bold = true;
            //refer.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row = 3;
            var dictionary = await GetCodeDictionary();

            //Set Title of Sheet
            {
                //WorkSheet.Cells[0, 1].Value = "Time Stamp Date";
                //WorkSheet.Cells[$"A{row}"].Value = "Time Stamp Date";
                //WorkSheet.Cells[$"A{row}"].Value = "Time Stamp Date";
                //WorkSheet.Cells[$"A{row}"].Value = "Time Stamp Date";
                var headers = new List<string>()
                {
                    "Time Stamp Date",
                    "Time Stamp Time",
                   // "User Coordinates",
                    "Created By",
                    "Incident Code",

                    MetricsKeys.OrganisationType,
                    "Name of Organisation" ,//nameof(Organisation),
                    "Location of Organisation State",
                    "Organisation LGA",


                    "Contact Channel",

                    "Contact Channel Other",
                    //"Name of Referring Organisation",

                    ////"Type of Service Provided by Referring Organisation",

                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_REFERRAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                    //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_OTHER",

                    //"Incident Code From Referring Organisation",
                    "Was the Violence Fatal",
                    "Who Reported the Incident",
                    "WHO REPORTED THE INCIDENT_OTHER",
                    "Sex of survivor",
                    "Age of survivor",
                    "Mobile No",
                    "MARITAL STATUS",



                    "EMPLOYMENT STATUS OF PARENT/GUARDIAN",
                    "EMPLOYMENT STATUS OF PARENT/GUARDIAN_OTHERS",
                    "EMPLOYMENT STATUS OF SURVIVOR/VICTIM",
                    "EMPLOYMENT STATUS OF SURVIVOR/VICTIM_OTHERS",

                    "VULNERABLE POPULATION_Person living with disability",
                    "VULNERABLE POPULATION_PLHIV",
                    "VULNERABLE POPULATION_Female sex worker",
                    "VULNERABLE POPULATION_IDP",
                    "VULNERABLE POPULATION_DRUG USER",
                    "VULNERABLE POPULATION_WIDOW",
                    "VULNERABLE POPULATION_OUT OF SCHOOL CHILD",
                    "VULNERABLE POPULATION_MINOR",
                    "VULNERABLE POPULATION_House maids/domestic staff",
                    "VULNERABLE POPULATION_CHILD APPRENTICE",
                    "VULNERABLE POPULATION_ORPHANS",
                    "VULNERABLE POPULATION_NOT APPLICABLE",
                    "VULNERABLE POPULATION_OTHER",



                    "EDUCATIONAL STATUS", //"Education of survivor",
                    "EDUCATIONAL STATUS_OTHER",
                    "DOES THE SURVIVOR/VICTIM LIVE ALONE",
                    "WHO DOES THE SURVIVOR/VICTIM LIVE WITH",
                    "WHO DOES THE SURVIVOR/VICTIM LIVE WITH_OTHER",
                    "ESTIMATED AVERAGE MONTHLY INCOME",
                    "DATE OF INCIDENT",
                    "DATE REPORTED",

                    "LOCATION OF VIOLENCE (STATE)",
                    "LOCATION OF VIOLENCE (L.G.A)",
                    "LOCATION OF VIOLENCE (WARD)",
                    "PLACE OF INCIDENT",
                    "PLACE OF INCIDENT_OTHER",
                    "TIME OF THE DAY THAT INCIDENT TOOK PLACE",

                    "TYPE OF VIOLENCE_SEXUAL ASSAULT",
                    "TYPE OF VIOLENCE_PHYSICAL ASSAULT",
                    "TYPE OF VIOLENCE_FINANCIAL/ECONOMIC",
                    "TYPE OF VIOLENCE_ONLINE/CYBER",
                    "TYPE OF VIOLENCE_RAPE",
                    "TYPE OF VIOLENCE_DEFILEMENT",
                    "TYPE OF VIOLENCE_FORCED MARRIAGE",
                    "TYPE OF VIOLENCE_DENIAL OF RESOURCES",
                    "TYPE OF VIOLENCE_PSYCHOLOGICAL/EMOTIONAL ABUSE",
                    "TYPE OF VIOLENCE_FEMALE GENITAL MUTILATION",
                    "TYPE OF VIOLENCE_VIOLATION OF PROPERTY & INHERITANCE RIGHTS",
                    "TYPE OF VIOLENCE_CHILD ABUSE AND NEGLECT",
                    "TYPE OF VIOLENCE_OTHER",
                    //"RECEIVED SERVICE FROM ANOTHER ORGANISATION ",

                    //"TYPE OF SERVICE RECEIVED",
                    /*
                    "Type of Serviced Received By Survivor_Police",
                    "Type of Serviced Received By Survivor_Legal assistance",
                    "Type of Serviced Received By Survivor_Livelihood/Social Welfare Program",
                    "Type of Serviced Received By Survivor_Safe house/shelter",
                    "Type of Serviced Received By Survivor_Psychosocial/counselling",
                    "Type of Serviced Received By Survivor_Medical/Health services",
                    "Type of Serviced Received By Survivor_Other",
                     *
                     */

                    //"TYPE OF SERVICE RECEIVED_OTHER",
                    //"NAME OF CSO/SP WHO PROVIDED SERVICE",
                    //"ADDRESS OF OTHER CSO/SP WHO PROVIDED SERVICE",
                    //"INCIDENT CODE", //FROM ANOTHER ORGANISATION

                    "SEX OF PERPETRATOR_1",
                    "AGE OF PERPETRATOR_1",
                    "SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_1",

                     "SEX OF PERPETRATOR_2",
                    "AGE OF PERPETRATOR_2",
                    "SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_2",

                     "SEX OF PERPETRATOR_3",
                    "AGE OF PERPETRATOR_3",
                    "SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_3",

                    "SEX OF PERPETRATOR_OTHER",
                    "AGE OF PERPETRATOR_OTHER",
                    "SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_OTHER",




                    "DOES THE SURVIVOR WANT ACCESS TO JUSTICE",
                    "OUTCOME OF PROSECUTION",
                    "DATE JUSTICE WAS RECEIVED",
                     "HAS THE CASE BEEN CLOSED",
                    "WHO CLOSED THE CASE?",
                    "DATE CASE WAS CLOSED",
                    //"REASON FOR REFUSING ACCESS TO JUSTICE?",

                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_OTHER",
                    //"TYPE OF SERVICES REFERRED FOR",

                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_POLICE",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_LEGAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_MEDICAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_LIVELIHOOD",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_SHELTER",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_PHYCHOSOCIAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_REFERRAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_FINANCIAL ASSISTANCE_ LEGAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_FINANCIAL ASSISTANCE_ MEDICAL",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_FINANCIAL ASSISTANCE_ SECURITY",
                    //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM_OTHER",

                    //"TYPE OF SERVICE REFERRED FOR _POLICE",
                    //"TYPE OF SERVICE REFERRED FOR _LEGAL",
                    //"TYPE OF SERVICE REFERRED FOR_MEDICAL",
                    //"TYPE OF SERVICE REFERRED FOR_LIVELIHOOD",
                    //"TYPE OF SERVICE REFERRED FOR_SHELTER",
                    //"TYPE OF SERVICE REFERRED FOR_PHYCHOSOCIAL",
                    //"TYPE OF SERVICE REFERRED FOR_FINANCIAL ASSISTANCE_ LEGAL",
                    //"TYPE OF SERVICE REFERRED FOR_FINANCIAL ASSISTANCE_ MEDICAL",
                    //"TYPE OF SERVICE REFERRED FOR_FINANCIAL ASSISTANCE_ SECURITY",
                    //"TYPE OF SERVICES REFERRED FOR_ OTHER",

                    //"NAME OF SERVICE PROVIDER REFERRED TO",

                    //"REFERRAL OUTCOME",
                    //"REFERRAL OUTCOME_OTHER",
                    //"INCIDENT CODE(S) FROM THE RECEIVING ORGANISATION",

                    //"OUTCOME OF THE SERVICE PROVIDED",
                    //"OUTCOME OF THE SERVICE PROVIDED_OTHER",
                    //"HAS THE CASE BEEN CLOSED",
                    //"WHO CLOSED THE CASE?",
                    //"DATE CASE WAS CLOSED",

                    //"GBV_COVIDI9_QUESTION1",
                    //"GBV_COVID19_QUESTION2",
                    //"GBV_COVIDI9_QUESTION3",
                    //"GBV_COVID19_QUESTION4",
                    //"TIME STAMP_ DATE",
                    //"TIME STAMP_ TIME",
                    //"USER COORDINATES",
                    //"CREATED BY",
                    //"DID THE CLIENT RECEIVE ACCESS TO JUSTICE",
                    //"DATE JUSTICE WAS RECEIVED",
                    //"OUTCOME OF THE SERVICE PROVIDED",
                    //"OUTCOME OF THE SERVICE PROVIDED_OTHER",
                    //"HAS THE CASE BEEN CLOSED",
                    //"WHO CLOSED THE CASE?",
                    //"DATE CASE WAS CLOSED",
                    "APPROVED BY ORG. SUPERVISOR_NAME",
                    "APPROVED BY ORG. SUPERVISOR_DATE",
                    "APPROVED BY LGA SUPERVISOR_NAME",
                    "APPROVED BY LGA SUPERVISOR_DATE",
                    "APPROVED BY STATE SUPERVISOR_NAME",
                    "APPROVED BY STATE SUPERVISOR_DATE",
                };
                foreach (var header in headers.Select((item, index) => new { Index = index, Item = item }))
                {
                    WorkSheet.Cells[row, header.Index + 1].Value = header.Item;
                }

                WorkSheet.Cells[row, 1, row, headers.Count].Style.Font.Bold = true;

                //WorkSheet.Cells[$"A{row}:BV{row}"].Style.Font.Bold = true;
            }

            //populate the excel sheet
            foreach (var item in model)
            {
                var infoModel = new List<PerpetratorsInformationModel>();
                var perpCount = item.PerpetratorsInformationList.Count;
                int loopCount = 0;
                if (perpCount >= 3)
                    loopCount = perpCount;
                else
                    loopCount = 3;
                for (int i = 0; i < loopCount; i++)
                {
                    if (perpCount < 3)
                    {
                        item.PerpetratorsInformationList.Add(new PerpetratorsInformationModel
                        {
                            AgeOfPerpetrator = 0,
                            SexOfPerpetrator = "",
                            SurviorRelationWithPerpetrator = ""
                        });
                    }
                    if (i >= 3)
                    {
                        infoModel.Add(item.PerpetratorsInformationList[i]);
                    }

                }
                row++;
                if (IsFullText) //check if the output is with code data or if it is with actual text data
                {
                    try
                    {
                        var dataRow = new List<string>()
                    {
                        item.CreatedAt.Date.ToLongDateString(), //"Time Stamp Date",
                        item.CreatedAt.TimeOfDay.ToString(),//"Time Stamp Time",
                        //$"Longitude: {item.Longitude}, Latitude: {item.Latitude}",//"User Coordinates",
                        item.CreatedByName,
                        item.IncidentCode,
                        item.OrganisationType.ToString(),
                        item.Organisation,
                        item.UserState, //"Location of Organisation State",
                        item.OrganisationLGA,//Organisation LGA

                        //item.IncidentCode,


                        KeyLists.ContactChannels.FirstOrDefault(c => c.ToLower() == item.ContactChannel.ToLower()) ?? "",

                        KeyLists.ContactChannels.Any(c => c.ToLower() == item.ContactChannel.ToLower()) ? "" : item.ContactChannel,//CONTACT CHANNEL OTHER
                        
                        //item.ContactChannelOrganisation ?? "", //"Name of Referring Organisation",

                        //item.ContactChannelOrganisationService is null? "":  string.Join(",", item.ContactChannelOrganisationService), //"Type of Service Provided by Referring Organisation",

                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Referral.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_REFERRAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                        //item.ContactChannelOrganisationService is null ? "": string.Join(",", item.ContactChannelOrganisationService.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_OTHER",

                        //item.ContactChannelOrganisationIncidentCode ?? "", //"Incident Code From Referring Organisation",
                        item.WasViolenceFatal.ToString(),

                        KeyLists.WhoReportedIncident.FirstOrDefault(c => c.ToLower() == item.WhoReportedIncident.ToLower()) ?? "",//

                        KeyLists.WhoReportedIncident.Any(c => c.ToLower() == item.WhoReportedIncident.ToLower()) ? "": item.WhoReportedIncident,//"WHO REPORTED THE INCIDENT_OTHER",
                        
                        item.SexOfSurvior,
                        item.AgeOfSurvior.ToString(),

                        item.SurvivorMobileNo,

                        item.MaritalStatus,

                        KeyLists.employmentStatus.FirstOrDefault(c => c.ToLower() == item.EmploymentStatusOfParentOrGuardian.ToLower()) ?? "",
                        KeyLists.employmentStatus.Any(c => c.ToLower() == item.EmploymentStatusOfParentOrGuardian.ToLower()) ? "" :item.EmploymentStatusOfParentOrGuardian,

                        KeyLists.employmentStatus.FirstOrDefault(c => c.ToLower() == item.EmploymentStatus.ToLower()) ?? "",
                        KeyLists.employmentStatus.Any(c => c.ToLower() == item.EmploymentStatus.ToLower()) ? "" :item.EmploymentStatus,

                        //item.EmploymentStatusOfParentOrGuardian,
                        //item.EmploymentStatus,

                        
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_PLWD) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_PLHIV) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_FSW) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_IDP) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_DrugUser) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Widow) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_OutSchoolChil) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Minor) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_MaidDomesticStaff) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_ChildApprentice) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Orphans) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_NotApplicable) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.VulnerablePopulation is null ? "": string.Join(",", item.VulnerablePopulation.Where(t => !KeyLists.VulnerablePopulation.Any(s => s.ToLower().Contains(t?.ToLower())))),


                        //string.Join(",",item.VulnerablePopulation.Where(t=>KeyLists.VulnerablePopulation.Any(s=>s.ToLower().Contains(t.ToLower())))),
                        //string.Join(",",item.VulnerablePopulation.Where(t=>!KeyLists.VulnerablePopulation.Any(s=>s.ToLower().Contains(t.ToLower())))),//"VULNERABLE POPULATION_OTHER
                        //KeyLists.VulnerablePopulation.FirstOrDefault(c => c.ToLower() == item.VulnerablePopulation.ToLower()) ?? "" ,

                        //KeyLists.VulnerablePopulation.Any(c => c.ToLower() == item.VulnerablePopulation.ToLower()) ? "": item.VulnerablePopulation ,//"VULNERABLE POPULATION_OTHER",
                        item.Education, //"Education of survivor",
                        KeyLists.educationalStatus.Any(c => c.ToLower() == item.Education.ToLower()) ? "" :item.Education, //"EDUCATIONAL STATUS_OTHER",

                        item.DoesSurviorLiveAlone.ToString(),
                        KeyLists.WhoDoesSurviorLiveWith.FirstOrDefault(c => c.ToLower() == item.WhoDoesSurviorLiveWith.ToLower()) ?? "",
                        KeyLists.WhoDoesSurviorLiveWith.Any(c => c.ToLower() == item.WhoDoesSurviorLiveWith.ToLower()) ? "" :item.WhoDoesSurviorLiveWith, //"WHO DOES THE SURVIVOR/VICTIM LIVE WITH_OTHER",
                        item.SurvivorEstimatedAverageMonthlyIncome,

                        item.DateOfIncident.ToString("dd-MMM-yyyy"),
                        item.DateReported.ToString("dd-MMM-yyyy"),

                        item.IncidentState,
                        item.IncidentLGA,
                        item.IncidentWard,
                        KeyLists.ActualLocationOfIncident.FirstOrDefault(c => c.ToLower() == item.ActualLocationOfIncident.ToLower()) ?? "",
                        KeyLists.ActualLocationOfIncident.Any(c => c.ToLower() == item.ActualLocationOfIncident.ToLower()) ? "": item.ActualLocationOfIncident,//"ACTUAL LOCATION OF INCIDENT_OTHER",
                        item.TimeOfDay.ToString(),

                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.SexualAssault) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),

                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.PhysicalAssault) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.FinancialOrEconomic) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.OnlineOrCyberBullying) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.Rape) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.Defilement) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.ForcedMarriage) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.DenialOfResourcesOrServices) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.EmotionalOrPsychological) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.FemaleGenitalMutilation) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.ViolationOfPropertyAndInheritanceRight) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.CaseCategories.Any(s=> s== (int)CaseCategoryOrTypeOfViolence.ChildAbuseAndNeglect) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        //string.Join(",", item.CaseCategories.ConvertAll(c => Enum.TryParse(typeof(CaseCategoryOrTypeOfViolence), c.ToString(), out _) ? ((CaseCategoryOrTypeOfViolence)c).ToString().Humanize() : "")), //KeyLists.TypeOfViolence.FirstOrDefault(c => c.ToLower() == item.Category.ToLower()) ?? "",
                        item.CaseCategoriesOthers,//KeyLists.TypeOfViolence.Any(c => c.ToLower() != item.Category.ToLower()) ? "": item.Category, //"TYPE OF VIOLENCE_OTHER",
                        //item.HasSurviorReceivedService.ToString(),
                        //item.TypeOfServiceReceivedBySurvior is null ? "" : string.Join(",", item.TypeOfServiceReceivedBySurvior.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE RECEIVED",
                        //item.TypeOfServiceReceivedBySurvior is null ? "": string.Join(",", item.TypeOfServiceReceivedBySurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE RECEIVED_OTHER",
                        //item.OtherServiceProviderName, //"NAME OF CSO/SP WHO PROVIDED SERVICE",
                        //item.OtherServiceProviderAddress, //"ADDRESS OF OTHER CSO/SP WHO PROVIDED SERVICE",
                        //item.OtherServiceProviderIncidentCode, //"INCIDENT CODE", //FROM ANOTHER ORGANISATION

                        //item.NumberOfPerpetrators == "0" ? "Don't know"  : new [] {"1", "2", "3", "0"}.FirstOrDefault(i => i == item.NumberOfPerpetrators) ?? "",
                        //new [] {"1", "2", "3", "0"}.Any(i => i == item.NumberOfPerpetrators) ? "" : item.NumberOfPerpetrators, //"NUMBER OF PERPETRATOR(S)_OTHER",
                        //item.IsSurviorContinuousThreat.ToString(),

                        item.PerpetratorsInformationList.First().SexOfPerpetrator,
                        item.PerpetratorsInformationList.First().AgeOfPerpetrator.ToString()??"",
                        KeyLists.Relationships.FirstOrDefault(c=>c.ToLower()==item.PerpetratorsInformationList.First().SurviorRelationWithPerpetrator?.ToLower())??item.PerpetratorsInformationList.First()?.SurviorRelationWithPerpetrator.FirstCharToUpper(),



                        item.PerpetratorsInformationList[1]?.SexOfPerpetrator,
                        item.PerpetratorsInformationList[1]?.AgeOfPerpetrator.ToString()??"",
                        KeyLists.Relationships.FirstOrDefault(c=>c.ToLower()==item.PerpetratorsInformationList[1]?.SurviorRelationWithPerpetrator?.ToLower())??item.PerpetratorsInformationList[1]?.SurviorRelationWithPerpetrator,

                        item.PerpetratorsInformationList[2]?.SexOfPerpetrator,
                        item.PerpetratorsInformationList[2]?.AgeOfPerpetrator.ToString()??"",
                        KeyLists.Relationships.FirstOrDefault(c=>c.ToLower()==item.PerpetratorsInformationList[2]?.SurviorRelationWithPerpetrator?.ToLower())??item.PerpetratorsInformationList[2]?.SurviorRelationWithPerpetrator,


                        string.Join(",",  infoModel.Select(p => p.SexOfPerpetrator)),
                        string.Join(",",  infoModel.Select(p => p.AgeOfPerpetrator)),
                        string.Join(",",  infoModel.Select(p => KeyLists.Relationships.Any(c => c.ToLower() == p.SurviorRelationWithPerpetrator?.ToLower()) ? "" : p.SurviorRelationWithPerpetrator)),  //"SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_OTHER",


                        //string.Join(",",  item.PerpetratorsInformationList.Select(p => p.SexOfPerpetrator)),
                        //string.Join(",",  item.PerpetratorsInformationList.Select(p => p.AgeOfPerpetrator)),
                        //string.Join(",",  item.PerpetratorsInformationList.Select(p => KeyLists.Relationships.FirstOrDefault(c => c.ToLower() == p.SurviorRelationWithPerpetrator?.ToLower()) ?? "")),
                        //string.Join(",",  item.PerpetratorsInformationList.Select(p => KeyLists.Relationships.Any(c => c.ToLower() == p.SurviorRelationWithPerpetrator?.ToLower()) ? "" : p.SurviorRelationWithPerpetrator)),  //"SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_OTHER",

                            
                      
                        item.DoestheSurviorWantJustice.ToString(), //"DOES THE SURVIVOR WANT ACCESS TO JUSTICE",
                        //item.SurviorDoesNotWantJusticeReasons is null ? "": string.Join(",", item.SurviorDoesNotWantJusticeReasons), //"REASON FOR REFUSING ACCESS TO JUSTICE?",

                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //typeOfServiceProvidedToSurvior
                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //other
                        //item.ActualReferralServiceReceived is null ? "": string.Join(",", item.ActualReferralServiceReceived),//"TYPE OF SERVICES REFERRED FOR", //ActualReferralServiceReceived

                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Referral.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_REFERRAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                    //    item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                    //    item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),   //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //other

                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                    //    item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                    //    item.ActualReferralServiceReceived is null ? "": string.Join(",", item.ActualReferralServiceReceived.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_OTHER",

                    // item.NameOfServiceProviderReferredTo, //"NAME OF SERVICE PROVIDER REFERRED TO",

                    // KeyLists.ReferralOutcome.FirstOrDefault(c => c.ToString() == item.ReferralOutcome) ?? "", //"REFERRAL OUTCOME",
                    // KeyLists.ReferralOutcome.Any(c => c.ToString() == item.ReferralOutcome) ? "" : item.ReferralOutcome, //"REFERRAL OUTCOME_OTHER",
                    //item.ReceivingOrganisationCode ,//"INCIDENT CODE(S) FROM THE RECEIVING ORGANISATION",

                    //KeyLists.OutcomeOfServiceProvided.FirstOrDefault(c => c.ToString() == item.OutcomeOfServiceorReferral) ?? "",// "OUTCOME OF THE SERVICE PROVIDED",
                    //KeyLists.OutcomeOfServiceProvided.Any(c => c.ToString() == item.OutcomeOfServiceorReferral) ? "" : item.OutcomeOfServiceorReferral, // "OUTCOME OF THE SERVICE PROVIDED_OTHER",
                    //item.HasCaseBeenClosed.ToString(), // "HAS THE CASE BEEN CLOSED",
                    //item.WhoClosedTheCase, //"WHO CLOSED THE CASE?",
                    //item.CaseClosedDate.HasValue ? item.CaseClosedDate.Value.ToLongDateString() : "", //"DATE CASE WAS CLOSED",

                    //item.GBV_COVID19_Question1,
                    //item.GBV_COVID19_Question2,
                    //item.GBV_COVID19_Question3,
                    //item.GBV_COVID19_Question4,

                    //item.FollowUps.FirstOrDefault()?.CaseClosedDate.Date.ToLongDateString(),// "TIME STAMP_ DATE",
                    //item.FollowUps.FirstOrDefault()?.CaseClosedDate.TimeOfDay.ToString(),// "TIME STAMP_ TIME",
                    //$"Longitude: {item.FollowUps.FirstOrDefault()?.Longitude}, Latitude: {item.FollowUps.FirstOrDefault()?.Latitude}", //"USER COORDINATES",
                    //item.FollowUps.FirstOrDefault()?.CreatedBy,// "CREATED BY",
                    //item.FollowUps.FirstOrDefault()?.HasClientReceivedjustice.ToString(),// "DID THE CLIENT RECEIVE ACCESS TO JUSTICE",
                    
                    //KeyLists.OutcomeOfServiceFollowUp.FirstOrDefault(c => c.ToString() == item.FollowUps.FirstOrDefault()?.FinalOutcome ) ?? "", //"OUTCOME OF THE SERVICE PROVIDED",
                    item.OutcomeOfProsecution,
                    //item.FollowUps.FirstOrDefault()?.JusticeReceivedDate.GetValueOrDefault().ToString(), 
                    item.DateJusticeReceived.HasValue ? item.DateJusticeReceived?.ToString("dd-MMM-yyyy"):"",//"DATE JUSTICE WAS RECEIVED",
                    //KeyLists.OutcomeOfServiceFollowUp.Any(c => c.ToString() == item.FollowUps.FirstOrDefault()?.FinalOutcome) ? "" : item.FollowUps.FirstOrDefault()?.FinalOutcome, //"OUTCOME OF THE SERVICE PROVIDED_OTHER",
                    //item.FollowUps.FirstOrDefault()?.HasCaseBeenClosed.ToString(), 
                    item.HasCaseBeenClosed.ToString(),//"HAS THE CASE BEEN CLOSED",
                    //item.FollowUps.FirstOrDefault()?.WhoClosedTheCase,  
                    item.WhoClosedTheCase,//"WHO CLOSED THE CASE?",
                    //item.FollowUps.FirstOrDefault()?.CaseClosedDate.Date.ToString(), 
                    item.CaseClosedDate.HasValue ? item.CaseClosedDate?.ToString("dd-MMM-yyyy"):"",//"DATE CASE WAS CLOSED",

                    //(item.IsApproved) ? YesOrNo.Yes.ToString() : YesOrNo.No.ToString(),

                    item.ValidatedByName, //"APPROVED BY ORG. SUPERVISOR_NAME",
                    item.ValidatedAt.HasValue ? item.ValidatedAt?.ToString("dd-MMM-yyyy") : "", //"APPROVED BY ORG. SUPERVISOR_DATE",

                    item.LgaValidatedByName, //"APPROVED BY LGA SUPERVISOR_NAME",
                    item.LgaValidatedAt.HasValue ? item.LgaValidatedAt?.ToString("dd-MMM-yyyy") : "", //"APPROVED BY LGA SUPERVISOR_DATE",

                    item.ApprovedByName, //"APPROVED BY STATE SUPERVISOR_NAME",
                    item.ApprovedAt.HasValue ? item.ApprovedAt?.ToString("dd-MMM-yyyy") : "",//"APPROVED BY STATE SUPERVISOR_DATE",
                    };

                        foreach (var data in dataRow.Select((item, index) => new { Index = index, Item = item }))
                        {
                            WorkSheet.Cells[row, data.Index + 1].Value = data.Item;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    try
                    {
                        var dataRow = new List<object>()
                    {


                        item.CreatedAt.Date.ToLongDateString(), //"Time Stamp Date",
                        item.CreatedAt.TimeOfDay.ToString(),//"Time Stamp Time",
                        //$"Longitude: {item.Longitude}, Latitude: {item.Latitude}",//"User Coordinates",
                        item.CreatedByName,
                        item.IncidentCode,
                        (int) item.OrganisationType,
                        item.Organisation,
                        item.UserState, //"Location of Organisation State",
                        item.OrganisationLGA, //Organisation LGA

                        string.IsNullOrWhiteSpace(item.ContactChannel)
                            ? ""
                            : GetValueFromDictionary(Field.ContactChannel.ToString(), item.ContactChannel, dictionary)
                                .ToString(),

                        KeyLists.ContactChannels.Any(c => c.ToLower() == item.ContactChannel.ToLower()) ? "" : item.ContactChannel,//CONTACT CHANNEL OTHER
                        //item.ContactChannelOrganisation ?? "", //"Name of Referring Organisation",



                        //item.ContactChannelOrganisationService is null? "":  string.Join(",", item.ContactChannelOrganisationService), //"Type of Service Provided by Referring Organisation",

                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_Referral.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_REFERRAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                        //item.ContactChannelOrganisationService?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                        //item.ContactChannelOrganisationService is null ? "": string.Join(",", item.ContactChannelOrganisationService.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_OTHER",


                        //item.ContactChannelOrganisationIncidentCode ?? "", //"Incident Code From Referring Organisation",

                        (int) item.WasViolenceFatal,

                         string.IsNullOrWhiteSpace(item.WhoReportedIncident)
                            ? ""
                            : GetValueFromDictionary(Field.WhoReported.ToString(), item.WhoReportedIncident, dictionary)
                                .ToString(),

                        KeyLists.WhoReportedIncident.Any(c => c.ToLower() == item.WhoReportedIncident.ToLower()) ? "": item.WhoReportedIncident,//"WHO REPORTED THE INCIDENT_OTHER",

                        
                        string.IsNullOrWhiteSpace(item.SexOfSurvior)
                            ? ""
                            : GetValueFromDictionary(Field.Sex.ToString(), item.SexOfSurvior, dictionary).ToString(),

                        item.AgeOfSurvior,
                        item.SurvivorMobileNo,

                         string.IsNullOrWhiteSpace(item.MaritalStatus)
                            ? ""
                            : GetValueFromDictionary(Field.MaritalStatus.ToString(), item.MaritalStatus, dictionary)
                                .ToString(),

                        string.IsNullOrWhiteSpace(item.EmploymentStatusOfParentOrGuardian)
                            ? ""
                            : GetValueFromDictionary(Field.EmploymentStatus.ToString(), item.EmploymentStatusOfParentOrGuardian, dictionary).ToString(),

                          KeyLists.employmentStatus.Any(c => c.ToLower() == item.EmploymentStatusOfParentOrGuardian.ToLower()) ? "" :item.EmploymentStatusOfParentOrGuardian,


                        string.IsNullOrWhiteSpace(item.EmploymentStatus)
                            ? ""
                            : GetValueFromDictionary(Field.EmploymentStatus.ToString(), item.EmploymentStatus, dictionary).ToString(),

                        KeyLists.employmentStatus.Any(c => c.ToLower() == item.EmploymentStatus.ToLower()) ? "" :item.EmploymentStatus,

                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_PLWD) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_PLHIV) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_FSW) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_IDP) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_DrugUser) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Widow) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_OutSchoolChil) ==true ? YesOrNo.Yes: YesOrNo.No.ToString(),
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Minor) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_MaidDomesticStaff) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_ChildApprentice) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_Orphans) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.VulnerablePopulation?.Any(s=> s == MetricsKeys.VulnerablePopulation_NotApplicable) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.VulnerablePopulation is null ? "": string.Join(",", item.VulnerablePopulation.Where(t => !KeyLists.VulnerablePopulation.Any(s => s.ToLower().Contains(t.ToLower())))),



                        //item.VulnerablePopulation  is null
                        //?""
                        //:GetValueFromDictionary(Field.VulnerablePopulation.ToString(),string.Join(",",item.VulnerablePopulation.Where(t=>KeyLists.VulnerablePopulation.Any(s=>s.ToLower().Contains(t.ToLower())))),
                        //        dictionary).ToString(),
                        //string.Join(",",item.VulnerablePopulation.Where(t=>!KeyLists.VulnerablePopulation.Any(s=>s.ToLower().Contains(t.ToLower())))),//"VULNERABLE POPULATION_OTHER",
                        #region VulnerablePopulation Initial Implementation
                        //    string.IsNullOrWhiteSpace(item.VulnerablePopulation)
                        //    ? ""
                        //    : GetValueFromDictionary(Field.VulnerablePopulation.ToString(), item.VulnerablePopulation,
                        //        dictionary).ToString(),

                        //KeyLists.VulnerablePopulation.Any(c => c.ToLower() == item.VulnerablePopulation.ToLower()) ? "": item.VulnerablePopulation ,//"VULNERABLE POPULATION_OTHER",
#endregion
                            string.IsNullOrWhiteSpace(item.Education)
                            ? ""
                            : GetValueFromDictionary(Field.Education.ToString(), item.Education, dictionary).ToString(), //"Education of survivor",


                        KeyLists.educationalStatus.Any(c => c.ToLower() == item.Education.ToLower()) ? "" :item.Education, //"EDUCATIONAL STATUS_OTHER",


                        (int) item.DoesSurviorLiveAlone,
                        string.IsNullOrWhiteSpace(item.WhoDoesSurviorLiveWith)
                            ? ""
                            : GetValueFromDictionary(Field.LiveWith.ToString(), item.WhoDoesSurviorLiveWith, dictionary)
                                .ToString(),

                        KeyLists.WhoDoesSurviorLiveWith.Any(c => c.ToLower() == item.WhoDoesSurviorLiveWith.ToLower()) ? "" :item.WhoDoesSurviorLiveWith, //"WHO DOES THE SURVIVOR/VICTIM LIVE WITH_OTHER",
                        
                        item.SurvivorEstimatedAverageMonthlyIncome,

                        item.DateOfIncident.ToString("dd-MMM-yyyy"),
                        item.DateReported.ToString("dd-MMM-yyyy"),

                        GetValueFromDictionary(nameof(State), item.IncidentState, dictionary),
                        GetValueFromDictionary(MetricsKeys.LGA, item.IncidentLGA, dictionary),
                        string.IsNullOrWhiteSpace(item.IncidentWard)
                            ? ""
                            : GetValueFromDictionary(nameof(Ward), item.IncidentWard, dictionary).ToString(),
                        string.IsNullOrWhiteSpace(item.ActualLocationOfIncident)
                            ? ""
                            : GetValueFromDictionary(Field.IncidentLocation.ToString(), item.ActualLocationOfIncident,
                                dictionary).ToString(),
                        KeyLists.ActualLocationOfIncident.Any(c => c.ToLower() == item.ActualLocationOfIncident.ToLower()) ? "": item.ActualLocationOfIncident,//"ACTUAL LOCATION OF INCIDENT_OTHER",
                        (int) item.TimeOfDay,

                        //item.CaseCategoryId,
                        //item.CaseCategories.ConvertAll(v => (int)(CaseCategoryOrTypeOfViolence)v),

                          item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.SexualAssault) ==true ? YesOrNo.Yes: YesOrNo.No,

                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.PhysicalAssault) ==true ? YesOrNo.Yes: YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.FinancialOrEconomic) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.OnlineOrCyberBullying) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.Rape) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.Defilement) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.ForcedMarriage) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.DenialOfResourcesOrServices) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.EmotionalOrPsychological) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.FemaleGenitalMutilation) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.ViolationOfPropertyAndInheritanceRight) ==true ? YesOrNo.Yes : YesOrNo.No,
                        item.CaseCategories.Any(s=> s== (int) CaseCategoryOrTypeOfViolence.ChildAbuseAndNeglect) ==true ? YesOrNo.Yes : YesOrNo.No,

                        item.CaseCategoriesOthers,//KeyLists.TypeOfViolence.Any(c => c.ToLower() != item.Category.ToLower()) ? "" : item.Category, //"TYPE OF VIOLENCE_OTHER",
                       
                        item.NumberOfPerpetrators,
                        new [] {"1", "2", "3", "0"}.Any(i => i == item.NumberOfPerpetrators) ? "" : item.NumberOfPerpetrators, //"NUMBER OF PERPETRATOR(S)_OTHER",
                        (int) item.IsSurviorContinuousThreat,

                        string.Join(",",  item.PerpetratorsInformationList.Select(p =>   GetValueFromDictionary(Field.Sex.ToString(), p.SexOfPerpetrator, dictionary).ToString() )),
                        string.Join(",",  item.PerpetratorsInformationList.Select(p => p.AgeOfPerpetrator)),
                        string.Join(",",  item.PerpetratorsInformationList.Select(p =>  GetValueFromDictionary(Field.Relationship.ToString(), KeyLists.Relationships.FirstOrDefault(c => c.ToLower() == p.SurviorRelationWithPerpetrator?.ToLower()) ?? "" , dictionary).ToString() )),
                        string.Join(",",  item.PerpetratorsInformationList.Select(p => KeyLists.Relationships.Any(c => c.ToLower() == p.SurviorRelationWithPerpetrator?.ToLower()) ? "" : p.SurviorRelationWithPerpetrator)) , //"SURVIVOR/VICTIM'S RELATIONSHIP WITH PERPETRATOR_OTHER",

                        (int) item.DoestheSurviorWantJustice,


                        KeyLists.OutcomeOfServiceFollowUp.FirstOrDefault(c => c.ToString() == item.FollowUps.FirstOrDefault()?.FinalOutcome) ?? "", //"OUTCOME OF THE SERVICE PROVIDED",
                        
                        item.DateJusticeReceived.GetValueOrDefault(), //"DATE JUSTICE WAS RECEIVED",
                        item.HasCaseBeenClosed, //"HAS THE CASE BEEN CLOSED",
                        item.WhoClosedTheCase,  //"WHO CLOSED THE CASE?",
                        item.CaseClosedDate.GetValueOrDefault(), //"DATE CASE WAS CLOSED",

                        // item.FollowUps.FirstOrDefault()?.JusticeReceivedDate.GetValueOrDefault(), //"DATE JUSTICE WAS RECEIVED",
                        //item.FollowUps.FirstOrDefault()?.HasCaseBeenClosed, //"HAS THE CASE BEEN CLOSED",
                        //item.FollowUps.FirstOrDefault()?.WhoClosedTheCase,  //"WHO CLOSED THE CASE?",
                        //item.FollowUps.FirstOrDefault()?.CaseClosedDate.Date, //"DATE CASE WAS CLOSED",
                        item.ValidatedByName, //"APPROVED BY ORG. SUPERVISOR_NAME",
                          
                        item.ValidatedAt.HasValue? item.ValidatedAt?.ToString("dd-MMM-yyyy") : "", //"APPROVED BY ORG. SUPERVISOR_DATE",

                        item.LgaValidatedByName, //"APPROVED BY LGA SUPERVISOR_NAME",
                        item.LgaValidatedAt.HasValue? item.LgaValidatedAt?.ToString("dd-MMM-yyyy") : "", //"APPROVED BY LGA SUPERVISOR_DATE",

                        item.ApprovedByName, //"APPROVED BY STATE SUPERVISOR_NAME",
                        item.ApprovedAt.HasValue? item.ApprovedAt?.ToString("dd-MMM-yyyy") : "", //"APPROVED BY STATE SUPERVISOR_DATE",



                        //item.HasSurviorReceivedService.ToString(),
                        //item.TypeOfServiceReceivedBySurvior is null ? "" : string.Join(",", item.TypeOfServiceReceivedBySurvior.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE RECEIVED",
                        //item.TypeOfServiceReceivedBySurvior is null ? "": string.Join(",", item.TypeOfServiceReceivedBySurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE RECEIVED_OTHER",
                        //item.OtherServiceProviderName, //"NAME OF CSO/SP WHO PROVIDED SERVICE",
                        //item.OtherServiceProviderAddress, //"ADDRESS OF OTHER CSO/SP WHO PROVIDED SERVICE",
                        //item.OtherServiceProviderIncidentCode, //"INCIDENT CODE", //FROM ANOTHER ORGANISATION

                      
                        
                        
                        
                        //item.SurviorDoesNotWantJusticeReasons is null ? "":   string.Join(",", item.SurviorDoesNotWantJusticeReasons), //"REASON FOR REFUSING ACCESS TO JUSTICE?",

                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //typeOfServiceProvidedToSurvior

                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //typeOfServiceProvidedToSurvior
                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //other
                        //item.ActualReferralServiceReceived is null ? "": string.Join(",", item.ActualReferralServiceReceived),//"TYPE OF SERVICES REFERRED FOR", //ActualReferralServiceReceived

                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No, //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes: YesOrNo.No, //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_Referral.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_REFERRAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                        //item.TypeOfServiceProvidedToSurvior?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                        //item.TypeOfServiceProvidedToSurvior is null ? "": string.Join(",", item.TypeOfServiceProvidedToSurvior.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),  //"TYPE OF SERVICE PROVIDED TO THE SURVIVOR/VICTIM", //other

                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_Servicesofthepolice.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No, //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION _POLICE",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_Legalassistance.Replace("TypeOfService_", ""))  ==true ? YesOrNo.Yes: YesOrNo.No, //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LEGAL",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_MedicalHealth.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_MEDICAL",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_LivelihoodSocialWelfare.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_LIVELIHOOD",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_SafeHouse.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_SHELTER",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_PsychosocialCounselling.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_PHYCHOSOCIAL",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceLegal.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ LEGAL",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceMedical.Replace("TypeOfService_", ""))  ==true? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ MEDICAL",
                        //item.ActualReferralServiceReceived?.Any(s=> s == MetricsKeys.TypeOfService_FinancialAssistanceSecurity.Replace("TypeOfService_", "")) ==true ? YesOrNo.Yes: YesOrNo.No,//"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_FINANCIAL ASSISTANCE_ SECURITY",
                        //item.ActualReferralServiceReceived is null ? "": string.Join(",", item.ActualReferralServiceReceived.Where(t => !KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))), //"TYPE OF SERVICE PROVIDED BY REFERRING ORGANISATION_OTHER",

                    // item.NameOfServiceProviderReferredTo, //"NAME OF SERVICE PROVIDER REFERRED TO",

                    // KeyLists.ReferralOutcome.FirstOrDefault(c => c.ToString() == item.ReferralOutcome) ?? "", //"REFERRAL OUTCOME",
                    // KeyLists.ReferralOutcome.Any(c => c.ToString() == item.ReferralOutcome) ? "" : item.ReferralOutcome, //"REFERRAL OUTCOME_OTHER",
                    //item.ReceivingOrganisationCode ,//"INCIDENT CODE(S) FROM THE RECEIVING ORGANISATION",

                    //KeyLists.OutcomeOfServiceProvided.FirstOrDefault(c => c.ToString() == item.OutcomeOfServiceorReferral) == null
                    //    ? ""
                    //    : GetValueFromDictionary(Field.ServiceOutcome.ToString(), KeyLists.OutcomeOfServiceProvided.FirstOrDefault(c => c.ToString() == item.OutcomeOfServiceorReferral),
                    //        dictionary).ToString(), // "OUTCOME OF THE SERVICE PROVIDED",

                    //KeyLists.OutcomeOfServiceProvided.Any(c => c.ToString() == item.OutcomeOfServiceorReferral) ? "" : item.OutcomeOfServiceorReferral, // "OUTCOME OF THE SERVICE PROVIDED_OTHER",

                    //(int)item.HasCaseBeenClosed, // "HAS THE CASE BEEN CLOSED",
                    //string.IsNullOrWhiteSpace(item.WhoClosedTheCase)
                    //    ? ""
                    //    : GetValueFromDictionary(Field.WhoClosedTheCase.ToString(), item.WhoClosedTheCase,
                    //        dictionary).ToString(),//"WHO CLOSED THE CASE?",

                    //item.CaseClosedDate.HasValue? item.CaseClosedDate.Value.ToLongDateString() : "", //"DATE CASE WAS CLOSED",

                    //item.GBV_COVID19_Question1,
                    //item.GBV_COVID19_Question2,
                    //item.GBV_COVID19_Question3,
                    //item.GBV_COVID19_Question4,

                    //item.FollowUps.FirstOrDefault()?.CaseClosedDate.Date.ToLongDateString(), //"TIME STAMP_ DATE",
                    //item.FollowUps.FirstOrDefault()?.CaseClosedDate.TimeOfDay, //"TIME STAMP_ TIME",
                    //$"Longitude: {item.FollowUps.FirstOrDefault()?.Longitude}, Latitude: {item.FollowUps.FirstOrDefault()?.Latitude}", //"USER COORDINATES",
                    //item.FollowUps.FirstOrDefault()?.CreatedBy, //"CREATED BY",
                    //item.FollowUps.FirstOrDefault()?.HasClientReceivedjustice, //"DID THE CLIENT RECEIVE ACCESS TO JUSTICE",
                    //item.FollowUps.FirstOrDefault()?.JusticeReceivedDate.GetValueOrDefault(), //"DATE JUSTICE WAS RECEIVED",
                    
                    //KeyLists.OutcomeOfServiceFollowUp.Any(c => c.ToString() == item.FollowUps.FirstOrDefault()?.FinalOutcome) ? "" : item.FollowUps.FirstOrDefault()?.FinalOutcome, //"OUTCOME OF THE SERVICE PROVIDED_OTHER",
                    
                    

                    //(item.IsApproved) ? (int)YesOrNo.Yes : (int)YesOrNo.No,

                   
                    };

                        foreach (var data in dataRow.Select((item, index) => new { Index = index, Item = item }))
                        {
                            WorkSheet.Cells[row, data.Index + 1].Value = data.Item;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                infoModel.Clear();
            }

            await Excelpackage.SaveAsync();

            return Excelpackage;
        }



        /// <summary>
        /// populates the SPSS data records
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="models"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private async Task<MemoryStream> WriteSPSSRecord(List<Variable> variables, List<CaseViewModel> models, Dictionary<string, Dictionary<int, string>> dictionary)
        {
            var filename = @"default.sav";

            var options = new SpssOptions();

            //write the data to sav file called default
            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                using var writer = new SpssWriter(fileStream, variables, options);

                foreach (var item in models)
                {
                    var newRecord = writer.CreateRecord();
                    newRecord[0] = item.IncidentCode;
                    newRecord[1] = (double)item.OrganisationType;
                    newRecord[2] = item.Organisation;

                    newRecord[3] = (double)GetValueFromDictionary(nameof(State), item.IncidentState, dictionary);
                    newRecord[4] = (double)GetValueFromDictionary(MetricsKeys.LGA, item.IncidentLGA, dictionary);
                    newRecord[5] = string.IsNullOrWhiteSpace(item.IncidentWard) ? -1.0 : GetValueFromDictionary(nameof(Ward), item.IncidentWard, dictionary);
                    newRecord[6] = item.CreatedByName;
                    //newRecord[7] = (double)item.CaseCategoryId;
                    //newRecord[7] = (double)item.CaseCategoryId;
                    newRecord[8] = string.IsNullOrWhiteSpace(item.ActualLocationOfIncident) ? -1.0 : (double)GetValueFromDictionary(Field.IncidentLocation.ToString(), item.ActualLocationOfIncident, dictionary);
                    newRecord[9] = (double)item.AgeOfSurvior;
                    //newRecord[10] = (double)item.AgeOfPerpetrator;
                    newRecord[11] = string.IsNullOrWhiteSpace(item.ContactChannel) ? -1.0 : (double)GetValueFromDictionary(Field.ContactChannel.ToString(), item.ContactChannel, dictionary);
                    newRecord[12] = item.DateOfIncident.ToString("dd/MM/yyyy");
                    newRecord[13] = item.DateReported.ToString("dd/MM/yyyy");
                    newRecord[14] = (double)item.TimeOfDay;
                    newRecord[15] = (double)item.DoesSurviorLiveAlone;
                    newRecord[16] = string.IsNullOrWhiteSpace(item.WhoDoesSurviorLiveWith) ? -1.0 : (double)GetValueFromDictionary(Field.LiveWith.ToString(), item.WhoDoesSurviorLiveWith, dictionary);
                    newRecord[17] = (double)item.DoestheSurviorWantJustice;
                    newRecord[18] = (double)item.HasSurviorReceivedService;
                    newRecord[19] = string.IsNullOrWhiteSpace(item.Education) ? -1.0 : (double)GetValueFromDictionary(Field.Education.ToString(), item.Education, dictionary);
                    newRecord[20] = string.IsNullOrWhiteSpace(item.EmploymentStatus) ? -1.0 : (double)GetValueFromDictionary(Field.EmploymentStatus.ToString(), item.EmploymentStatus, dictionary);
                    newRecord[21] = string.IsNullOrWhiteSpace(item.EmploymentStatusOfParentOrGuardian) ? -1.0 : (double)GetValueFromDictionary(Field.EmploymentStatus.ToString(), item.EmploymentStatusOfParentOrGuardian, dictionary);
                    newRecord[22] = (double)item.IsSurviorContinuousThreat;
                    newRecord[23] = string.IsNullOrWhiteSpace(item.MaritalStatus) ? -1.0 : (double)GetValueFromDictionary(Field.MaritalStatus.ToString(), item.MaritalStatus, dictionary);
                    newRecord[24] = string.IsNullOrWhiteSpace(item.SexOfSurvior) ? -1.0 : (double)GetValueFromDictionary(Field.Sex.ToString(), item.SexOfSurvior, dictionary);
                    // newRecord[25] = string.IsNullOrWhiteSpace(item.SexOfPerpetrator) ? -1.0 :
                    // (double)GetValueFromDictionary(Field.Sex.ToString(), item.SexOfPerpetrator, dictionary);
                    newRecord[26] = item.NumberOfPerpetrators;
                    newRecord[27] = (double)GetListValue(MetricsKeys.VulnerablePopulation_PLHIV, item.VulnerablePopulation);
                    newRecord[28] = (double)GetListValue(MetricsKeys.VulnerablePopulation_PLWD, item.VulnerablePopulation);
                    newRecord[29] = (double)GetListValue(MetricsKeys.VulnerablePopulation_FSW, item.VulnerablePopulation);
                    newRecord[30] = (double)GetListValue(MetricsKeys.VulnerablePopulation_DrugUser, item.VulnerablePopulation);
                    newRecord[31] = (double)GetListValue(MetricsKeys.VulnerablePopulation_IDP, item.VulnerablePopulation);
                    newRecord[32] = (double)GetListValue(MetricsKeys.VulnerablePopulation_OutSchoolChil, item.VulnerablePopulation);
                    newRecord[33] = (double)GetListValue(MetricsKeys.VulnerablePopulation_Minor, item.VulnerablePopulation);
                    newRecord[34] = (double)GetListValue(MetricsKeys.VulnerablePopulation_Widow, item.VulnerablePopulation);
                    newRecord[35] = (double)GetListValue(MetricsKeys.VulnerablePopulation_MaidDomesticStaff, item.VulnerablePopulation);
                    newRecord[36] = (double)GetListValue(MetricsKeys.VulnerablePopulation_NotApplicable, item.VulnerablePopulation);
                    newRecord[37] = (double)GetListValue(MetricsKeys.VulnerablePopulation_Orphans, item.VulnerablePopulation);
                    newRecord[38] = (double)GetListValue(MetricsKeys.VulnerablePopulation_ChildApprentice, item.VulnerablePopulation);
                    //newRecord[27] = string.IsNullOrWhiteSpace(item.VulnerablePopulation) ? -1.0 : (double)GetValueFromDictionary(Field.VulnerablePopulation.ToString(), item.VulnerablePopulation, dictionary);
                    newRecord[39] = (double)item.WasViolenceFatal;
                    newRecord[40] = string.IsNullOrWhiteSpace(item.WhoReportedIncident) ? -1.0 : (double)GetValueFromDictionary(Field.WhoReported.ToString(), item.WhoReportedIncident, dictionary);

                    //newRecord[30] = string.IsNullOrWhiteSpace(item.SurviorRelationWithPerpetrator) ? -1.0 : (double)GetValueFromDictionary(Field.Relationship.ToString(), item.SurviorRelationWithPerpetrator, dictionary);
                    newRecord[41] = item.GBV_COVID19_Question1;
                    newRecord[42] = item.GBV_COVID19_Question2;
                    newRecord[43] = item.GBV_COVID19_Question3;
                    newRecord[44] = item.GBV_COVID19_Question4;
                    newRecord[45] = (double)item.HasCaseBeenClosed;
                    newRecord[46] = item.NameOfServiceProviderReferredTo;
                    newRecord[47] = (double)GetListValue(MetricsKeys.FollowUpAction_SafeHouse, item.FollowUpActionByCSO);
                    newRecord[48] = (double)GetListValue(MetricsKeys.FollowUpAction_Referral, item.FollowUpActionByCSO);
                    newRecord[49] = (double)GetListValue(MetricsKeys.FollowUpAction_LivelihoodSocialWelfare, item.FollowUpActionByCSO);
                    newRecord[50] = (double)GetListValue(MetricsKeys.FollowUpAction_Other, item.FollowUpActionByCSO);

                    newRecord[51] = (double)GetListValue(MetricsKeys.TypeOfService_Servicesofthepolice, item.ActualReferralServiceReceived);
                    newRecord[52] = (double)GetListValue(MetricsKeys.TypeOfService_Legalassistance, item.ActualReferralServiceReceived);
                    newRecord[53] = (double)GetListValue(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, item.ActualReferralServiceReceived);
                    newRecord[54] = (double)GetListValue(MetricsKeys.TypeOfService_SafeHouse, item.ActualReferralServiceReceived);
                    newRecord[55] = (double)GetListValue(MetricsKeys.TypeOfService_PsychosocialCounselling, item.ActualReferralServiceReceived);
                    newRecord[56] = (double)GetListValue(MetricsKeys.TypeOfService_MedicalHealth, item.ActualReferralServiceReceived);
                    newRecord[57] = (double)GetListValue(MetricsKeys.TypeOfService_Other, item.ActualReferralServiceReceived);

                    newRecord[58] = (double)GetListValue(MetricsKeys.TypeOfService_Servicesofthepolice, item.TypeOfReferralServiceRequired);
                    newRecord[59] = (double)GetListValue(MetricsKeys.TypeOfService_Legalassistance, item.TypeOfReferralServiceRequired);
                    newRecord[60] = (double)GetListValue(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, item.TypeOfReferralServiceRequired);
                    newRecord[61] = (double)GetListValue(MetricsKeys.TypeOfService_SafeHouse, item.TypeOfReferralServiceRequired);
                    newRecord[62] = (double)GetListValue(MetricsKeys.TypeOfService_PsychosocialCounselling, item.TypeOfReferralServiceRequired);
                    newRecord[63] = (double)GetListValue(MetricsKeys.TypeOfService_MedicalHealth, item.TypeOfReferralServiceRequired);
                    newRecord[64] = (double)GetListValue(MetricsKeys.TypeOfService_Other, item.TypeOfReferralServiceRequired);

                    newRecord[65] = (double)GetListValue(MetricsKeys.TypeOfService_Servicesofthepolice, item.TypeOfServiceProvidedToSurvior);
                    newRecord[66] = (double)GetListValue(MetricsKeys.TypeOfService_Legalassistance, item.TypeOfServiceProvidedToSurvior);
                    newRecord[67] = (double)GetListValue(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, item.TypeOfServiceProvidedToSurvior);
                    newRecord[68] = (double)GetListValue(MetricsKeys.TypeOfService_SafeHouse, item.TypeOfServiceProvidedToSurvior);
                    newRecord[69] = (double)GetListValue(MetricsKeys.TypeOfService_PsychosocialCounselling, item.TypeOfServiceProvidedToSurvior);
                    newRecord[70] = (double)GetListValue(MetricsKeys.TypeOfService_MedicalHealth, item.TypeOfServiceProvidedToSurvior);
                    newRecord[71] = (double)GetListValue(MetricsKeys.TypeOfService_Other, item.TypeOfServiceProvidedToSurvior);

                    newRecord[72] = (double)GetListValue(MetricsKeys.TypeOfService_Servicesofthepolice, item.TypeOfServiceReceivedBySurvior);
                    newRecord[73] = (double)GetListValue(MetricsKeys.TypeOfService_Legalassistance, item.TypeOfServiceReceivedBySurvior);
                    newRecord[74] = (double)GetListValue(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, item.TypeOfServiceReceivedBySurvior);
                    newRecord[75] = (double)GetListValue(MetricsKeys.TypeOfService_SafeHouse, item.TypeOfServiceReceivedBySurvior);
                    newRecord[76] = (double)GetListValue(MetricsKeys.TypeOfService_PsychosocialCounselling, item.TypeOfServiceReceivedBySurvior);
                    newRecord[77] = (double)GetListValue(MetricsKeys.TypeOfService_MedicalHealth, item.TypeOfServiceReceivedBySurvior);
                    newRecord[78] = (double)GetListValue(MetricsKeys.TypeOfService_Other, item.TypeOfServiceReceivedBySurvior);

                    newRecord[79] = string.IsNullOrWhiteSpace(item.OutcomeOfServiceorReferral) ? -1.0 : (double)GetValueFromDictionary(Field.ServiceOutcome.ToString(), item.OutcomeOfServiceorReferral, dictionary);
                    newRecord[80] = (item.IsApproved) ? (double)YesOrNo.Yes : (double)YesOrNo.No;
                    newRecord[81] = item.ApprovedByName;
                    newRecord[82] = item.ApprovedAt.HasValue ? item.ApprovedAt?.ToString("dd/MM/yyyy") : "";
                    newRecord[83] = string.IsNullOrWhiteSpace(item.WhoClosedTheCase) ? -1.0 : GetValueFromDictionary(Field.WhoClosedTheCase.ToString(), item.WhoClosedTheCase, dictionary);
                    writer.WriteRecord(newRecord);
                }

                writer.EndFile();
            }

            //read the saved default sav file
            using FileStream fileStream1 = new FileStream(filename, FileMode.Open, FileAccess.Read,
                FileShare.Read, 2048 * 10, FileOptions.SequentialScan);

            byte[] bytes = new byte[fileStream1.Length];
            await fileStream1.ReadAsync(bytes, 0, (int)fileStream1.Length);

            var stream = new MemoryStream();
            await stream.WriteAsync(bytes, 0, (int)fileStream1.Length);

            //set stream position to the beginning
            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// set the SPSS variables from code dictionary
        /// </summary>
        /// <param name="CodeDictionary"></param>
        /// <returns></returns>
        private List<Variable> SetSPSSVariables(Dictionary<string, Dictionary<int, string>> CodeDictionary)
        {
            //sets up the variables for the Spss document
            var variables = new List<Variable>
                            {
                                new Variable("Incident_Code")
                                {
                                    Label = "IncidentCode",
                                    Type = DataType.Text,
                                    TextWidth = 500,
                                },
                                new Variable(MetricsKeys.OrganisationType)
                                {
                                    Label = nameof(OrganisationType),
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.OrganisationType, CodeDictionary),

                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                new Variable(nameof(Organisation))
                                {
                                    Label = nameof(Organisation),
                                    Type = DataType.Text,
                                    TextWidth = 1000,
                                },
                                new Variable(nameof(State))
                                {
                                    Label = nameof(State),
                                    ValueLabels = GetDictionaryByKey(nameof(State), CodeDictionary),

                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable(MetricsKeys.LGA)
                                {
                                    Label = MetricsKeys.LGA,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.LGA, CodeDictionary),

                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                new Variable(nameof(Ward))
                                {
                                    Label = nameof(Ward),
                                    ValueLabels = GetDictionaryByKey(nameof(Ward), CodeDictionary),

                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable("Created_By")
                                {
                                    Label = "Created By",
                                    Type = DataType.Text,
                                    TextWidth = 1000,
                                },

                                new Variable(MetricsKeys.IncidentType)
                                {
                                    Label = MetricsKeys.IncidentType,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.IncidentType, CodeDictionary),

                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable("Actual_Location_of_Incident")
                                {
                                    Label = Field.IncidentLocation.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.IncidentLocation.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Age_of_survivor")
                                {
                                    Label = "Age of survivor",
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                 new Variable("Age_of_Perpetrator")
                                {
                                    Label = "Age of Perpetrator",
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                  new Variable("Contact_Channel")
                                {
                                    Label = Field.ContactChannel.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.ContactChannel.ToString(),CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                 new Variable("Date_of_Incident")
                                {
                                    Label = "Date_of_Incident",
                                    Type = DataType.Text,
                                    TextWidth = 300,
                                },
                                  new Variable("Date_Reported")
                                {
                                    Label = "Date_Reported",
                                    Type = DataType.Text,
                                    TextWidth = 300,
                                },

                                new Variable(MetricsKeys.TimeOfDay)
                                {
                                    Label = MetricsKeys.TimeOfDay,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TimeOfDay,CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                new Variable(MetricsKeys.DoestheSurviorLiveAlone)
                                {
                                    Label = MetricsKeys.DoestheSurviorLiveAlone,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.DoestheSurviorLiveAlone,CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Who_does_Survior_live_with")
                                {
                                    Label = Field.LiveWith.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.LiveWith.ToString(),CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable( MetricsKeys.DoestheSurviorWantJustice)
                                {
                                    Label = MetricsKeys.DoestheSurviorWantJustice,
                                    ValueLabels = GetDictionaryByKey( MetricsKeys.DoestheSurviorWantJustice,CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                  new Variable( MetricsKeys.HasSurviorReceivedService)
                                {
                                    Label =MetricsKeys.HasSurviorReceivedService,
                                    ValueLabels = GetDictionaryByKey( MetricsKeys.HasSurviorReceivedService,CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                 new Variable("Education_of_survivor")
                                {
                                    Label = Field.Education.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.Education.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                new Variable("Employment_Status")
                                {
                                    Label = Field.EmploymentStatus.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.EmploymentStatus.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                 new Variable("Employment_Status_of_Parent_Or_Guardian")
                                {
                                    Label = Field.EmploymentStatus.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.EmploymentStatus.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable(MetricsKeys.IsSurviorContinousTreat)
                                {
                                    Label = MetricsKeys.IsSurviorContinousTreat,
                                    ValueLabels = GetDictionaryByKey( MetricsKeys.IsSurviorContinousTreat, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable( "Martial_Status")
                                {
                                    Label = Field.MaritalStatus.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.MaritalStatus.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable( "Sex_of_survivor")
                                {
                                    Label = Field.Sex.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.Sex.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable( "Sex_of_Perpetrator")
                                {
                                    Label = Field.Sex.ToString(),
                                    ValueLabels = GetDictionaryByKey( Field.Sex.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                      new Variable( "Number_of_Perpetrators")
                                {
                                    Label = "Number_of_Perpetrators",

                                    //PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                  //  WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Text,
                                    TextWidth = 200,
                                   // MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                //  new Variable( "Vulnerable_Population")
                                //{
                                //    Label = Field.VulnerablePopulation.ToString(),
                                //    ValueLabels = GetDictionaryByKey( Field.VulnerablePopulation.ToString(), CodeDictionary),
                                //    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                //    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                //    Type = DataType.Numeric,
                                //    Width = 10,
                                //    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                //},

                                    new Variable( "Vulnerable_Population_PLHIV")
                                {
                                    Label ="Vulnerable_Population_PLHIV",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_PLHIV, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable( "Vulnerable_Population_PLWD")
                                {
                                    Label ="Vulnerable_Population_PLWD",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_PLWD, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                     new Variable( "Vulnerable_Population_FSW")
                                {
                                    Label ="Vulnerable_Population_FSW",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_FSW, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                     new Variable( "Vulnerable_Population_DrugUser")
                                {
                                    Label ="Vulnerable_Population_DrugUser",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_DrugUser, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                     new Variable( "Vulnerable_Population_IDP")
                                {
                                    Label ="Vulnerable_Population_IDP",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_IDP, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                      new Variable( "Vulnerable_Population_OutSchoolChil")
                                {
                                    Label ="Vulnerable_Population_OutSchoolChil",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_OutSchoolChil, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },


                                      new Variable( "Vulnerable_Population_Minor")
                                {
                                    Label ="Vulnerable_Population_Minor",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_Minor, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                       new Variable( "Vulnerable_Population_Widow")
                                {
                                    Label ="Vulnerable_Population_Widow",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_Widow, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                       new Variable( "Vulnerable_Population_MaidDomesticStaff")
                                {
                                    Label ="Vulnerable_Population_MaidDomesticStaff",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_MaidDomesticStaff, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                        new Variable( "Vulnerable_Population_NotApplicable")
                                {
                                    Label ="Vulnerable_Population_NotApplicable",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_NotApplicable, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                       new Variable( "Vulnerable_Population_Orphans")
                                {
                                    Label ="Vulnerable_Population_Orphans",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_Orphans, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                         new Variable( "Vulnerable_Population_ChildApprentice")
                                {
                                    Label ="Vulnerable_Population_ChildApprentice",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.VulnerablePopulation_ChildApprentice, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                     new Variable(  MetricsKeys.WasViolenceFatal)
                                {
                                    Label =  MetricsKeys.WasViolenceFatal,
                                    ValueLabels = GetDictionaryByKey(  MetricsKeys.WasViolenceFatal, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                  new Variable( "Who_Reported_the_Incident")
                                {
                                    Label = Field.WhoReported.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.WhoReported.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable( "Survivor_Relationship_with_Perpetrator")
                                {
                                    Label = Field.Relationship.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.Relationship.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                     new Variable("GBV_COVIDI9_Question1")
                                {
                                    Label = "GBV_COVIDI9_Question1",
                                    Type = DataType.Text,
                                    TextWidth = 15000,
                                },
                                     new Variable("GBV_COVIDI9_Question2")
                                {
                                    Label = "GBV_COVIDI9_Question2",
                                    Type = DataType.Text,
                                   TextWidth = 10000,
                                },
                                   new Variable("GBV_COVIDI9_Question3")
                                {
                                    Label = "GBV_COVIDI9_Question3",
                                    Type = DataType.Text,
                                    TextWidth = 10000,
                                },
                                    new Variable("GBV_COVIDI9_Question4")
                                {
                                    Label = "GBV_COVIDI9_Question4",
                                    Type = DataType.Text,
                                    TextWidth = 10000,
                                },
                                     new Variable( MetricsKeys.HasCaseBeenClosed)
                                {
                                    Label =MetricsKeys.HasCaseBeenClosed,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.HasCaseBeenClosed, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                 new Variable("Name_of_Service_Provider_Referred_To")
                                {
                                    Label = "Name_of_Service_Provider_Referred_To",
                                    Type = DataType.Text,
                                    TextWidth = 1000,
                                },
                                 new Variable( "FollowUpAction_SafeHouse_Shelter")
                                {
                                    Label ="FollowUpAction_SafeHouse_Shelter",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.FollowUpAction_SafeHouse, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable(MetricsKeys.FollowUpAction_Referral)
                                {
                                    Label =MetricsKeys.FollowUpAction_Referral,
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.FollowUpAction_Referral, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("FollowUpAction_LivelihoodSocialWelfare")
                                {
                                    Label ="FollowUpAction_LivelihoodSocialWelfare",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.FollowUpAction_LivelihoodSocialWelfare, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("FollowUpAction_Other")
                                {
                                    Label ="FollowUpAction_Other",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.FollowUpAction_Other, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                               new Variable("Actual_Referral_Service_Received_Police")
                                {
                                    Label ="Actual_Referral_Service_Received_Police",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Servicesofthepolice, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                      new Variable("Actual_Referral_Service_Received_Legal_asistance")
                                {
                                    Label ="Actual_Referral_Service_Received_Legal_asistance",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Legalassistance, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Actual_Referral_Service_Received_Livelihood_Social_Welfare_Program")
                                {
                                    Label ="Actual_Referral_Service_Received_Livelihood_Social_Welfare_Program",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Actual_Referral_Service_Received_Safe_house_shelter")
                                {
                                    Label ="Actual_Referral_Service_Received_Safe_house_shelter",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_SafeHouse, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Actual_Referral_Service_Received_Psychosocial_counselling")
                                {
                                    Label ="Actual_Referral_Service_Received_Psychosocial_counselling",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_PsychosocialCounselling, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Actual_Referral_Service_Received_Medical_Health_services")
                                {
                                    Label ="Actual_Referral_Service_Received_Medical_Health_services",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_MedicalHealth, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Actual_Referral_Service_Received_Other")
                                {
                                    Label ="Actual_Referral_Service_Received_Other",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Other, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                   new Variable("Type_of_Referral_Service_Required_Police")
                                {
                                    Label ="Type_of_Referral_Service_Required_Police",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Servicesofthepolice, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                      new Variable("Type_of_Referral_Service_Required_Legal_asistance")
                                {
                                    Label ="Type_of_Referral_Service_Required_Legal_asistance",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Legalassistance, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Referral_Service_Required_Livelihood_Social_Welfare_Program")
                                {
                                    Label ="Type_of_Referral_Service_Required_Livelihood_Social_Welfare_Program",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Referral_Service_Required_Safe_house_shelter")
                                {
                                    Label ="Type_of_Referral_Service_Required_Safe_house_shelter",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_SafeHouse, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Type_of_Referral_Service_Required_Psychosocial_counselling")
                                {
                                    Label ="Type_of_Referral_Service_Required_Psychosocial_counselling",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_PsychosocialCounselling, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Type_of_Referral_Service_Required_Medical_Health_services")
                                {
                                    Label ="Type_of_Referral_Service_Required_Medical_Health_services",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_MedicalHealth, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Referral_Service_Required_Other")
                                {
                                    Label ="Type_of_Referral_Service_Required_Other",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Other, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                    new Variable("Type_of_Service_Provided_to_Survior_Police")
                                {
                                    Label ="Type_of_Service_Provided_to_Survior_Police",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Servicesofthepolice, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Service_Provided_to_Survior_Legal_asistance")
                                {
                                    Label = "Type_of_Service_Provided_to_Survior_Legal_asistance",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Legalassistance, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Service_Provided_to_Survior_Livelihood_Social_Welfare_Program")
                                {
                                    Label ="Type_of_Service_Provided_to_Survior_Livelihood_Social_Welfare_Program",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Service_Provided_to_Survior_Safe_house_shelter")
                                {
                                    Label = "Type_of_Service_Provided_to_Survior_Safe_house_shelter",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_SafeHouse, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Service_Provided_to_Survior_Psychosocial_counselling")
                                {
                                    Label ="Type_of_Service_Provided_to_Survior_Psychosocial_counselling",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_PsychosocialCounselling, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Type_of_Service_Provided_to_Survior_Medical_Health_services")
                                {
                                    Label ="Type_of_Service_Provided_to_Survior_Medical_Health_services",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_MedicalHealth, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Service_Provided_to_Survior_Other")
                                {
                                    Label ="Type_of_Service_Provided_to_Survior_Other",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Other, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                   new Variable("Type_of_Serviced_Received_By_Survior_Police")
                                {
                                    Label = "Type_of_Serviced_Received_By_Survior_Police",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Servicesofthepolice, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                      new Variable("Type_of_Serviced_Received_By_Survior_Legal_asistance")
                                {
                                    Label ="Type_of_Serviced_Received_By_Survior_Legal_asistance",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Legalassistance, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Serviced_Received_By_Survior_Livelihood_Social_Welfare_Program")
                                {
                                    Label ="Type_of_Serviced_Received_By_Survior_Livelihood_Social_Welfare_Program",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_LivelihoodSocialWelfare, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                     new Variable("Type_of_Serviced_Received_By_Survior_Safe_house_shelter")
                                {
                                    Label ="Type_of_Serviced_Received_By_Survior_Safe_house_shelter",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_SafeHouse, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                        new Variable("Type_of_Serviced_Received_By_Survior_Psychosocial_counselling")
                                {
                                    Label ="Type_of_Serviced_Received_By_Survior_Psychosocial_counselling",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_PsychosocialCounselling, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                   new Variable("Type_of_Serviced_Received_By_Survior_Medical_Health_services")
                                {
                                    Label = "Type of Serviced Received By Survior_Medical_Health services",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_MedicalHealth, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                    new Variable("Type_of_Serviced_Received_By_Survior_Other")
                                {
                                    Label ="Type_of_Serviced_Received_By_Survior_Other",
                                    ValueLabels = GetDictionaryByKey(MetricsKeys.TypeOfService_Other, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable("Outcome_of_Service_or_Referral")
                                {
                                    Label =Field.ServiceOutcome.ToString(),
                                    ValueLabels = GetDictionaryByKey(Field.ServiceOutcome.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                                 new Variable( MetricsKeys.IsApproved)
                                {
                                    Label = MetricsKeys.IsApproved,
                                    ValueLabels = GetDictionaryByKey( MetricsKeys.IsApproved, CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },

                                new Variable( "Approved_By")
                                {
                                    Label ="Approved_By",
                                    Type = DataType.Text,
                                    TextWidth = 700,
                                },
                                 new Variable( "Date_Approved")
                                {
                                    Label ="Date_Approved",
                                    Type = DataType.Text,
                                    TextWidth = 300,
                                },
                                  new Variable( "Who_Closed_the_Case")
                                {
                                    Label ="Who_Closed_the_Case",
                                    ValueLabels = GetDictionaryByKey(Field.WhoClosedTheCase.ToString(), CodeDictionary),
                                    PrintFormat = new OutputFormat(FormatType.F, 8, 2),
                                    WriteFormat = new OutputFormat(FormatType.F, 8, 2),
                                    Type = DataType.Numeric,
                                    Width = 10,
                                    MissingValueType = MissingValueType.OneDiscreteMissingValue
                                },
                             };

            return variables;
        }

        /// <summary>
        /// Create the code dictionary
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, Dictionary<int, string>>> GetCodeDictionary()
        {
            var entrys = await _context.Entries.AsNoTracking()
                                .OrderBy(c => c.Field)
                                .ToListAsync();

            var entries = entrys.OrderBy(c => int.Parse(c.Key)).GroupBy(c => c.Field).ToList();

            var categories = await _context.CaseCategories.AsNoTracking()
                             .OrderBy(c => c.Id).ToListAsync();

            var States = await _context.States.AsNoTracking().ToListAsync();

            var LGAS = await _context.LocalGovernmentAreas.AsNoTracking().ToListAsync();

            var Wards = await _context.Wards.AsNoTracking().ToListAsync();

            //Entry, Values, Key

            var codeDictionary = new Dictionary<string, Dictionary<int, string>>
            {
                //add organistion Type
                {
                    MetricsKeys.OrganisationType,
                    new Dictionary<int, string>
                        {
                            {  (int)OrganisationType.CSO, OrganisationType.CSO.ToString()},
                            {  (int)OrganisationType.ServiceProvider, OrganisationType.ServiceProvider.ToString() }
                        }
                },
                {
                    MetricsKeys.TimeOfDay,
                    new Dictionary<int, string>
                    {
                        { (int)TimeOfDay.Morning, TimeOfDay.Morning.ToString() },
                        {(int)TimeOfDay.Afternoon, TimeOfDay.Afternoon.ToString() },
                        {(int)TimeOfDay.Evening, TimeOfDay.Evening.ToString() },
                        {(int)TimeOfDay.Unknown, TimeOfDay.Unknown.ToString() }
                    }
                },
                {
                    MetricsKeys.WasViolenceFatal,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                  {
                    MetricsKeys.IsSurviorContinousTreat,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                {
                    MetricsKeys.HasSurviorReceivedService,
                    new Dictionary<int, string>
                   {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                  {
                    MetricsKeys.DoestheSurviorWantJustice,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                {
                    MetricsKeys.DoestheSurviorLiveAlone,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                  {
                    MetricsKeys.HasCaseBeenClosed,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() },
                        {(int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString() }
                    }
                },
                {
                    MetricsKeys.IsApproved,
                    new Dictionary<int, string>
                    {
                        {(int)YesOrNo.No, YesOrNo.No.ToString() },
                        {(int)YesOrNo.Yes, YesOrNo.Yes.ToString() }
                    }
                }
            };

            var dictionary = new Dictionary<int, string>();

            // add categories
            foreach (var cat in categories)
            {
                dictionary.Add(cat.Id, cat.Name);
            }
            dictionary.Add(0, "Other");

            codeDictionary.Add(MetricsKeys.IncidentType, dictionary);
            dictionary = new Dictionary<int, string>();

            //add entries
            foreach (var entry in entries)
            {
                var diction = new Dictionary<int, string>();
                foreach (var en in entry)
                {
                    if (en.Type == EntryType.Listed)
                    {
                        if (en.Field == Field.TypeOfService || en.Field == Field.FollowUpAction)
                        {
                            diction.Add((int)YesOrNo.No, YesOrNo.No.ToString());
                            diction.Add((int)YesOrNo.Yes, YesOrNo.Yes.ToString());
                            diction.Add((int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString());
                            codeDictionary.Add($"{en.Field}_{en.Value}", diction);
                            diction = new Dictionary<int, string>();
                        }
                        else
                        {
                            diction.Add(int.Parse(en.Key), en.Value);
                        }
                    }
                }

                if (entry.Key == Field.TypeOfService || entry.Key == Field.FollowUpAction)
                {
                    diction.Add((int)YesOrNo.No, YesOrNo.No.ToString());
                    diction.Add((int)YesOrNo.Yes, YesOrNo.Yes.ToString());
                    diction.Add((int)YesOrNo.NotApplicable, YesOrNo.NotApplicable.ToString());
                    codeDictionary.Add($"{entry.Key}_Other", diction);
                    diction = new Dictionary<int, string>();
                }
                else
                {
                    diction.Add((int)EntryType.Other, EntryType.Other.ToString());
                    codeDictionary.Add(entry.Key.ToString(), diction);
                }
            }

            //Add states
            foreach (var state in States.OrderBy(c => int.Parse(c.Key)))
            {
                dictionary.Add(int.Parse(state.Key), state.Name);
            }

            codeDictionary.Add(nameof(State), dictionary);
            dictionary = new Dictionary<int, string>();

            //Add LGA
            foreach (var lga in LGAS.OrderBy(c => int.Parse(c.Key)))
            {
                dictionary.Add(int.Parse(lga.Key), lga.Name);
            }

            codeDictionary.Add(MetricsKeys.LGA, dictionary);
            dictionary = new Dictionary<int, string>();

            //Add ward
            foreach (var ward in Wards.OrderBy(c => int.Parse(c.Key)))
            {
                dictionary.Add(int.Parse(ward.Key), ward.Name);
            }

            codeDictionary.Add(nameof(Ward), dictionary);
            dictionary = new Dictionary<int, string>();

            return codeDictionary;
        }

        /// <summary>
        /// Gets the int value of a data entry from the code dictionary
        /// </summary>
        /// <param name="Entry"></param>
        /// <param name="Value"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private int GetValueFromDictionary(string Entry, string Value, Dictionary<string, Dictionary<int, string>> dictionary)
        {
            var valueDictionary = dictionary.Where(c => c.Key.Trim().ToLower() == Entry.Trim().ToLower())
                                 .Select(c => c.Value).FirstOrDefault()
                                 .Where(d => d.Value.Trim().ToLower() == Value.Trim().ToLower())
                                 .FirstOrDefault();

            return valueDictionary.Key;
        }

        /// <summary>
        /// Gets the Dictionary key value pair
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private IDictionary<double, string> GetDictionaryByKey(string Key, Dictionary<string, Dictionary<int, string>> dictionary)
        {
            var valueDictionary = dictionary.Where(c => c.Key.Trim().ToLower() == Key.Trim().ToLower())
                                .Select(c => c.Value).FirstOrDefault();

            var Idictionary = new Dictionary<double, string>();

            foreach (var value in valueDictionary)
            {
                Idictionary.Add(value.Key, value.Value);
            }
            Idictionary.Add(-1.0, "Empty/Null");
            return Idictionary;
        }

        /// <summary>
        ///Gets list value for data for excel and Spss exports
        ///takes in the Entry type and list of values then one integer value is selected either Yes or No
        /// </summary>
        /// <param name="Entry"></param>
        /// <param name="Values"></param>
        /// <returns></returns>

        private int GetListValue(string Entry, List<string> Values)
        {
            if (Values is null)
            {
                return (int)YesOrNo.NotApplicable;
            }
            else if (Values.Count <= 0)
            {
                return (int)YesOrNo.NotApplicable;
            }
            var list = new List<string>
            {
                MetricsKeys.VulnerablePopulation_PLHIV,
                MetricsKeys.VulnerablePopulation_PLWD,
                MetricsKeys.VulnerablePopulation_FSW,
                MetricsKeys.VulnerablePopulation_DrugUser,
                MetricsKeys.VulnerablePopulation_IDP,
                MetricsKeys.VulnerablePopulation_OutSchoolChil,
                MetricsKeys.VulnerablePopulation_Minor,
                MetricsKeys.VulnerablePopulation_Widow,
                MetricsKeys.VulnerablePopulation_MaidDomesticStaff,
                MetricsKeys.VulnerablePopulation_NotApplicable,
                MetricsKeys.VulnerablePopulation_Orphans,
                MetricsKeys.VulnerablePopulation_ChildApprentice,
                MetricsKeys.FollowUpAction_LivelihoodSocialWelfare,
                MetricsKeys.FollowUpAction_Referral,
                MetricsKeys.FollowUpAction_SafeHouse,
                MetricsKeys.TypeOfService_Legalassistance,
                MetricsKeys.TypeOfService_LivelihoodSocialWelfare,
                MetricsKeys.TypeOfService_MedicalHealth,
                MetricsKeys.TypeOfService_PsychosocialCounselling,
                MetricsKeys.TypeOfService_SafeHouse,
                MetricsKeys.TypeOfService_Servicesofthepolice,

            };
            if (!Entry.EndsWith("Other"))
            {
                var items = Values.Any(c => Entry.Trim().EndsWith(c.Trim()));
                if (!items)
                {
                    return (int)YesOrNo.No;
                }

                return (int)YesOrNo.Yes;
            }
            else
            {
                foreach (var item in Values)
                {
                    var value = list.Any(c => c.Trim().EndsWith(item.Trim()));

                    if (!value)
                    {
                        return (int)YesOrNo.Yes;
                    }
                }
            }

            return (int)YesOrNo.No;
        }

        /// <summary>
        /// Get Cases based on the Search Model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<List<CaseViewModel>> GetAllReportCases(CaseSearchModel model)
        {
            if (UserId is null)
            {
                return new List<CaseViewModel>();
            }

            var User = await _context.Users.AsNoTracking()
                .Select(c => new UserViewModel
                {
                    Designation = c.Designation,
                    Role = c.Type,
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastLogin = c.LastLogin,
                    Id = c.Id,
                    LastName = c.LastName,
                    Organisation = c.OrganisationId.HasValue ? c.Organisation.Name : null,
                    OrganisationId = c.OrganisationId,
                    PhoneNumber = c.PhoneNumber,
                    StateId = c.StateId,
                    State = c.StateId.HasValue ? c.State.Name : null,
                    StateCode = c.StateId.HasValue ? c.State.Code : null,
                    IsActivated = c.IsActivated,
                    LocalGovernments = c.LocalGovernments
                    //LocalGovernmentId = c.LocalGovernmentAreaId
                }).FirstOrDefaultAsync(c => c.Id == UserId);

            if (User is null)
            {
                return new List<CaseViewModel>();
            }

            var query = _context.Cases
                .Include(c => c.CreatedBy)
                .Include(c => c.FollowUpActions)
                .AsNoTracking();

            //this was added bcos deleted users data are missen in data export
            var allUserInCases = _context.Users.Where(u => query.Select(c => c.CreatedBy).Contains(u))
                .IgnoreQueryFilters();

            //apply filters
            //if (User.Role == RoleKeys.FederalSupervisor || User.Role == RoleKeys.StateAdministrator || User.Role == RoleKeys.StateSupervisor)
            //{
            //    query = query.Where(c => c.IsValidated);
            //}

            if (model.StartDate.HasValue)
            {
                query = query.Where(c => c.DateReported >= model.StartDate);
            }

            if (model.EndDate.HasValue)
            {
                query = query.Where(c => c.DateReported <= model.EndDate);
            }
            if (model.OrganisationId.HasValue && model.OrganisationId.Value > 0)
            {
                query = query.Where(c => c.OrganisationId == model.OrganisationId);
            }
            if (model.StateId.HasValue && model.StateId.Value > 0)
            {
                query = query.Where(c => c.IncidentStateId == model.StateId || !string.IsNullOrWhiteSpace(User.StateCode) && c.IncidentCode.ToLower().Contains(User.StateCode.ToLower()));
            }

            //if (User.LocalGovernmentId.HasValue && User.Role == RoleKeys.LocalGovernment)
            //{
            //    query = query.Where(c => c.IncidentLGAId == User.LocalGovernmentId);
            //}

            //check is user is a local government access
            if (User.LocalGovernments.Any() && User.Role == RoleKeys.LocalGovernment)
            {
                var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
                query = query.Where(c => userLgas.Contains("," + c.IncidentLGAId + ",") || c.CreatedById == User.Id);
            }

            if (model.IncidentLGAId.HasValue && model.IncidentLGAId.Value > 0)
            {
                var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
                query = query.Where(c => c.IncidentLGAId == model.IncidentLGAId || userLgas.Contains("," + c.IncidentLGAId + ","));
            }

            if (model.TimeOfDay.HasValue)
            {
                query = query.Where(c => c.TimeOfDay == model.TimeOfDay);
            }

            //if (model.CaseCategoryId.HasValue && model.CaseCategoryId.Value > 0)
            //{
            //    query = query.Where(c => c.CaseCategoryId == model.CaseCategoryId);
            //}

            if (!string.IsNullOrWhiteSpace(model.IncidentCode))
            {
                query = query.Where(c => c.IncidentCode.ToLower() == model.IncidentCode.ToLower());
            }

            if (model.IsCaseClosed.HasValue && model.IsCaseClosed.Value != YesOrNo.NotApplicable)
            {
                query = query.Where(c => c.HasCaseBeenClosed == model.IsCaseClosed);
            }
            if (model.IsApproved.HasValue)
            {
                query = query.Where(c => c.IsApproved == model.IsApproved.Value);
            }

            if (model.IsValidated.HasValue)
            {
                query = query.Where(c => c.IsValidated == model.IsValidated.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.Gender))
            {
                query = query.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
            }

            if (model.MinimumAge.HasValue)
            {
                query = query.Where(c => c.AgeOfSurvior >= model.MinimumAge);
            }
            if (model.MaximumAge.HasValue)
            {
                query = query.Where(c => c.AgeOfSurvior <= model.MaximumAge);
            }

            if (!string.IsNullOrWhiteSpace(model.TypeOfServiceProvided))
            {
                query = query.Where(c => c.TypeOfServiceProvidedToSurvior.ToLower().Contains(model.TypeOfServiceProvided.Trim().ToLower()));
            }

            //query = query.Where(c => c.CreatedById == "75fc49a8-d98e-4eae-ae84-63cf98d66e81" && c.IncidentCode == "LA/3401/0819");
            var LGAS = _context.LocalGovernmentAreas.AsNoTracking();
            var Cases = await query.Select(c => new CaseViewModel
            {
                Id = c.Id,
                CreatedAt = c.CreatedAt,
                IncidentCode = c.IncidentCode,
                ActualLocationOfIncident = c.ActualLocationOfIncident,
                //ActualReferralServiceReceived = string.IsNullOrEmpty(c.ActualReferralServiceReceived) ? null : JsonConvert.DeserializeObject<List<string>>(c.ActualReferralServiceReceived),
                AgeOfSurvior = c.AgeOfSurvior,
                CanBeEdited = c.CanBeEdited,
                Category = c.Category.Name,
                ContactChannel = c.ContactChannel,
                CreatedById = c.CreatedById,

                CreatedByName = c == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).LastName}" : $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}",
                DateOfIncident = c.DateOfIncident,
                DateReported = c.DateReported,
                DoesSurviorLiveAlone = c.DoesSurviorLiveAlone,
                DoestheSurviorWantJustice = c.DoestheSurviorWantJustice,
                Education = c.Education,
                EmploymentStatus = c.EmploymentStatus,
                EmploymentStatusOfParentOrGuardian = c.EmploymentStatusOfParentOrGuardian,
                //FollowUpActionByCSO = string.IsNullOrEmpty(c.FollowUpActionByCSO) ? null : JsonConvert.DeserializeObject<List<string>>(c.FollowUpActionByCSO),
                //GBV_COVID19_Question1 = c.GBV_COVID19_Question1,
                //GBV_COVID19_Question2 = c.GBV_COVID19_Question2,
                //GBV_COVID19_Question3 = c.GBV_COVID19_Question3,
                //GBV_COVID19_Question4 = c.GBV_COVID19_Question4,
                HasCaseBeenClosed = c.HasCaseBeenClosed,
                HasSurviorReceivedService = c.HasSurviorReceivedService,

                IncidentLGA = c.IncidentLGA.Name,
                IncidentLGAId = c.IncidentLGAId,
                IncidentState = c.IncidentState.Name,
                IncidentStateId = c.IncidentStateId,
                //complex check if incident ward is unknown
                IncidentWardId = !c.IncidentWardId.HasValue ? c.IncidentWardId : !_configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.DevUnknown ? 0 : _configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.LiveUnknown ? 0 : c.IncidentWardId,
                IncidentWard = c.IncidentWardId.HasValue ? c.IncidentWard.Name : null,
                IsSurviorContinuousThreat = c.IsSurviorContinuousThreat,
                MaritalStatus = c.MaritalStatus,
                //NameOfServiceProviderReferredTo = c.NameOfServiceProviderReferredTo,
                NumberOfPerpetrators = c.NumberOfPerpetrators,
                Organisation = c.Organisation.Name,
                OrganisationLGA = LGAS.FirstOrDefault(x => x.Id == c.OrganisationLgaId).Name,
                //OrganisationLgaId = c.OrganisationLgaId,
                OrganisationId = c.OrganisationId,
                OrganisationType = c.Organisation.OrganisationType,
                //OutcomeOfServiceorReferral = c.OutcomeOfServiceorReferral,
                SerialNumber = c.SerialNumber,
                SexOfSurvior = c.SexOfSurvior,
                TimeOfDay = c.TimeOfDay,
                PerpetratorsInformationList = c.PerpetratorsInformationList,
                //TypeOfReferralServiceRequired = string.IsNullOrEmpty(c.TypeOfReferralServiceRequired) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfReferralServiceRequired),
                //TypeOfServiceProvidedToSurvior = string.IsNullOrEmpty(c.TypeOfServiceProvidedToSurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvidedToSurvior),
                //TypeOfServiceReceivedBySurvior = string.IsNullOrEmpty(c.TypeOfServiceReceivedBySurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceReceivedBySurvior),
                VulnerablePopulation = string.IsNullOrWhiteSpace(c.VulnerablePopulation) ? null : JsonConvert.DeserializeObject<List<string>>(c.VulnerablePopulation),
                WasViolenceFatal = c.WasViolenceFatal,
                WhoDoesSurviorLiveWith = c.WhoDoesSurviorLiveWith,
                WhoReportedIncident = c.WhoReportedIncident,
                IsApproved = c.IsApproved,
                IsValidated = c.IsValidated,
                ValidatedAt = c.ValidatedAt,
                ApprovedAt = c.ApprovedAt,
                ValidatedById = c.ValidatedById,
                ValidatedByName = string.IsNullOrEmpty(c.ValidatedById) ? null : c.ValidatedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).LastName}" : $"{c.ValidatedBy.FirstName} {c.ValidatedBy.LastName}",
                ApprovedById = c.ApprovedById,
                ApprovedByName = string.IsNullOrEmpty(c.ApprovedById) ? null : c.ApprovedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).LastName}" : $"{c.ApprovedBy.FirstName} {c.ApprovedBy.LastName}",
                WhoClosedTheCase = c.WhoClosedTheCase,
                //Latitude = c.Latitude,
                //Longitude = c.Longitude,
                UserState = c.CreatedBy.State.Name,
                ReferralOutcome = c.ReferralOutcome,
                CaseClosedDate = c.CaseClosedDate,
                DateJusticeReceived = c.DateJusticeReceived,

                FollowUps = c.FollowUpActions.Select(f => new FollowUpViewModel
                {
                    FinalOutcome = f.FinalOutcome,
                    CaseClosedDate = f.CaseClosedDate.GetValueOrDefault(),
                    HasClientReceivedjustice = f.HasClientReceivedjustice,
                    JusticeReceivedDate = f.JusticeReceivedDate,
                    HasCaseBeenClosed = f.HasCaseBeenClosed,
                    WhoClosedTheCase = f.WhoClosedTheCase
                }),
                LgaValidatedAt = c.LgaValidatedAt.GetValueOrDefault(),
                LgaValidatedByName = c.LgaValidatedBy.FirstName + " " + c.LgaValidatedBy.LastName,
                CaseCategoriesOthers = c.CaseCategoriesOthers,
                CaseCategories = c.CaseCategoriesList,
                OutcomeOfProsecution = c.OutcomeOfProsecution
            }).OrderBy(c => c.CreatedAt).AsSplitQuery().ToListAsync();

            return Cases;
        }

        /// <summary>
        /// function to Get Case By Age Group By Sex
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        private List<CaseBySubjectBySex> GetCaseByAgeGroupBySex(IEnumerable<AdminDashboardProjectModel> cases, bool? doestheSurviorWantJustice = null)
        {
            if (doestheSurviorWantJustice.HasValue)
            {
                var yesOrNo = doestheSurviorWantJustice.Value ? YesOrNo.Yes : YesOrNo.No;
                cases = cases.Where(c => c.DoestheSurviorWantJustice == yesOrNo).ToList();
            }

            var ageRanges = new List<(int? Min, int? Max)>()
            {
                {(0 , 4)},
                {(5 ,9)},
                {(10, 14)},
                {(15 , 19)},
                {(20 , 24)},
                {(25 ,29)},
                {(30 , 34)},
                {(35 , 39)},
                {(40 , 45)},
                {(46 ,49)},
                {(50, 1000)}
            };

            return GroupCaseByAgeBySex(ageRanges, cases);
        }

        private List<CaseBySubjectBySex> GroupCaseByAgeBySex(List<(int? Min, int? Max)> ageRanges, IEnumerable<AdminDashboardProjectModel> cases)
        {
            var casesByAgeGroupBySex = new List<CaseBySubjectBySex>();

            foreach (var range in ageRanges)
            {
                var rangeCases = cases
                    .Where(c => c.AgeOfSurvior >= range.Min && c.AgeOfSurvior <= range.Max)
                    .Select(c => new Case
                    {
                        SexOfSurvior = c.SexOfSurvior,
                        HasSurviorReceivedService = c.HasSurviorReceivedService
                    }).ToList();

                casesByAgeGroupBySex.Add(new CaseBySubjectBySex
                {
                    Subject = range.Max >= 50 ? "50+" : $"{range.Min} - {range.Max}",
                    FemaleCount = rangeCases.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    MaleCount = rangeCases.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    OtherCount = rangeCases.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    TotalNewCases = rangeCases.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    TotalFollowUpCases = rangeCases.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                });
            }

            return casesByAgeGroupBySex;
        }

        private (List<CaseBySubject>, List<CaseBySubjectBySex>) GetCaseByTypeOfService(IEnumerable<Case> cases)
        {
            return GetCaseByTypeOfService(_mapper.Map<List<AdminDashboardProjectModel>>(cases));
        }

        /// <summary>
        /// function to GetCaseByType of Service and by Sex
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        private (List<CaseBySubject>, List<CaseBySubjectBySex>) GetCaseByTypeOfService(IEnumerable<AdminDashboardProjectModel> cases)
        {
            //List of service provider options from string keys
            var list = new List<string>
            {
                MetricsKeys.TypeOfService_Legalassistance,
                MetricsKeys.TypeOfService_LivelihoodSocialWelfare,
                MetricsKeys.TypeOfService_MedicalHealth,
                MetricsKeys.TypeOfService_PsychosocialCounselling,
                MetricsKeys.TypeOfService_SafeHouse,
                MetricsKeys.TypeOfService_Servicesofthepolice,
                MetricsKeys.TypeOfService_Referral,
                MetricsKeys.TypeOfService_FinancialAssistanceMedical,
                MetricsKeys.TypeOfService_FinancialAssistanceLegal,
                MetricsKeys.TypeOfService_FinancialAssistanceSecurity,
            };

            var typeOfServiceAndSex = cases.Where(c => !string.IsNullOrWhiteSpace(c.TypeOfServiceProvidedToSurvior))
                .Select(c => new Case
                {
                    TypeOfServiceProvidedToSurvior = c.TypeOfServiceProvidedToSurvior,
                    SexOfSurvior = c.SexOfSurvior,
                    HasSurviorReceivedService = c.HasSurviorReceivedService
                }).ToList();

            var serviceBySex = new List<(string, string, YesOrNo)>();

            //loops through the service types to pull the required data if its contained
            foreach (var caseObject in typeOfServiceAndSex)
            {
                //filters the list replaces those not listed with "Other"
                if (!string.IsNullOrWhiteSpace(caseObject.TypeOfServiceProvidedToSurvior))
                {
                    foreach (var service in JsonConvert.DeserializeObject<List<string>>(caseObject.TypeOfServiceProvidedToSurvior))
                    {
                        var value = list.Any(c => c.Trim().ToLower().EndsWith(service.Trim().ToLower()));

                        if (!value)
                        {
                            //replace with Other if not found in list
                            serviceBySex.Add(("Other", caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                        }
                        else
                        {
                            serviceBySex.Add((service, caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                        }
                    }
                }
            }

            var group = serviceBySex.GroupBy(c => c.Item1).ToList();

            //groups the services by common string
            var caseByTypeofService = group.Select(p => new CaseBySubject
            {
                Id = 0,
                Name = p.Key,
                Count = p.Count()
            }).ToList();

            //groups the services by sex here item2 is Sex , item1 is typeofService
            var caseByTypeofServiceBySex = group.Select(p => new CaseBySubjectBySex
            {
                Subject = p.Key,
                FemaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Female),
                MaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Male),
                OtherCount = p.Count(c =>
                    c.Item2.ToLower() != MetricsKeys.Male && c.Item2.ToLower() != MetricsKeys.Female),
                TotalNewCases = p.Count(c => c.Item3 == YesOrNo.No),
                TotalFollowUpCases = p.Count(c => c.Item3 == YesOrNo.Yes)
            }).ToList();

            { //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
              //add empty rows
                foreach (var item in list)
                {
                    var str = item.Replace("TypeOfService_", "");
                    if (!caseByTypeofServiceBySex.Any(c => c.Subject.ToLower() == str.ToLower()))
                    {
                        caseByTypeofServiceBySex.Add(new CaseBySubjectBySex
                        {
                            Subject = str
                        });
                    }
                }

                //add empty other row
                if (!caseByTypeofServiceBySex.Any(c => c.Subject.ToLower() == "other"))
                {
                    caseByTypeofServiceBySex.Add(new CaseBySubjectBySex
                    {
                        Subject = "Other"
                    });
                }
            }

            return (caseByTypeofService, caseByTypeofServiceBySex);
        }

        private (List<CaseBySubject> bySubject, List<CaseBySubjectBySex> bySubjectBySex) GetCaseByVulnerablePopulation(IEnumerable<AdminDashboardProjectModel> cases)
        {
            //List of service provider options from string keys
            var list = KeyLists.VulnerablePopulation;

            var typeOfServiceAndSex = cases.Where(c => !string.IsNullOrWhiteSpace(c.VulnerablePopulation))
                .Select(c => new Case
                {
                    VulnerablePopulation = c.VulnerablePopulation,
                    SexOfSurvior = c.SexOfSurvior,
                    HasSurviorReceivedService = c.HasSurviorReceivedService
                }).ToList();

            var serviceBySex = new List<(string, string, YesOrNo)>();

            //loops through the service types to pull the required data if its contained
            foreach (var caseObject in typeOfServiceAndSex)
            {
                //filters the list replaces those not listed with "Other"
                if (!string.IsNullOrWhiteSpace(caseObject.VulnerablePopulation))
                {
                    foreach (var service in JsonConvert.DeserializeObject<List<string>>(caseObject.VulnerablePopulation))
                    {
                        switch (service)
                        {
                            case null:
                                serviceBySex.Add(("None", caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                break;
                            default:
                                var value = list.Any(c => c.Trim().ToLower().EndsWith(service.Trim().ToLower()));

                                if (!value)
                                {
                                    //replace with Other if not found in list
                                    serviceBySex.Add(("Other", caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                }
                                else
                                {
                                    switch (service)
                                    {
                                        case "":
                                            serviceBySex.Add(("None", caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                            break;
                                        case "IDP":
                                            serviceBySex.Add((service, caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                            break;
                                        case "PLHIV":
                                            serviceBySex.Add((service, caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                            break;
                                        default:
                                            serviceBySex.Add((service.Trim().ToLower().FirstLetterToUpper(), caseObject.SexOfSurvior, caseObject.HasSurviorReceivedService));
                                            break;
                                    }

                                    //var test = list.Where(x => x.Contains(service.Trim()).fi;
                                }
                                break;
                        }
                    }
                }
            }

            var group = serviceBySex.GroupBy(c => c.Item1.Trim());

            //groups the services by common string
            var caseByTypeofService = group.Select(p => new CaseBySubject
            {
                Id = 0,
                Name = p.Key,
                Count = p.Count()
            }).ToList();

            //groups the services by sex here item2 is Sex , item1 is typeofService
            var caseByTypeofServiceBySex = group.Select(p => new CaseBySubjectBySex
            {
                Subject = p.Key,
                FemaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Female),
                MaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Male),
                OtherCount = p.Count(c =>
                    c.Item2.ToLower() != MetricsKeys.Male && c.Item2.ToLower() != MetricsKeys.Female),
                TotalNewCases = p.Count(c => c.Item3 == YesOrNo.No),
                TotalFollowUpCases = p.Count(c => c.Item3 == YesOrNo.Yes)
            }).ToList();

            { //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
              //add empty rows
                foreach (var item in list)
                {
                    var str = item.Replace("TypeOfService_", "");
                    if (!caseByTypeofServiceBySex.Any(c => c.Subject.ToLower() == str.ToLower()))
                    {
                        caseByTypeofServiceBySex.Add(new CaseBySubjectBySex
                        {
                            Subject = str
                        });
                    }
                }

                //add empty other row
                if (!caseByTypeofServiceBySex.Any(c => c.Subject.ToLower() == "other"))
                {
                    caseByTypeofServiceBySex.Add(new CaseBySubjectBySex
                    {
                        Subject = "Other"
                    });
                }
            }

            return (caseByTypeofService, caseByTypeofServiceBySex);
        }
        private static List<CaseBySubjectBySex> CasesByTypeOfViolenceBySex(IEnumerable<AdminDashboardProjectModel> cases, bool? doesTheSurvivorWantJustice = null)
        {
            if (doesTheSurvivorWantJustice.HasValue)
            {
                var yesOrNo = doesTheSurvivorWantJustice.Value ? YesOrNo.Yes : YesOrNo.No;
                cases = cases.Where(c => c.DoestheSurviorWantJustice == yesOrNo).ToList();
            }

            var caseCategoryList = Enum.GetValues(typeof(CaseCategoryOrTypeOfViolence)).ToListDynamic();
            var caseByType = new List<CaseBySubjectBySex>();

            foreach (var cat in caseCategoryList)
            {
                var catInfo = cases.Where(c => c.CaseCategoriesList.Contains((int)cat));

                caseByType.Add(new CaseBySubjectBySex
                {
                    Subject = cat.ToString(), //catInfo.FirstOrDefault().Category.Name,
                    FemaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    MaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    OtherCount = catInfo.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    TotalNewCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    TotalFollowUpCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                });
            }

            var otherCases = cases.Where(c => !c.CaseCategoriesList.Any() && !string.IsNullOrWhiteSpace(c.CaseCategoriesOthers))
                .Select(c => new
                {
                    c.SexOfSurvior,
                    c.HasSurviorReceivedService
                });

            var others = new CaseBySubjectBySex
            {
                MaleCount = otherCases.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = otherCases.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                OtherCount = otherCases.Count(c => c.SexOfSurvior != MetricsKeys.Male && c.SexOfSurvior != MetricsKeys.Female),
                TotalNewCases = otherCases.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                TotalFollowUpCases = otherCases.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes),
                Subject = "Others"
            };

            caseByType.Add(others);

            return caseByType.OrderBy(c => c.FemaleCount + c.MaleCount + c.OtherCount).ToList();
        }

        private List<CaseBySubjectBySex> CasesByWhoClosedCaseBySex(IEnumerable<AdminDashboardProjectModel> cases)
        {
            var queries = new List<(QueryFutureValue<bool> any, string subject, QueryFutureValue<int> femaleCount, QueryFutureValue<int> maleCount, QueryFutureValue<int> otherCount, QueryFutureValue<int> totalNewCases, QueryFutureValue<int> totalFollowUpCases)>();

            var caseByType = new List<CaseBySubjectBySex>();

            foreach (var type in KeyLists.WhoClosedTheCase)
            {
                var casesInCategory = cases.Where(c =>
                            c.WhoClosedTheCase != null &&
                            c.WhoClosedTheCase.Trim().ToLower().Contains(type.Trim().ToLower()))
                        .Select(c =>
                            new
                            {
                                c.SexOfSurvior,
                                c.HasSurviorReceivedService
                            }).ToList();

                if (casesInCategory.Any())
                {
                    caseByType.Add(new CaseBySubjectBySex
                    {
                        Subject = type,
                        MaleCount = casesInCategory.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                        FemaleCount = casesInCategory.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                        OtherCount = casesInCategory.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                        TotalNewCases = casesInCategory.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                        TotalFollowUpCases = casesInCategory.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                    });
                }
            }

            return caseByType.OrderBy(c => c.FemaleCount + c.MaleCount + c.OtherCount).ToList();
        }

        /// <summary>
        /// function the get the case by pepetrator by Sex
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        private static IEnumerable<CaseBySubjectBySex> GetCaseByRelationShipBySex(IEnumerable<AdminDashboardProjectModel> cases)
        {
            cases = cases.Where(c => !string.IsNullOrWhiteSpace(c.PerpetratorsInformation));

            var list = new List<string>
                {
                   MetricsKeys.Relationship_ReligiousLeader,
                   MetricsKeys.Relationship_DomesticStaff ,
                   MetricsKeys.Relationship_Teacher ,
                   MetricsKeys.Relationship_curentpartner ,
                   MetricsKeys.Relationship_formerpartner ,
                   MetricsKeys.Relationship_Parent,
                   MetricsKeys.Relationship_Relative ,
                   MetricsKeys.Relationhip_Spouse,
                   MetricsKeys.Relationship_LawEnforcement,
                   MetricsKeys.Relationship_Caregiver ,
                   MetricsKeys.Relationhip_Stranger
                 };


            var relationships = new List<PerpetratorsInformationModel>();

            foreach (var item in cases)
            {
                var perps = JsonConvert.DeserializeObject<List<PerpetratorsInformationModel>>(item.PerpetratorsInformation);

                foreach (var perp in perps)
                {
                    //var value = list.Any(c => c.Trim().ToLower().Equals(perp.SurviorRelationWithPerpetrator.Trim().ToLower()));
                    var value = list.Exists(c => c.Trim().Equals(perp.SurviorRelationWithPerpetrator.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (!value)
                    {
                        perp.SurviorRelationWithPerpetrator = ("Other");
                        relationships.Add(perp); //replace with Other if not found in list
                    }
                    else
                    {
                        var relationship = new PerpetratorsInformationModel();
                        if (relationships.Any())
                            relationship = relationships.Find(x => x.SurviorRelationWithPerpetrator.Trim().Equals(perp.SurviorRelationWithPerpetrator.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (relationship != null && !string.IsNullOrEmpty(relationship.SurviorRelationWithPerpetrator))
                        {
                            perp.SurviorRelationWithPerpetrator = relationship.SurviorRelationWithPerpetrator;
                        }
                        relationships.Add(perp);
                    }
                }
            }
            ////Todo: re write query for multiple perpetrators

            //groups the services by surviour relationship
            var caseByTypeofService = relationships
                                      .GroupBy(c => c.SurviorRelationWithPerpetrator) //Todo: re write query for multiple perpetrators
                                      .Select(p => new CaseBySubjectBySex
                                      {
                                          Subject = p.Key, //Todo: re write query for multiple perpetrators
                                          MaleCount = p.Count(c => c.SexOfPerpetrator.ToLower() == MetricsKeys.Male), //Todo: re write query for multiple perpetrators
                                          FemaleCount = p.Count(c => c.SexOfPerpetrator.ToLower() == MetricsKeys.Female) //Todo: re write query for multiple perpetrators
                                      })
                                       .OrderBy(c => c.Subject);

            return caseByTypeofService.OrderBy(c => c.MaleCount + c.FemaleCount);
        }

        #region Old implementation of GetCaseByVulnerablePopulation

        ///// <summary>
        ///// function to getCasebyVulnerablePopulation count and by Sex
        ///// </summary>
        ///// <param name="cases"></param>
        ///// <returns></returns>
        //private (List<CaseBySubject> bySubject, List<CaseBySubjectBySex> bySubjectBySex) GetCaseByVulnerablePopulation(IEnumerable<AdminDashboardProjectModel> list)
        //{
        //    var cases = list
        //        .Where(c => !string.IsNullOrWhiteSpace(c.VulnerablePopulation))
        //        .Where(c => c.VulnerablePopulation.ToLower() != MetricsKeys.Other).Select(c => new AdminDashboardProjectModel
        //        {
        //            VulnerablePopulation = c.VulnerablePopulation,
        //            HasSurviorReceivedService = c.HasSurviorReceivedService,
        //            SexOfSurvior = c.SexOfSurvior
        //        });

        //    var vulList = KeyLists.VulnerablePopulation;

        //    var vul = new List<AdminDashboardProjectModel>();

        //    //check if the vulnerable population type enter is listed above, else replace with other
        //    foreach (var item in cases)
        //    {
        //        var value = vulList.Any(c => c.Trim().ToLower() == item.VulnerablePopulation.Trim().ToLower());

        //        if (!value)
        //        {
        //            item.VulnerablePopulation = "Other";
        //            vul.Add(item); //replace with Other if not found in list
        //        }
        //        else
        //        {
        //            vul.Add(item); //add item if in the list
        //        }
        //    }

        //    var groupByvulnerablePopulation = vul.GroupBy(c => c.VulnerablePopulation).ToList();

        //    //group case by vulnerable population get count
        //    var sas = groupByvulnerablePopulation.Select(p => new CaseBySubject
        //    {
        //        Id = 0,
        //        Name = p.FirstOrDefault()?.VulnerablePopulation,
        //        Count = p.Count()
        //    }).OrderBy(c => c.Name).ToList();

        //    //group case by vulnerable population get count by sex
        //    var vulnerablePopulationBySex = groupByvulnerablePopulation.Select(p => new CaseBySubjectBySex
        //    {
        //        Subject = p.Key,
        //        FemaleCount = p.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
        //        MaleCount = p.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
        //        OtherCount = p.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
        //        TotalNewCases = p.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
        //        TotalFollowUpCases = p.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
        //    }).ToList();

        //    { //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
        //      //add empty rows
        //        foreach (var item in vulList)
        //        {
        //            if (!vulnerablePopulationBySex.Any(c => c.Subject.ToLower() == item.ToLower()))
        //            {
        //                vulnerablePopulationBySex.Add(new CaseBySubjectBySex
        //                {
        //                    Subject = item
        //                });
        //            }
        //        }

        //        //add empty other row
        //        if (!vulnerablePopulationBySex.Any(c => c.Subject.ToLower() == "other"))
        //        {
        //            vulnerablePopulationBySex.Add(new CaseBySubjectBySex
        //            {
        //                Subject = "Other"
        //            });
        //        }
        //    }

        //    return (sas, vulnerablePopulationBySex);
        //}

        #endregion

        private List<CaseByYear> GetCaseByYearByMonth(List<AdminDashboardProjectModel> Cases)
        {
            //group by date reported, by year and month
            var caseByMonth = Cases.GroupBy(c => c.DateReported.Month);
            var YearList = Cases.GroupBy(c => c.DateReported.Year)
                             .Select(c => c.Key);

            var casebyYearbyMonth = new List<CaseByYear>();

            for (int i = 1; i <= 12; i++)
            {
                string monthName = new DateTime(2010, i, 1).ToString("MMM", CultureInfo.InvariantCulture);

                var Month = caseByMonth.Where(m => m.Key == i).FirstOrDefault();

                if (Month is null)  //if there is not case for a particular month
                {
                    //set all years available count to 0
                    casebyYearbyMonth.Add(new CaseByYear
                    {
                        Month = monthName,
                        Years = YearList.Select(p => new CaseBySubject // set the year list variable
                        {
                            Id = p, //p is the year
                            Name = p.ToString(),
                            Count = 0
                        }).ToList(),
                    });
                }
                else
                {
                    //groups the months data by years
                    var MYears = Month.GroupBy(c => c.DateReported.Year).ToList();

                    var yearss = new List<CaseBySubject>();

                    //loop through the yearsList
                    foreach (var y in YearList)
                    {
                        //gets the data for a particular year
                        var sf = MYears.Where(p => p.Key == y)
                                .FirstOrDefault();

                        yearss.Add(new CaseBySubject
                        {
                            Id = y,
                            Name = y.ToString(),
                            Count = sf is null ? 0 : sf.Count()
                        });
                    }

                    casebyYearbyMonth.Add(new CaseByYear
                    {
                        Month = monthName,
                        Years = yearss
                    });
                }
            }

            return casebyYearbyMonth;
        }

        /// <summary>
        /// Gets the cases by location by sex
        /// </summary>
        /// <param name="cases"></param>
        /// <returns></returns>
        private List<CaseBySubjectBySex> GetCaseByLocationOfViolenceBySex(IQueryable<Case> cases)
        {
            var list = new List<string>
            {
             "Bush/forest",
             "Perpetrator's house",
             "Road",
             "Client's house",
            };

            var caseByLocation = cases.Where(c => !string.IsNullOrWhiteSpace(c.ActualLocationOfIncident))
                                       .ToList();

            List<Case> location = new List<Case>();

            foreach (var item in caseByLocation)
            {
                var value = list.Any(c => c.Trim().ToLower() == item.ActualLocationOfIncident.Trim().ToLower());

                if (!value)
                {
                    item.ActualLocationOfIncident = "Other";
                    location.Add(item); //replace with Other if not found in list
                }
                else
                {
                    location.Add(item);
                }
            }

            //group case by vulnerable population get count by sex
            var locationBySex = location.GroupBy(c => c.ActualLocationOfIncident)
                                .Select(p => new CaseBySubjectBySex
                                {
                                    Subject = p.Key,
                                    FemaleCount = p.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                                    MaleCount = p.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                                    OtherCount = p.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                                    TotalNewCases = p.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                                    TotalFollowUpCases = p.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                                }).ToList();

            { //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
              //add empty rows
                foreach (var item in list)
                {
                    if (!locationBySex.Any(c => c.Subject.ToLower() == item.ToLower()))
                    {
                        locationBySex.Add(new CaseBySubjectBySex
                        {
                            Subject = item
                        });
                    }
                }

                //add empty other row
                if (!locationBySex.Any(c => c.Subject.ToLower() == "other"))
                {
                    locationBySex.Add(new CaseBySubjectBySex
                    {
                        Subject = "Other"
                    });
                }
            }

            return locationBySex;
        }

        /// <summary>
        /// group cases by violience and sex by convicted
        /// </summary>
        /// <param name="Cases"></param>
        /// <returns></returns>
        public (List<CaseBySubjectBySex> Convicted, List<CaseBySubjectBySex> Court) GetCaseByViolenceBySexForConvicted(IQueryable<Case> Cases)
        {
            var casesReportedToPolice = Cases.Where(c => c.TypeOfServiceReceivedBySurvior.Contains("Services of the police/other security actors")).ToList();

            var casesReportedToPoliceConvicted = casesReportedToPolice.Where(c => c.OutcomeOfServiceorReferral != null && c.OutcomeOfServiceorReferral.ToLower().Contains("Perpetrator convicted".ToLower())).ToList();
            var casesReportedToPoliceCourt = casesReportedToPolice.Where(c => c.OutcomeOfServiceorReferral != null && c.OutcomeOfServiceorReferral.ToLower().Contains("Perpetrator charged to court".ToLower())).ToList();

            var caseCategoryList = Enum.GetValues(typeof(CaseCategoryOrTypeOfViolence));
            var CasesReportedToPoliceConvictedBySex = new List<CaseBySubjectBySex>();

            foreach (var cat in caseCategoryList)
            {
                var catInfo = casesReportedToPoliceConvicted.Where(c => c.CaseCategoriesList.Contains((int)cat));

                CasesReportedToPoliceConvictedBySex.Add(new CaseBySubjectBySex
                {
                    Subject = cat.ToString(), //catInfo.FirstOrDefault().Category.Name,
                    FemaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    MaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    OtherCount = catInfo.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    TotalNewCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    TotalFollowUpCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                });
            }

            var CasesReportedToPoliceCourtBySex = new List<CaseBySubjectBySex>();

            foreach (var cat in caseCategoryList)
            {
                var catInfo = casesReportedToPoliceCourt.Where(c => c.CaseCategoriesList.Contains((int)cat));

                CasesReportedToPoliceCourtBySex.Add(new CaseBySubjectBySex
                {
                    Subject = cat.ToString(),
                    FemaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    MaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    OtherCount = catInfo.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    TotalNewCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    TotalFollowUpCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                });
            }

            return (CasesReportedToPoliceConvictedBySex, CasesReportedToPoliceCourtBySex);
        }

        public async Task<AppResult<MemoryStream>> GetOrganisationsExcelSheet()
        {
            var OrganisationQuery = _context.Organisations.OrderBy(c => c.Name)
                .Select(c => new OrganisationViewModel
                {
                    Id = c.Id,
                    States = string.IsNullOrWhiteSpace(c.States) ? new List<StateList>() : JsonConvert.DeserializeObject<List<StateList>>(c.States ?? "[]"),
                    Address = c.Address,
                    CreatedAt = c.CreatedAt,
                    Code = c.Code,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    ModifiedAt = c.ModifiedAt.GetValueOrDefault(),
                    Name = c.Name,
                    OrganisationType = c.OrganisationType,
                    NumberOfUsers = c.Users.Count(),
                    Website = c.Website,
                    Acronym = c.Acronym,
                    HotLine = c.HotLine,
                    TypeOfService = JsonConvert.DeserializeObject<List<string>>(c.TypeOfService ?? "[]"),
                    SocialMediaData = JsonConvert.DeserializeObject<List<SocialMediaData>>(c.SocialMediaData ?? "[]")
                });

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new ExcelPackage();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add($"Organisations_{DateTime.Now:MMM-dd-yy}");

            var row = 1;
            var dictionary = await GetCodeDictionary();

            //Set Title of Sheet
            {
                WorkSheet.Cells[$"A{row}"].Value = "Name";
                WorkSheet.Cells[$"B{row}"].Value = "Code";
                WorkSheet.Cells[$"C{row}"].Value = "Email";
                WorkSheet.Cells[$"D{row}"].Value = "Phone Number";
                WorkSheet.Cells[$"E{row}"].Value = "Address";
                WorkSheet.Cells[$"F{row}"].Value = "States";
                WorkSheet.Cells[$"G{row}"].Value = "Organisation Type";
                WorkSheet.Cells[$"H{row}"].Value = "Acronym";
                WorkSheet.Cells[$"I{row}"].Value = "Website";
                WorkSheet.Cells[$"J{row}"].Value = "HotLine";
                WorkSheet.Cells[$"K{row}"].Value = "Type Of Service";
                WorkSheet.Cells[$"L{row}"].Value = "Social Media";
                WorkSheet.Cells[$"M{row}"].Value = "Date Created";
                WorkSheet.Cells[$"A{row}:M{row}"].Style.Font.Bold = true;
            }

            //populate the excel sheet
            foreach (var item in OrganisationQuery)
            {
                row++;

                WorkSheet.Cells[$"A{row}"].Value = item.Name;
                WorkSheet.Cells[$"B{row}"].Value = item.Code;
                WorkSheet.Cells[$"C{row}"].Value = item.Email;
                WorkSheet.Cells[$"D{row}"].Value = item.PhoneNumber;
                WorkSheet.Cells[$"E{row}"].Value = item.Address;
                WorkSheet.Cells[$"F{row}"].Value = string.Join(" , ", item.States.Select(s => s.Name)); //list
                WorkSheet.Cells[$"G{row}"].Value = item.OrganisationType;
                WorkSheet.Cells[$"H{row}"].Value = item.Acronym;
                WorkSheet.Cells[$"I{row}"].Value = item.Website;
                WorkSheet.Cells[$"J{row}"].Value = item.HotLine;
                WorkSheet.Cells[$"K{row}"].Value = string.Join(" , ", item.TypeOfService);
                WorkSheet.Cells[$"L{row}"].Value = string.Join(" , ", item.SocialMediaData.Select(s => s.Url)); //list
                WorkSheet.Cells[$"M{row}"].Value = item.CreatedAt.ToString("dd/MM/yyyy");
            }

            await Excelpackage.SaveAsync();

            var stream = new MemoryStream(Excelpackage.GetAsByteArray());

            var newFile = $"Organisations-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";

            return new AppResult<MemoryStream>
            {
                Data = stream,
                Message = newFile
            };
        }
    }
}