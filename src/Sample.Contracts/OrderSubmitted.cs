namespace Sample.Contracts
{
    using System;


    public interface OrderSubmitted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }

        string CustomerNumber { get; }
        string PaymentCardNumber { get; }
    }
}