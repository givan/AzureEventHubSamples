using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Options;
using System;

namespace NETCoreReceiver
{
    internal class MyEventHubProcessorHostFactory : IEventProcessorHostFactory
    {
        private MyEventHub _myEventHubConfig;

        public MyEventHubProcessorHostFactory(IOptions<MyEventHub> config)
        {
            _myEventHubConfig = config.Value ?? throw new ArgumentNullException("config", "Could not find MyEventHub config settings");
        }
        EventProcessorHost IEventProcessorHostFactory.Create()
        {
            string eventProcessorHostName = $"MyEventProcessor-{DateTime.Now}";
            var eventProcessorHost = new EventProcessorHost(
                eventProcessorHostName, 
                _myEventHubConfig.EventHubName, 
                PartitionReceiver.DefaultConsumerGroupName, 
                _myEventHubConfig.SenderConnString, 
                _myEventHubConfig.StorageAccConnString,
                _myEventHubConfig.StorageContainerName
                );
            return eventProcessorHost;
        }
    }
}