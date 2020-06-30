namespace Sample.Components
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class ContainerScopedFilter :
        IFilter<ConsumeContext<SubmitOrder>>
    {
        public Task Send(ConsumeContext<SubmitOrder> context, IPipe<ConsumeContext<SubmitOrder>> next)
        {
            var provider = context.GetPayload<IServiceProvider>();

            Console.WriteLine("Filter ran");

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}