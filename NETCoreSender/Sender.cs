using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NETCoreSender
{
    interface IEventHubSender
    {
       void Create();

       Task SendRandomMessagesAsync(int numberOfMessages);

       void Close();
    }

    /// <summary>
    /// Source - https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
    /// </summary>
    class Sender : IEventHubSender
    {
        private const int TimeoutMsWaitBetweenMessages = 100;

        internal EventHubClient MyEventHubClient { get; private set; }

        internal MyEventHub MyEventHubConfig { get; private set; }

        public Sender(IOptions<MyEventHub> config)
        {
            // We want to know if secrets is null so we throw an exception if it is
            this.MyEventHubConfig = config.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public void Create()
        {
            Close();

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(this.MyEventHubConfig.SenderConnString)
            {
                EntityPath = this.MyEventHubConfig.EventHubName
            };

            this.MyEventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        public async Task SendRandomMessagesAsync(int numberOfMessages)
        {
            if (this.MyEventHubClient == null)
                throw new ApplicationException("Need to call CreateAsync() to create the EventHub client");

            if (numberOfMessages <= 0)
                throw new ArgumentOutOfRangeException("numberOfMessages", "Must be positive number");

            DateTime timestamp = DateTime.Now;

            for (int i = 0; i < numberOfMessages; i++)
            {
                try
                {
                    string message = $"Timestamp: {timestamp}; Message Number: {i}";
                    Console.WriteLine($"Sending message: {message}");
                    await this.MyEventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{DateTime.Now} > Exception: ${exception.Message}");
                    Console.ResetColor();
                }

                Thread.Sleep(TimeoutMsWaitBetweenMessages);
            }
        }

        public async void Close()
        {
            if (MyEventHubClient != null)
            {
                await MyEventHubClient.CloseAsync();
                MyEventHubClient = null;
            }
        }
    }
}
