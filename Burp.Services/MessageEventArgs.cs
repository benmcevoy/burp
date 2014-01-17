using System;
using Burp.Model;

namespace Burp.Services
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }

        public MessageEventArgs(Message message)
        {
            this.Message = message;
        }
    }
}
