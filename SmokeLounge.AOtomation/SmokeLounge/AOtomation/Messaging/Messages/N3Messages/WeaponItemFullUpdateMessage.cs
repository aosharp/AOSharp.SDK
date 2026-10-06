// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeaponItemFullUpdateMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the WeaponItemFullUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    // Serialized by WeaponItemFullUpdateSerializer. The client reads only the shared
    // IDynelFullUpdate fields for this message (Gamecode.dll 0x100a2a85 -> 0x100a143b).
    [AoContract((int)N3MessageType.WeaponItemFullUpdate)]
    public class WeaponItemFullUpdateMessage : N3Message, IDynelFullUpdate
    {
        #region Constructors and Destructors

        public WeaponItemFullUpdateMessage()
        {
            this.N3MessageType = N3MessageType.WeaponItemFullUpdate;
            this.Version = 11;
            this.Rotation = Quaternion.Identity;
            this.Stats = new GameTuple<Stat, int>[0];
            this.Blob = new byte[0];
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

        public GameTuple<Stat, int>[] Stats { get; set; }

        public byte[] Blob { get; set; }
    }
}