namespace Sample.Contracts
{
    using System;


    public interface OrderStatus
    {
        Guid OrderId { get; }

        string State { get; }
    }
}