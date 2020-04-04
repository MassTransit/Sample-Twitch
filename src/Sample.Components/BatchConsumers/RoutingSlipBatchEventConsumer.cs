namespace Sample.Components.BatchConsumers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Consumers;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using Microsoft.Extensions.Logging;


    public class RoutingSlipBatchEventConsumer :
        IConsumer<Batch<RoutingSlipCompleted>>
    {
        readonly ILogger<RoutingSlipEventConsumer> _logger;

        public RoutingSlipBatchEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.Log(LogLevel.Information, "Routing Slips Completed: {TrackingNumbers}",
                    string.Join(", ", context.Message.Select(x => x.Message.TrackingNumber)));
            }

            return Task.CompletedTask;
        }
    }
}