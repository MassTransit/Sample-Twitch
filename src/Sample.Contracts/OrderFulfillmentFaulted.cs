namespace Sample.Contracts
{
    using System;


    public interface OrderFulfillmentFaulted
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }
    }
}