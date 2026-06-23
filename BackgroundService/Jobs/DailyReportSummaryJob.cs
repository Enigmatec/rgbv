using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundServices.Jobs
{
    public class DailyReportSummaryJob : BackgroundServiceProcessor
    {
        protected override string Schedule => "15 9 * * 6"; // cron tab string to execute at 10:15pm daily
        private ApplicationDbContext _context;
        private INotification _notification;
        protected override string JobName => "Report Weekly Summary Admin Job"; // the Name of the Job

        public DailyReportSummaryJob(IServiceScopeFactory serviceScopeFactory, INotification notification) : base(serviceScopeFactory, notification)
        {
            _notification = notification;
        }

        public override async Task JobExecuteAsync(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetService<ApplicationDbContext>(); // uses the IService provider to get the scoped Service for the DBContext.

            var start = DateTime.Now.ToUniversalTime().AddHours(1).Date.AddDays(-7); //gets the start time of the day at 12:00am  GMT+1

            var Cases = await _context.Cases.Where(c => c.CreatedAt >= start)
                              .Select(c => new CaseSummaryModel
                              {
                                  OrganisationId = c.OrganisationId,
                                  Organisation = c.Organisation.Name,
                                  OrganisationType = c.Organisation.OrganisationType,
                                  StateId = c.IncidentStateId,
                                  State = c.IncidentState.Name,
                                  //CaseCategoryId = c.CaseCategoryId,
                                  CaseCategory = c.Category.Name,
                                  StateReported = c.CreatedBy.State.Name
                              }).ToListAsync();

            var superAdmin = await _context.Users.Where(c => c.Type == RoleKeys.Administrator /*|| c.Type == RoleKeys.FederalSupervisor*/).ToListAsync();

            if (Cases.Count > 0)
            {
                var caseByOrganisation = Cases.GroupBy(c => c.OrganisationId)
                                        .Select(c => new CaseBySubject
                                        {
                                            Id = c.Key,
                                            Name = $"{c.FirstOrDefault().Organisation} - {c.FirstOrDefault().OrganisationType}",
                                            Count = c.Count()
                                        }).ToList();
                var caseByState = Cases.GroupBy(c => c.StateId).Select(c => new CaseBySubject
                {
                    Id = c.Key,
                    Name = c.FirstOrDefault().State,
                    Count = c.Count()
                }).ToList();
                var caseByCategory = Cases.GroupBy(c => c.CaseCategoryId).Select(c => new CaseBySubject
                {
                    Id = c.Key,
                    Name = c.FirstOrDefault().CaseCategory,
                    Count = c.Count()
                }).ToList();
                var caseByStateReported = Cases.GroupBy(c => c.StateReported).Select(c => new CaseBySubject
                {
                    Id = 0,
                    Name = c.Key,
                    Count = c.Count()
                }).ToList();

                var caseReport = new CaseReportModel
                {
                    CaseByCategory = caseByCategory,
                    CaseByOrganisation = caseByOrganisation,
                    CaseByState = caseByState,
                    CaseByStateReported = caseByStateReported
                };

                await _notification.SendDailyReportSummary(superAdmin, caseReport);
            }
            else
            {
                await _notification.SendDailyReportSummary(superAdmin, null);
            }
        }
    }
}