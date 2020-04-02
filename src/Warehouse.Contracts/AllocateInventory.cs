namespace Warehouse.Contracts
{
    using System;


    public interface AllocateInventory
    {
        Guid AllocationId { get; }

        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}