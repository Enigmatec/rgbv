using System.ComponentModel;

namespace Core.Enums
{
    public enum CaseCategoryOrTypeOfViolence
    {
        All=0,
        [Description("Physical assault")]
        PhysicalAssault = 1,

        Defilement = 2,

        Rape = 3,

        [Description("Forced marriage")]
        ForcedMarriage = 4,

        [Description("Denial of resources/services")]
        DenialOfResourcesOrServices = 5,

        [Description("Emotional/psychological")]
        EmotionalOrPsychological = 6,

        [Description("Sexual assault")]
        SexualAssault = 7,

        [Description("Female genital mutilation")]
        FemaleGenitalMutilation = 8,

        [Description("Violation of property and inheritance right")]
        ViolationOfPropertyAndInheritanceRight = 9,

        [Description("Child abuse and neglect")]
        ChildAbuseAndNeglect = 10,

        [Description("Early marriage")]
        EarlyMarriage = 11,

        [Description("Online/Cyber bullying")]
        OnlineOrCyberBullying = 12,
        
        [Description("Financial/Economic")]
        FinancialOrEconomic=13
    }
}