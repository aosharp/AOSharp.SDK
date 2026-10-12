// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfoPacketMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the InfoPacketMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// InfoPacketIIR_t: the <see cref="GameData.InfoPacket"/> of the character the message names. The client's reader
    /// fails (and the message is dropped) when its stream does. Read and written by InfoPacketSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.InfoPacket)]
    public class InfoPacketMessage : N3Message
    {
        #region Constructors and Destructors

        public InfoPacketMessage()
        {
            this.N3MessageType = N3MessageType.InfoPacket;
            this.Info = new InfoPacket();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public InfoPacket Info { get; set; }

        #endregion
    }
}
