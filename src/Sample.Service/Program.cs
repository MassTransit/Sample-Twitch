namespace Sample.Service
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Components.BatchConsumers;
    using Components.Consumers;
    using Components.CourierActivities;
    using Components.StateMachines;
    using Components.StateMachines.OrderStateMachineActivities;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using MassTransit.Definition;
    using MassTransit.MongoDbIntegration.MessageData;
    using MassTransit.RabbitMqTransport;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Warehouse.Contracts;


    class Program
    {
        static DependencyTrackingTelemetryModule _module;
        static TelemetryClient _telemetryClient;

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    _module = new DependencyTrackingTelemetryModule();
                    _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                    TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                    configuration.InstrumentationKey = "6b4c6c82-3250-4170-97d3-245ee1449278";
                    configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                    _telemetryClient = new TelemetryClient(configuration);

                    _module.Initialize(configuration);

                    services.AddScoped<AcceptOrderActivity>();

                    services.AddScoped<RoutingSlipBatchEventConsumer>();

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://127.0.0.1";
                                r.DatabaseName = "orders";
                            });

                        cfg.UsingRabbitMq(ConfigureBus);

                        cfg.AddRequestClient<AllocateInventory>();
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                });

            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();

            _telemetryClient?.Flush();
            _module?.Dispose();

            Log.CloseAndFlush();
        }

        static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));
            configurator.UseMessageScheduler(new Uri("queue:quartz"));

            configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
            {
                e.PrefetchCount = 20;

                e.Batch<RoutingSlipCompleted>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit = TimeSpan.FromSeconds(5);

                    b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
                });
            });

            configurator.ConfigureEndpoints(context);
        }
    }
}