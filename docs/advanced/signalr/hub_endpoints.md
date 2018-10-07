# Hub Endpoints

The core of communication contracts between the client and server are hubs. Depending on your application and complexity you might have a few hubs as a separation of concern for your application. The backplanes work through 5 types of events **per hub**.

So this translated well into MassTransit Events:

* `All<THub>` - Invokes the method (with args) for each connection on the specified hub
* `Connection<THub>` - Invokes the method (with args) for the specific connection
* `Group<THub>` - Invokes the method (with args) for all connections belonging to the specified group
* `GroupManagement<THub>` - Adds or removes a connection to the group (on a remote server)
* `User<THub>` - Invokes the method (with args) for all connections belonging to the specific user id

So each of these Messages has a corresponding consumer, and it will get a singleton `HubLifetimeManager<THub>` through DI to perform the specific task.

In the most simple configuration, all consumers are on a single endpoint (as shown in the [quickstart](quickstart.md)). MassTransit's recommendation is to usually have one consumer per endpoint, or multiple messages that are related. Taking into consideration [SignalR Limitations](https://docs.microsoft.com/en-us/aspnet/signalr/overview/performance/scaleout-in-signalr#limitations), you may decide to separate each group of 5 consumers (per hub). But this shouldn't be necessary for most scenarios, because most brokers can handle quite a large throughput of messages, and SignalR recommends re-thinking your strategy for very high throughput, real-time applications (video games).

## Optional (Alternate) Hub Per Endpoint

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // other config...
    services.AddMassTransitBackplane(out var hubConsumers, typeof(Startup).Assembly); // This is the first important line

    // Other config perhaps...

    // creating the bus config
    services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        // other config...

        // Creates a non-durable, temporary endpoint per hub
        cfg.CreateBackplaneEndpointsByHub<IRabbitMqReceiveEndpointConfigurator>(provider, host, hubConsumers, e=>
        {
            e.AutoDelete = true;
            e.Durable = false;
        });
    }));
}
```