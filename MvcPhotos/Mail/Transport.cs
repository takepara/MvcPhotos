using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace MvcPhotos.Mail
{
    public class Transport : IDisposable, ITransport
    {
        private readonly TcpClient _client;
        private Stream _networkStream;
        private StreamReader _streamReader;

        public Transport()
        {
            _client = new TcpClient
            {
                ReceiveTimeout = 180000,
                SendTimeout = 10000
            };
        }

        public bool Connect(string server, int port)
        {
            return Connect(server, port, false);
        }

        public bool Connect(string server, int port, bool isSecure)
        {
            // initialization
            _client.Connect(server, port);
            if(isSecure)
            {
                var secureStream = new SslStream(_client.GetStream());
                secureStream.AuthenticateAsClient(server);
                _networkStream = secureStream;
            }
            else
            {
                _networkStream = _client.GetStream();
            }
            _streamReader = new StreamReader(_networkStream);

            return _client.Connected;
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        public bool IsValidResponse(string responseText)
        {
            return responseText.StartsWith("+");
        }

        // サーバーにコマンドを送る
        public void SendCommand(string command)
        {
            if (!_client.Connected)
                return;

            var data = command + Mime.CrLf;
            var bytes = Encoding.ASCII.GetBytes(data.ToCharArray());
            
            _networkStream.Write(bytes, 0, bytes.Length);
        }

        // 一行読み込み
        public IEnumerable<string> ReadLine()
        {
            if(!_client.Connected)
                yield break;
            
            string line;
            while ((line = _streamReader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public void Dispose()
        {
            if (!_client.Connected) 
                return;
            
            _streamReader.Close();
            _networkStream.Close();
            _client.Close();
        }
    }
}
