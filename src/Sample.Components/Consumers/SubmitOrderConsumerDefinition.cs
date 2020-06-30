namespace Sample.Components.Consumers
{
    using System;
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;


    public class SubmitOrderConsumerDefinition :
        ConsumerDefinition<SubmitOrderConsumer>
    {
        readonly IServiceProvider _serviceProvider;

        public SubmitOrderConsumerDefinition(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            ConcurrentMessageLimit = 20;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
            endpointConfigurator.UseServiceScope(_serviceProvider);

            consumerConfigurator.Message<SubmitOrder>(m => m.UseFilter(new ContainerScopedFilter()));
        }
    }
}