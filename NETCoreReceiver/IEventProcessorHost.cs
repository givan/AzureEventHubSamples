using Microsoft.Azure.EventHubs.Processor;

namespace NETCoreReceiver
{
    internal interface IEventProcessorHostFactory
    {
        EventProcessorHost Create();
    }
}