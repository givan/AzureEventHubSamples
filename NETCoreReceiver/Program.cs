using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NETCoreReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            // use appsecrets in development - use env variables: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows
            // merge appsettings with env variables: https://stackoverflow.com/questions/48298284/merging-appsettings-with-environment-variables-in-net-core
            // https://stackoverflow.com/questions/47733228/how-to-use-environmentname-in-net-core-2-0-console-application
            // need to add the NuGet package Microsoft.Extensions.Configuration.EnvironmentVariables
            // Safe storage of app secrets in development in ASP.NET Core -> https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows
            // User Secrets in a .NET Core Console App - https://www.twilio.com/blog/2018/05/user-secrets-in-a-net-core-console-app.html
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            //only add secrets in development
            if (isDevelopment)
            {
                // add secrets in command prompt with the command: dotnet user-secrets set MyEventHub:EventHubName mytesteventhub
                builder.AddUserSecrets<MyEventHub>();
            }

            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            //Map the implementations of your classes here ready for DI
            services
                .Configure<MyEventHub>(configuration.GetSection(nameof(MyEventHub)))
                .AddOptions()
                .AddSingleton<IEventProcessorHostFactory, MyEventHubProcessorHostFactory>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();

            var processorFactory = serviceProvider.GetService<IEventProcessorHostFactory>();
            var processor = processorFactory.Create();

            await processor.RegisterEventProcessorAsync<SimpleEventProcessor>();

            Console.WriteLine("Receiving. Press ENTER to stop worker");
            Console.ReadLine();

            await processor.UnregisterEventProcessorAsync();
        }
    }
}
