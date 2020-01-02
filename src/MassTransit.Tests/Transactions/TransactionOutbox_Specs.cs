using GreenPipes.Internals.Extensions;
using MassTransit.TestFramework;
using MassTransit.TestFramework.Messages;
using MassTransit.Transactions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace MassTransit.Tests.Transactions
{
    [TestFixture]
    public class Publishing_a_message :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_publish_properly()
        {
            var message = new PingMessage();
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await transactionOutbox.Publish(message);

                // Hasn't published yet
                Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());

                transaction.Complete();
            }
            // Now has published
            await _received;
        }

        [Test]
        public async Task Should_not_publish_properly()
        {
            var message = new PingMessage();
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await transactionOutbox.Publish(message);
            }

            Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());
        }

        [Test]
        public async Task Should_send_properly()
        {
            var message = new PingMessage();
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var sendEndpoint = await transactionOutbox.GetSendEndpoint(InputQueueAddress);
                await sendEndpoint.Send(message);

                Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());

                transaction.Complete();
            }

            await _received;
        }

        [Test]
        public async Task Should_not_send_properly()
        {
            var message = new PingMessage();
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var sendEndpoint = await transactionOutbox.GetSendEndpoint(InputQueueAddress);
                await sendEndpoint.Send(message);
            }

            Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());
        }

        Task<ConsumeContext<PingMessage>> _received;

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _received = Handled<PingMessage>(configurator);
        }
    }


}
