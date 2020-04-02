namespace Sample.Contracts
{
    using System;


    public interface OrderFulfillmentCompleted
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }
    }
}