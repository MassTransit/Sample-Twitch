namespace Warehouse.Contracts
{
    using System;


    public interface AllocationHoldDurationExpired
    {
        Guid AllocationId { get; }
    }
}