using System;
using System.Net;
using System.IO;
using System.Reflection;

namespace Burp.Model
{
    public class Client
    {
        public string Name { get; set; }

        public string IPAddress { get; set; }

        /// <summary>
        /// TODO: I'd like to only have the avatar attached for annoce or broadcast messages inorder to reduce the message weight
        /// </summary>
        public byte[] Avatar { get; set; }

        public IPAddress AsIPAddress()
        {
            return System.Net.IPAddress.Parse(this.IPAddress);
        }

        public void AttachAvatar(string avatar)
        {
            if (!string.IsNullOrEmpty(avatar))
            {
                Avatar = File.ReadAllBytes(avatar);
            }
        }
    }
}

