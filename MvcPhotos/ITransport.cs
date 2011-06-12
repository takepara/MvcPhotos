using System.Collections.Generic;

namespace MvcPhotos
{
    public interface ITransport
    {
        bool Connect(string server, int port);
        bool Connect(string server, int port, bool isSecure);
        bool Connected { get; }
        bool IsValidResponse(string responseText);
        void SendCommand(string command);
        IEnumerable<string> ReadLine();
    }
}