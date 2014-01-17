using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Burp.UDP
{
    public class Listener : IDisposable
    {
        private readonly int _listenPort;
        private Thread _listenThread;
        public event EventHandler<MessageEventArgs> MessageReceived;
        private readonly UdpClient _listener;
        private object _lock = new object();
        private bool _isListening = false;

        public Listener(int listenPort)
        {
            _listenPort = listenPort;
            _listener = new UdpClient(_listenPort);
            _listenThread = new Thread(new ThreadStart(StartListener));
            _listenThread.Start();
        }

        private void StartListener()
        {
            lock (_lock)
            {
                _isListening = true;
            }

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, _listenPort);

            try
            {
                while (_isListening)
                {
                    Debug.WriteLine("Waiting for broadcast");

                    byte[] bytes = _listener.Receive(ref groupEP);

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    Debug.WriteLine("Received broadcast from {0} :\n {1}\n",
                        groupEP.ToString(),
                        message);

                    OnMessageReceived(message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                _listener.Close();
            }
        }

        private void OnMessageReceived(string message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(message));
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _isListening = false;
            }

            _listener.Close();
        }
    }
}
