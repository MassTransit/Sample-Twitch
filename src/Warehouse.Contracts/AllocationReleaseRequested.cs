namespace Warehouse.Contracts
{
    using System;


    public interface AllocationReleaseRequested
    {
        Guid AllocationId { get; }

        string Reason { get; }
    }
}