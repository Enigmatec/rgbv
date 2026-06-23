namespace Core.Entities
{
    public class Setting : BaseEnity<int>
    {
        public bool HasDateReportedValidation { get; set; }
        public bool IsGBVQuestionsEnabled { get; set; }

        public bool AllowPrevMonthCases { get; set; }
    }
}