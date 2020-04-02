namespace Warehouse.Contracts
{
    using System;


    public interface AllocationCreated
    {
        Guid AllocationId { get; }
        TimeSpan HoldDuration { get; }
    }
}