namespace Sample.Components.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Consumers;
    using Contracts;
    using MassTransit;
    using MassTransit.Testing;
    using NUnit.Framework;


    [TestFixture]
    public class When_an_order_request_is_consumed
    {
        [Test]
        public async Task Should_respond_with_acceptance_if_ok()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                var requestClient = await harness.ConnectRequestClient<SubmitOrder>();

                var response = await requestClient.GetResponse<OrderSubmissionAccepted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));

                Assert.That(consumer.Consumed.Select<SubmitOrder>().Any(), Is.True);

                Assert.That(await harness.Sent.Any<OrderSubmissionAccepted>(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_respond_with_rejected_if_test()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                var requestClient = await harness.ConnectRequestClient<SubmitOrder>();

                var response = await requestClient.GetResponse<OrderSubmissionRejected>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "TEST123"
                });

                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));

                Assert.That(consumer.Consumed.Select<SubmitOrder>().Any(), Is.True);

                Assert.That(harness.Sent.Select<OrderSubmissionRejected>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_consume_submit_order_commands()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.InputQueueSendEndpoint.Send<SubmitOrder>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(consumer.Consumed.Select<SubmitOrder>().Any(), Is.True);

                Assert.That(harness.Sent.Select<OrderSubmissionAccepted>().Any(), Is.False);
                Assert.That(harness.Sent.Select<OrderSubmissionRejected>().Any(), Is.False);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_not_publish_order_submitted_event_when_rejected()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(5)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.InputQueueSendEndpoint.Send<SubmitOrder>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "TEST123"
                });

                Assert.That(consumer.Consumed.Select<SubmitOrder>().Any(), Is.True);

                Assert.That(harness.Published.Select<OrderSubmitted>().Any(), Is.False);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_publish_order_submitted_event()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                await harness.InputQueueSendEndpoint.Send<SubmitOrder>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(await harness.Published.Any<OrderSubmitted>(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}