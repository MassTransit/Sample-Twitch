namespace Warehouse.Startup
{
    using System;
    using Components.Consumers;
    using Components.StateMachines;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.MongoDbIntegration;
    using MassTransit.Platform.Abstractions;
    using Microsoft.Extensions.DependencyInjection;


    public class WarehouseStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionConfigurator configurator, IServiceCollection services)
        {
            configurator.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
            configurator.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://mongo";
                    r.DatabaseName = "allocations";
                });
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IRegistrationContext<IServiceProvider> context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
        }
    }
}