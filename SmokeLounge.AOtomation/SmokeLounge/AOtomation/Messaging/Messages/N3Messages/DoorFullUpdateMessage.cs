// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoorFullUpdateMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the DoorFullUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    // Serialized by DoorFullUpdateSerializer.
    [AoContract((int)N3MessageType.DoorFullUpdate)]
    public class DoorFullUpdateMessage : N3Message, IDynelFullUpdate
    {
        #region Constructors and Destructors

        public DoorFullUpdateMessage()
        {
            this.N3MessageType = N3MessageType.DoorFullUpdate;
            this.Version = 11;
            this.Version2 = 2;
            this.Version3 = 2;
            this.Rotation = Quaternion.Identity;
            this.Stats = new GameTuple<Stat, int>[0];
            this.Identities = new Identity[0];
        }

        #endregion

        public int Version { get; set; }

        public Identity Owner { get; set; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public int Playfield { get; set; }

        public Identity StateMachine { get; set; }

        public byte InventoryId { get; set; }

        public byte CurrBodyLocation { get; set; }

        // Stat 12 = mesh id; stat 0 = flags, where 0x80 = open
        public GameTuple<Stat, int>[] Stats { get; set; }

        // Only written when non-empty; a null or empty blob is sent as length 0
        public byte[] Blob { get; set; }

        public int Version2 { get; set; }

        public int Unknown3 { get; set; }

        public Identity[] Identities { get; set; }

        public int Version3 { get; set; }

        // Ends up in Door_t+0x1d0
        public int DoorValue { get; set; }
    }
}