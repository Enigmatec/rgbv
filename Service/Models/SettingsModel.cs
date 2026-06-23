namespace Service.Models
{
    public class SettingsViewModel
    {
        public bool HasDateReportedValidation { get; set; }
        public bool IsGBVQuestionsEnabled { get; set; }

        public bool AllowPrevMonthCases { get; set; }
    }
}