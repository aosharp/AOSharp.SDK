// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoorStatusUpdateMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the DoorStatusUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    [AoContract((int)N3MessageType.DoorStatusUpdate)]
    public class DoorStatusUpdateMessage : N3Message
    {
        #region Constructors and Destructors

        public DoorStatusUpdateMessage()
        {
            this.N3MessageType = N3MessageType.DoorStatusUpdate;
            this.Version = 2;
            this.Identities = new Identity[0];
        }

        #endregion

        // Client ignores the rest of the message unless this is 2
        [AoMember(0)]
        public int Version { get; set; }

        // Client treats this as true only when == 1
        [AoMember(1)]
        public byte FlagA { get; set; }

        // 1 = play open, anything else = play close
        [AoMember(2)]
        public byte Open { get; set; }

        // Stat 195
        [AoMember(3)]
        public int Unknown195 { get; set; }

        // 1 sets Door_t+0x1d5
        [AoMember(4)]
        public byte FlagC { get; set; }

        [AoMember(5, SerializeSize = ArraySizeType.X3F1)]
        public Identity[] Identities { get; set; }
    }
}