﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MassTransit.Transports.OnRamp;
using Microsoft.EntityFrameworkCore;
using MassTransit.EntityFrameworkCoreIntegration.OnRamp;
using Npgsql;
using System.Data.SQLite;

namespace OutboxSweeper
{
    class Program
    {
        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var massTransitOptions = hostContext.Configuration.GetSection(MassTransitOptions.Key).Get<MassTransitOptions>();
                services.Configure<MassTransitOptions>(options => hostContext.Configuration.GetSection(MassTransitOptions.Key).Bind(options));

                services.AddMassTransit(cfg =>
                {
                    cfg.SetKebabCaseEndpointNameFormatter();

                    cfg.UsingRabbitMq((x, y) =>
                    {
                        y.UseOutboxTransport(x);
                        y.Host(massTransitOptions.RabbitMq.HostAddress, massTransitOptions.RabbitMq.VirtualHost, h =>
                        {
                            h.Username(massTransitOptions.RabbitMq.Username);
                            h.Password(massTransitOptions.RabbitMq.Password);
                        });
                    });
                });

                //services.AddOnRampTransport<OnRampDbContext>(cfg =>
                //{
                //    cfg.SetInstanceId(hostContext.Configuration.GetValue<string>("name"));
                //    cfg.Clustered = true;
                //    cfg.PrefetchCount = 1000;
                //    cfg.BulkRemove = true; // This dramatically increases sweeper speed, because it will wait until all messages in the prefetch are complete and perform fewer SQL queries. You run the risk of more duplicates should there be an outage between publish, and database write.
                //});
                //services.AddDbContext<OnRampDbContext>((p,opt) => opt.UseSqlServer(p.GetRequiredService<IConfiguration>().GetConnectionString("mydb")));

                services.AddOnRampTransport(cfg =>
                {
                    cfg.SetInstanceId(hostContext.Configuration.GetValue<string>("name"));
                    cfg.Clustered = true;
                    cfg.PrefetchCount = 1000;
                    cfg.BulkRemove = true;
                    cfg.UsePostgres();
                });
                //services.AddScoped<DbConnection>(p => new SQLiteConnection(p.GetRequiredService<IConfiguration>().GetConnectionString("mydb")));
                services.AddScoped<DbConnection>(p => new NpgsqlConnection(p.GetRequiredService<IConfiguration>().GetConnectionString("mydb")));
                //services.AddScoped<DbConnection>(p => new SqlConnection(p.GetRequiredService<IConfiguration>().GetConnectionString("mydb")));

                services.AddMassTransitHostedService();
                services.AddOnRampTransportHostedService();
            });

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            try
            {
                // Set JsonConvert Default Serializer Settings
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
                settings.Converters.Add(new StringEnumConverter());
                JsonConvert.DefaultSettings = () => settings;

                var builder = CreateHostBuilder(args);

                if (isService)
                {
                    await builder.UseWindowsService().Build().RunAsync();
                }
                else
                {
                    await builder.RunConsoleAsync();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Host terminated unexpectedly: {ex}");
                return;
            }
        }
    }
}
