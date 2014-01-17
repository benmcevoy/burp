using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using System.Linq;
using Burp.Model;
using Burp.UDP;
using System.Timers;

namespace Burp.Services
{
    public class MessageService : IDisposable
    {
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly IPAddress _subnetMask;
        private readonly string _localIPAddress;
        private readonly IPAddress _broadcastIPAddress;
        private readonly Listener _listener;
        private readonly Sender _sender;
        private readonly string _name;
        private Dictionary<IPAddress, bool> _connectedClientsCheckList = new Dictionary<IPAddress,bool>();

        private Timer _pollConnectedClientsTimer = new Timer();
        private Timer _pollExpiryTimer = new Timer();

        private Client _source;
        public Client Source { get { return _source; } }
        
        private Dictionary<IPAddress, Client> _connectedClients = new Dictionary<IPAddress, Client>();
        public Dictionary<IPAddress, Client> ConnectedClients { get { return _connectedClients; } }

        public event EventHandler ConnectedClientsChanged;
        public event EventHandler<MessageEventArgs> MessageReceived;

        public MessageService(string name, string avatar, int port, IPAddress localIPaddress, IPAddress subnetMask)
        {
            _name = name;
            _subnetMask = subnetMask;
            _localIPAddress = localIPaddress.ToString();
            _broadcastIPAddress = localIPaddress.GetBroadcastAddress(_subnetMask);

            _source = new Client() { IPAddress = _localIPAddress, Name = _name };
            _source.AttachAvatar(avatar);

            _listener = new Listener(port);
            _sender = new Sender(port);

            _pollConnectedClientsTimer.Elapsed += new ElapsedEventHandler(PollConnectedClientsTimer_Elapsed);
            _listener.MessageReceived += new EventHandler<UDP.MessageEventArgs>(Listener_MessageReceived);

            // see if any one is out there
            Broadcast();

            _pollConnectedClientsTimer.Interval = 20000; // 20 seconds
            _pollConnectedClientsTimer.Start();

            _pollExpiryTimer.Interval = 5000;
            _pollExpiryTimer.Elapsed += new ElapsedEventHandler(PollExpiryTimer_Elapsed);
        }

        private void ProcessReceivedMessage(string message)
        {
            var msg = _serializer.Deserialize<Message>(message);

            switch (msg.Type)
            {
                case MessageType.None:
                    break;

                case MessageType.Broadcast:
                    UpdateConnectedClients(msg.Source); // someone we recieve a broadcast meesage from is clearly online
                    Annouce(msg.Source); // reply with our announcement
                    break;

                case MessageType.Annouce:
                    UpdateConnectedClients(msg.Source);
                    break;

                case MessageType.Message:
                    if (MessageReceived != null)
                    {
                        MessageReceived(this, new MessageEventArgs(msg));
                    }
                    break;

                default:
                    break;
            }
        }

        private void PollConnectedClientsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // TODO: i think this could be expressed better
            // basically when we poll for who is out there we then listen for five seconds (expiry timer)
            // and check off the list of people we heard from with the list of people we knew about
            // if you don't call back then we no assume you're offline and take you off the connected clients list
            // I'm a bit concerned that there will be potential threading issues here as we add and remove clients
            
            _pollConnectedClientsTimer.Stop();

            _connectedClientsCheckList = new Dictionary<IPAddress, bool>();

            foreach (var client in _connectedClients.Values)
            {
                _connectedClientsCheckList.Add(client.AsIPAddress(), false);
            }

            Broadcast();

            _pollExpiryTimer.Start(); // you have five seconds to comply
        }

        private void PollExpiryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _pollExpiryTimer.Stop();

            foreach (var checkClient in _connectedClientsCheckList)
            {
                if (!checkClient.Value)
                {
                    // get rid of anyone who did not respond
                    _connectedClients.Remove(checkClient.Key);
                }
            }

            OnConnectedClientsChanged();

            _pollConnectedClientsTimer.Start();
        }

        private void UpdateConnectedClients(Client client)
        {
            var annoucedIPAddress = client.AsIPAddress();

            // listen out for all announcements
            if (_connectedClientsCheckList.ContainsKey(annoucedIPAddress))
            {
                _connectedClientsCheckList[annoucedIPAddress] = true;
            }

            // add any newly announced clients
            if (!_connectedClients.ContainsKey(annoucedIPAddress))
            {
                _connectedClients.Add(annoucedIPAddress, client);

                OnConnectedClientsChanged();
            }
        }

        private void Listener_MessageReceived(object sender, UDP.MessageEventArgs e)
        {
            ProcessReceivedMessage(e.Message);
        }

        private void OnMessageReceived(Message annouceMessage)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageEventArgs(annouceMessage));
            }
        }

        private void OnConnectedClientsChanged()
        {
            if (ConnectedClientsChanged != null)
            {
                ConnectedClientsChanged(this, new EventArgs());
            }
        }

        private void Annouce(Client destination)
        {
            var message = new Message(_source)
            {
                Type = MessageType.Annouce,
                Text = string.Format("{0} is online", _source.Name)
            };
            
            message.AddDestination(destination);

            _sender.Send(_serializer.Serialize(message), destination.AsIPAddress());
        }

        private void Broadcast()
        {
            Send(new Message(_source)
            {
                Type = MessageType.Broadcast
                
            });
        }

        public void Send(string message)
        {
            Send(new Message(_source)
            {
                Type = MessageType.Message,
                Text = message,
                Destinations = _connectedClients.Values.ToList()
            });
        }

        public void Send(Message message)
        {
            var payload = _serializer.Serialize(message);

            switch (message.Type)
            {
                case MessageType.None:
                    break;

                case MessageType.Broadcast:
                    _sender.Send(payload, _broadcastIPAddress);
                    break;

                case MessageType.Annouce:
                    // you do not send unsolicited announcements
                    throw new InvalidOperationException("Cannot send unsolicited Annouce messages");
                
                case MessageType.Message:
                    foreach (var client in message.Destinations)
                    {
                        _sender.Send(payload, client.AsIPAddress());
                    }
                    break;

                default:
                    break;
            }
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
