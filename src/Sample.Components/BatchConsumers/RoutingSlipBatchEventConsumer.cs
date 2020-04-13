namespace Sample.Components.BatchConsumers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Consumers;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Courier.Contracts;
    using MassTransit.Definition;
    using Microsoft.Extensions.Logging;


    public class RoutingSlipBatchEventConsumerDefinition :
        ConsumerDefinition<RoutingSlipBatchEventConsumer>
    {
        public RoutingSlipBatchEventConsumerDefinition()
        {
            ConcurrentMessageLimit = 20;
        }
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<RoutingSlipBatchEventConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>(o => o.SetMessageLimit(10).SetTimeLimit(100));
        }
    }

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