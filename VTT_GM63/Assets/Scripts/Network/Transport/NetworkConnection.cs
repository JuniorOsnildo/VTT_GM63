namespace Network.Transport
{
    public class NetworkConnection
    {
        public string Id { get; private set; }

        public NetworkConnection(string id)
        {
            Id = id;
        }
    }
}