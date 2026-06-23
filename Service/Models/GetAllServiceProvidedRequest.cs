using Core.Entities;
using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class GetAllServiceProvidedRequest : PaginationModel
    {
        public ValidationStatus? ValidationStatus { get; set; }

        public DateTime? StartDateOfServiceProvision { get; set; }

        public DateTime? EndDateOfServiceProvision { get; set; }

        public int? StateId { get; set; }

        public int? OrganisationLgaId { get; set; }

        public string Organisation { get; set; }

        public int? OrganisationId { get; set; }

        public List<string> TypeOfServiceProvided { get; set; }

        public string ServiceProvisionCode { get; set; }
        public string IsCaseValidated { get; set; }
    }

    public class ServiceProvidedRequest
    {
        public ValidationStatus? ValidationStatus { get; set; }

        public DateTime? StartDateOfServiceProvision { get; set; }

        public DateTime? EndDateOfServiceProvision { get; set; }

        public int? StateId { get; set; }

        public int? OrganisationLgaId { get; set; }

        public string Organisation { get; set; }

        public int? OrganisationId { get; set; }

        public List<string> TypeOfServiceProvided { get; set; }

        public string ServiceProvisionCode { get; set; }
        public string IsCaseValidated { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}