using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class TemplateModel<T>
    {
        public T Model { get; set; }

        public Dictionary<string, object> ViewData { get; set; }

        public TemplateModel(T model)
        {
            Model = model;
            ViewData = new Dictionary<string, object>();
        }

        public TemplateModel<T> Add(string Key, object Value)
        {
            ViewData.Add(Key, Value);

            return this;
        }
    }

    public class RegistrationMailModel
    {
        public string ConfirmationUrl { get; set; }
        public string UserName { get; set; }
    }

    public class PasswordMailModel
    {
        public string Password { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }

    public class ReportMailModel
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public CaseReportModel CaseReport { get; set; }
        public string Date => DateTime.Now.ToUniversalTime().AddHours(1).ToString("dd MMM, yyyy");
        public string State { get; set; }
    }

    public class CaseUpdateMailModel
    {
        public string IncidentCode { get; set; }
        public string Editor { get; set; }
        public string Name { get; set; }
        public string TimeOfEdit => DateTime.Now.ToUniversalTime().AddHours(1).ToString("dd MMM, yyyy hh:mm tt");
        public string Organisation { get; set; }
        public string State { get; set; }
    }

    public class CaseRejectedEmailModel
    {
        public string UserName { get; set; }

        public string Note { get; set; }

        public string IncidentCode { get; set; }
    }
}