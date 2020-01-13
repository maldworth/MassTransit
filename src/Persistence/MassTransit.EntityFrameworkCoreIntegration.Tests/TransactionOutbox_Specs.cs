﻿// Copyright 2007-2019 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
namespace MassTransit.EntityFrameworkCoreIntegration.Tests
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Transactions;
    using GreenPipes.Internals.Extensions;
    using MassTransit.Tests.Saga.Messages;
    using MassTransit.Transactions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using TestFramework;


    /// <summary>
    /// This test fixture has nothing to do with the Saga, but I wanted to test EFCore with the TransactionOutbox,
    /// so this was the easiest project to add a test spec to which has EF Core already referenced.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class TransactionOutbox_Specs :
        InMemoryTestFixture
    {
        private static object _createLock = new object();
        private static bool _creating = false;
        private static bool _created = false;

        [Test]
        public async Task Should_publish_after_db_create()
        {
            var message = new InitiateSimpleSaga();
            var product = new Product { Name = "Should_publish_after_db_create" };
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using(var dbContext = GetDbContext())
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                dbContext.Car.Add(product);
                await dbContext.SaveChangesAsync();

                await transactionOutbox.Publish(message);

                // Hasn't published yet
                Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());

                transaction.Complete();
            }

            // Now has published
            await _received;

            using (var dbContext = GetDbContext())
            {
                Assert.IsTrue(await dbContext.Car.AnyAsync(x => x.Id == product.Id));
            }
        }

        [Test]
        public async Task Should_not_publish_properly()
        {
            var message = new InitiateSimpleSaga();
            var product = new Product { Name = "Should_not_publish_properly" };
            var transactionOutbox = new TransactionOutbox(Bus, Bus, new NullLoggerFactory());

            using(var dbContext = GetDbContext())
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var entity = dbContext.Car.Add(product);
                await dbContext.SaveChangesAsync();

                await transactionOutbox.Publish(message);
            }

            Assert.That(async () => await _received.OrTimeout(s: 3), Throws.TypeOf<TimeoutException>());

            using(var dbContext = GetDbContext())
            {
                Assert.IsFalse(await dbContext.Car.AnyAsync(x => x.Id == product.Id));
            }
        }

        Task<ConsumeContext<InitiateSimpleSaga>> _received;

        private TransactionOutboxTestsDbContext GetDbContext()
        {
            var dbContext = new TransactionOutboxTestsDbContext(new DbContextOptionsBuilder().UseSqlServer(LocalDbConnectionStringProvider.GetLocalDbConnectionString("MassTransitUnitTests_TransactionOutbox")).Options);

            if (_created == false)
            {
                lock (_createLock)
                {
                    if (_creating == false)
                    {
                        _creating = true;
                        dbContext.Database.EnsureCreated();
                        _created = true;
                        //RelationalDatabaseCreator databaseCreator = (RelationalDatabaseCreator)dbContext.Database.GetService<IDatabaseCreator>();
                        //databaseCreator.CreateTables();

                    }
                }
            }

            return dbContext;
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _received = Handled<InitiateSimpleSaga>(configurator);
        }
    }

    public class TransactionOutboxTestsDbContext : DbContext
    {
        public TransactionOutboxTestsDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Car { get; set; }
    }

    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
