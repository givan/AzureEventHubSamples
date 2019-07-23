using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NETCoreSender
{
    class Program
    {
        public static void Main(string [] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            // use appsetings.json in .NET core - https://blog.bitscry.com/2017/05/30/appsettings-json-in-net-core-console-app/
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
                builder.AddUserSecrets<MyEventHub>();
            }

            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            //Map the implementations of your classes here ready for DI
            services
                .Configure<MyEventHub>(configuration.GetSection(nameof(MyEventHub)))
                .AddOptions()
                .AddSingleton<IEventHubSender, Sender>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();

            // Get the service you need - DI will handle any dependencies - in this case IOptions<MyEventHub>
            var sender = serviceProvider.GetService<IEventHubSender>();

            sender.Create();

            int msgCount = int.Parse(Environment.GetEnvironmentVariable("MSG_COUNT") ?? "20");
            Console.WriteLine($"About to send {msgCount} messages ... ");
            await sender.SendRandomMessagesAsync(msgCount); // send 20 messages

            sender.Close();
        }
    }
}
