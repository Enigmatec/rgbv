using System.ComponentModel.DataAnnotations;

namespace Core.Enums
{
    public enum OrganisationType
    {
        CSO = 1,

        [Display(Name = "Service Provider")]
        ServiceProvider,
        [Display(Name = "Implementing Partner")]
        Partner
    }
}