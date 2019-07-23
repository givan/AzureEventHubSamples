namespace NETCoreReceiver
{
    internal class MyEventHub
    {
        public string EventHubName { get; set; }
        public string SenderConnString { get; set; }
        public string StorageAccConnString { get; set; }
        public string StorageContainerName { get; set; }
    }
}