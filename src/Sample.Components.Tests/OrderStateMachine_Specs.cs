namespace Sample.Components.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous.Graphing;
    using Automatonymous.Visualizer;
    using Contracts;
    using MassTransit;
    using MassTransit.Testing;
    using NUnit.Framework;
    using StateMachines;


    [TestFixture]
    public class Submitting_an_order
    {
        [Test]
        public async Task Should_create_a_state_instance()
        {
            var orderStateMachine = new OrderStateMachine();

            var harness = new InMemoryTestHarness();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var instance = saga.Sagas.Contains(instanceId.Value);
                Assert.That(instance.CustomerNumber, Is.EqualTo("12345"));
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_respond_to_status_checks()
        {
            var orderStateMachine = new OrderStateMachine();

            var harness = new InMemoryTestHarness();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var requestClient = await harness.ConnectRequestClient<CheckOrder>();

                var response = await requestClient.GetResponse<OrderStatus>(new {OrderId = orderId});

                Assert.That(response.Message.State, Is.EqualTo(orderStateMachine.Submitted.Name));
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_cancel_when_customer_account_closed()
        {
            var orderStateMachine = new OrderStateMachine();

            var harness = new InMemoryTestHarness();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                await harness.Bus.Publish<CustomerAccountClosed>(new
                {
                    CustomerId = InVar.Id,
                    CustomerNumber = "12345"
                });

                instanceId = await saga.Exists(orderId, x => x.Canceled);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_accept_when_order_is_accepted()
        {
            var orderStateMachine = new OrderStateMachine();

            var harness = new InMemoryTestHarness();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                await harness.Bus.Publish<OrderAccepted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                });

                instanceId = await saga.Exists(orderId, x => x.Accepted);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public void Show_me_the_state_machine()
        {
            var orderStateMachine = new OrderStateMachine();

            var graph = orderStateMachine.GetGraph();

            var generator = new StateMachineGraphvizGenerator(graph);

            string dots = generator.CreateDotFile();

            Console.WriteLine(dots);

        }
    }
}