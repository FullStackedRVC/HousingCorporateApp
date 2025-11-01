using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Diagnostics;
namespace HouseCom
{
    public class ActivityEventLogProcessor : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord data)
        {
            // Execute base implementation, since I will only extend behavior
            base.OnEnd(data);
            var currentActivity = Activity.Current;
            currentActivity?.AddEvent(new ActivityEvent(data.Attributes.ToString()));
        }

    }
}