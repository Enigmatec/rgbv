using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using Service.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundServices
{
    /// <summary>
    /// Abstract bass class that inherits from backgroundServices class implementing the
    /// IhostedService Using the CrontabSchedule to Schedule The Jobs. All Jobs inherits from this Class
    /// </summary>
    public abstract class BackgroundServiceProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly INotification _notification;
        private CrontabSchedule _schedule;
        private DateTime _nextDate;
        private DateTime UTCTime => DateTime.Now.ToUniversalTime().AddHours(1);

        protected abstract string Schedule { get; }  //implemented by all service jobs

        protected abstract string JobName { get; }

        public BackgroundServiceProcessor(IServiceScopeFactory serviceScopeFactory, INotification notification)
        {
            _serviceScopeFactory = serviceScopeFactory;

            _schedule = CrontabSchedule.Parse(Schedule); //get the scheduled time format from the Schedule string.

            _nextDate = _schedule.GetNextOccurrence(UTCTime); //gets the next Occurence of the job.
            _notification = notification;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                //get the current date from the UTCTime property
                var currentdate = UTCTime;

                if (currentdate > _nextDate)  // checks if the currentdate is greater than the scheduled time of execute the job.
                {
                    // use the Iservicescopefactory provider to create a scope for services that are
                    // not a singleton.
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        await JobExecuteAsync(scope.ServiceProvider);
                    }
                    //gets the next scheduled occurence after execution.
                    _nextDate = _schedule.GetNextOccurrence(UTCTime);
                }

                await Task.Delay(15000, stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }

        // abstract method implemented by all service jobs.
        public abstract Task JobExecuteAsync(IServiceProvider serviceProvider);
    }
}