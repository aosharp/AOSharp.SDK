// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayfieldAnarchyFMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the PlayfieldAnarchyFMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using AOSharp.Common.GameData;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The playfield the character enters (n3PlayfieldFullUpdateIIR_t, then PlayfieldAnarchyFIIR_t's world block): a
    /// version, the character's position, from version 2 the playfield proxy, from version 4 an object inline, then
    /// the playfield's world X and Z. The inline object is a playfield record, a generated building or a proxy
    /// instance; any other type fails the read. Live servers leave the inline identity off (and the world block, in
    /// older messages) when there is no room for it. Read and written by PlayfieldAnarchyFSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.PlayfieldAnarchyF)]
    public class PlayfieldAnarchyFMessage : N3Message
    {
        #region Constructors and Destructors

        public PlayfieldAnarchyFMessage()
        {
            this.N3MessageType = N3MessageType.PlayfieldAnarchyF;
            this.Unknown = 0x00;
            this.Version = 4;
            this.PlayfieldProxyVersion = (byte)'a';
            this.HasInlineIdentity = true;
            this.HasWorld = true;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public Vector3 CharacterCoordinates { get; set; }

        /// <summary>From version 2: 'a'; any other fails the read.</summary>
        [AoMember(2)]
        public byte PlayfieldProxyVersion { get; set; }

        /// <summary>The playfield.</summary>
        [AoMember(3)]
        public Identity PlayfieldId1 { get; set; }

        [AoMember(4)]
        public int Unknown3 { get; set; }

        [AoMember(5)]
        public int SG { get; set; }

        /// <summary>Where the character leaves the playfield to.</summary>
        [AoMember(6)]
        public Identity ProxyId { get; set; }

        /// <summary>From version 4: whether the inline identity was sent.</summary>
        [AoMember(7)]
        public bool HasInlineIdentity { get; set; }

        /// <summary>The inline object's identity; 0:0, none.</summary>
        [AoMember(8)]
        public Identity InlineIdentity { get; set; }

        /// <summary>The int every inline object starts with after its identity.</summary>
        [AoMember(9)]
        public int InlineVersion { get; set; }

        [AoMember(10)]
        public PlayfieldRecord PlayfieldRecord { get; set; }

        [AoMember(11)]
        public AcgBuildingGenerator BuildingGenerator { get; set; }

        [AoMember(12)]
        public ProxyInstance ProxyInstance { get; set; }

        /// <summary>Whether the world X and Z were sent.</summary>
        [AoMember(13)]
        public bool HasWorld { get; set; }

        [AoMember(14)]
        public int PlayfieldX { get; set; }

        [AoMember(15)]
        public int PlayfieldZ { get; set; }

        #endregion
    }
}
