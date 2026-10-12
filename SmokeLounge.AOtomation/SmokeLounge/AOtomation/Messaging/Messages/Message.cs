// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the Message type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages
{
    public class Message
    {
        #region Public Properties

        public MessageBody Body { get; set; }

        public Header Header { get; set; }

        public byte[] RawPacket {  get; set; }

        /// <summary>The read ran past the end of the packet: what it lacked was read as 0 (the client drops such a message).</summary>
        public bool ReadFailed { get; set; }

        #endregion
    }
}