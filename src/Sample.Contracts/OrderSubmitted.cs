namespace Sample.Contracts
{
    using System;
    using MassTransit;


    public interface OrderSubmitted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }

        string CustomerNumber { get; }
        string PaymentCardNumber { get; }

        MessageData<string> Notes { get; }
    }
}