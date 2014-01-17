using System;
using System.Collections.Generic;

namespace Burp.Model
{
    public class Message
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Message()
        {
        }

        public Message(Client source)
        {
            this.Source = source;
            this.DateTime = DateTime.Now;
            this.Destinations = new List<Client>();
        }

        public void AddDestination(Client destination)
        {
            Destinations.Add(destination);
        }

        public MessageType Type { get; set; }

        public Client Source { get; set; }

        public List<Client> Destinations { get; set; }

        public string Text { get; set; }

        public DateTime DateTime { get; set; }
    }
}