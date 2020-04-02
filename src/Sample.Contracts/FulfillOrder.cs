namespace Sample.Contracts
{
    using System;


    public interface FulfillOrder
    {
        Guid OrderId { get; }

        string CustomerNumber { get; }
        string PaymentCardNumber { get; }
    }
}