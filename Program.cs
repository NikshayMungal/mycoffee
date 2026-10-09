using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoffeeNChill.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Register document store and other repositories
        services.AddSingleton<IDocumentStore, DocumentStores>();
        services.AddSingleton<MenuRepository>();
        services.AddSingleton<OrderRepository>();
    })
    .Build();

host.Run();