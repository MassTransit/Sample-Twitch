namespace Sample.Components.CourierActivities
{
    using System;


    public interface PaymentArguments
    {
        Guid OrderId { get; }
        decimal Amount { get; }
        string CardNumber { get; }
    }
}