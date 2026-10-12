// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayfieldRecord.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the PlayfieldRecord type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The client's RDBPlayfield_t as a PlayfieldAnarchyF carries it inline (record type 1000001): the playfield's
    /// header, then its rooms (indoors: a tilemap other than itself) or its zones' camera attractors (outdoors), its
    /// water, its environment and its destinations. Format versions below 7, and an id other than the inline
    /// identity's, fail the read.
    /// </summary>
    public class PlayfieldRecord
    {
        #region Constants

        public const int RecordType = 1000001;

        public const int MinFormatVersion = 7;

        #endregion

        #region Constructors and Destructors

        public PlayfieldRecord()
        {
            this.Name = string.Empty;
            this.Reserved = new byte[24];
            this.Rooms = new List<PlayfieldRoom>();
            this.Zones = new List<List<CameraAttractor>>();
            this.Water = new List<WaterMesh>();
            this.Environment = new byte[0];
            this.Destinations = new List<PlayfieldDestination>();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Id { get; set; }

        /// <summary>At most 32 bytes.</summary>
        [AoMember(2)]
        public string Name { get; set; }

        /// <summary>The playfield whose tiles it uses; another playfield's makes it indoors.</summary>
        [AoMember(3)]
        public int TilemapId { get; set; }

        [AoMember(4)]
        public int ZoneSize { get; set; }

        public bool IsIndoor
        {
            get
            {
                return this.TilemapId != this.Id;
            }
        }

        /// <summary>From format version 9.</summary>
        [AoMember(5)]
        public int Flags { get; set; }

        [AoMember(6)]
        public int UnknownV9B { get; set; }

        [AoMember(7)]
        public int PlayfieldTypeBits { get; set; }

        [AoMember(8)]
        public int UnknownV9C { get; set; }

        [AoMember(9)]
        public int UnknownV9D { get; set; }

        /// <summary>24 bytes the client reserves (from format version 9).</summary>
        [AoMember(10)]
        public byte[] Reserved { get; set; }

        /// <summary>Indoors: a room per zone.</summary>
        [AoMember(11)]
        public List<PlayfieldRoom> Rooms { get; set; }

        /// <summary>Outdoors: each zone's camera attractors (from format version 5; none before).</summary>
        [AoMember(12)]
        public List<List<CameraAttractor>> Zones { get; set; }

        [AoMember(13)]
        public List<WaterMesh> Water { get; set; }

        [AoMember(14)]
        public int Unknown3 { get; set; }

        [AoMember(15)]
        public int Unknown4 { get; set; }

        /// <summary>
        /// The environment's format (0 or less: no environment). 26 sends 22 bytes, 29 sends 25, any other 26.
        /// </summary>
        [AoMember(16)]
        public int EnvironmentVersion { get; set; }

        /// <summary>
        /// EnvironmentData_t's bytes as sent: 0..7 the weights of the eight weather kinds, 8..18 the parameters (10 and
        /// 11 swapped), 19..25 values passed through.
        /// </summary>
        [AoMember(17)]
        public byte[] Environment { get; set; }

        [AoMember(18)]
        public List<PlayfieldDestination> Destinations { get; set; }

        #endregion
    }
}
