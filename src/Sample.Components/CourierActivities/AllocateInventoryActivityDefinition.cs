namespace Sample.Components.CourierActivities
{
    using MassTransit.Definition;


    public class AllocateInventoryActivityDefinition :
        ActivityDefinition<AllocateInventoryActivity, AllocateInventoryArguments, AllocateInventoryLog>
    {
        public AllocateInventoryActivityDefinition()
        {
            ConcurrentMessageLimit = 10;
        }
    }
}