namespace Warehouse.Components.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;


    public class AllocateInventoryConsumer :
        IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await context.Publish<AllocationCreated>(new
            {
                context.Message.AllocationId,
                HoldDuration = 15000,
            });

            await context.RespondAsync<InventoryAllocated>(new
            {
                context.Message.AllocationId,
                context.Message.ItemNumber,
                context.Message.Quantity
            });
        }
    }
}