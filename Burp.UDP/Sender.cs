using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Burp.UDP
{
    public class Sender
    {
        private readonly int _sendPort;

        public Sender(int sendPort)
        {
            _sendPort = sendPort;
        }

        public void Send(string message, IPAddress ipAddress)
        {
            using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                var sendBuffer = Encoding.ASCII.GetBytes(message);

                var endPoint = new IPEndPoint(ipAddress, _sendPort);

                s.SendTo(sendBuffer, endPoint);

                Debug.WriteLine(string.Concat("Sent: ", message));
            }
        }
    }
}

