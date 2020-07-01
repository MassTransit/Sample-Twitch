namespace Sample.Startup
{
    using Components.BatchConsumers;
    using Components.Consumers;
    using Components.CourierActivities;
    using Components.StateMachines;
    using Components.StateMachines.OrderStateMachineActivities;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.MongoDbIntegration.MessageData;
    using MassTransit.Platform.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Warehouse.Contracts;


    public class SampleStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            services.AddScoped<AcceptOrderActivity>();

            configurator.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            configurator.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
            configurator.AddConsumersFromNamespaceContaining<RoutingSlipBatchEventConsumer>();

            configurator.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://mongo";
                    r.DatabaseName = "orders";
                });

            configurator.AddRequestClient<AllocateInventory>();
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://mongo", "attachments"));
        }
    }
}