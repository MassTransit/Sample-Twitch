namespace Sample.Components.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    public class FulfillOrderConsumer :
        IConsumer<FulfillOrder>
    {
        public async Task Consume(ConsumeContext<FulfillOrder> context)
        {
            if (context.Message.CustomerNumber.StartsWith("INVALID"))
            {
                throw new InvalidOperationException("We tried, but the customer is invalid");
            }

            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("AllocateInventory", new Uri("queue:allocate-inventory_execute"), new
            {
                ItemNumber = "ITEM123",
                Quantity = 10.0m
            });

            builder.AddActivity("PaymentActivity", new Uri("queue:payment_execute"),
                new
                {
                    CardNumber = context.Message.PaymentCardNumber ?? "5999-1234-5678-9012",
                    Amount = 99.95m
                });

            builder.AddVariable("OrderId", context.Message.OrderId);

            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send<OrderFulfillmentFaulted>(new {context.Message.OrderId}));

            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Completed | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send<OrderFulfillmentCompleted>(new {context.Message.OrderId}));

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}