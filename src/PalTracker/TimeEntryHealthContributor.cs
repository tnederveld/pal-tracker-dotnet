using System.Linq;
using Steeltoe.Management.Endpoint.Health;
using static Steeltoe.Management.Endpoint.Health.HealthStatus;

namespace PalTracker
{
    public class TimeEntryHealthContributor : IHealthContributor
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        public const int MaxTimeEntries = 5;


        public TimeEntryHealthContributor(ITimeEntryRepository timeEntryRepository)
        {
            _timeEntryRepository = timeEntryRepository;
        }

        public Health Health()
        {
            var count = _timeEntryRepository.List().Count();
            var status = count < MaxTimeEntries ? UP : DOWN;

            var health = new Health {Status = status};

            health.Details.Add("threshold", MaxTimeEntries);
            health.Details.Add("count", count);
            health.Details.Add("status", status.ToString());

            return health;
        }

        public string Id { get; } = "timeEntry";
    }
}