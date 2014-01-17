using System;

namespace Burp.UDP
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
