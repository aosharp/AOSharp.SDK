// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayfieldRoom.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the PlayfieldRoom type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// An indoor playfield's room (one per zone): its flags (the rotation in the low two bits; 0x80, a name follows),
    /// its tile rectangle, its place in the template, its door connections, a packed lightmap, its water and its
    /// camera attractors.
    /// </summary>
    public class PlayfieldRoom
    {
        #region Constructors and Destructors

        public PlayfieldRoom()
        {
            this.DoorConnections = new List<DoorConnection>();
            this.Name = string.Empty;
            this.Lightmap = new byte[0];
            this.Water = new List<WaterMesh>();
            this.CameraAttractors = new List<CameraAttractor>();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public byte Flags { get; set; }

        public int Rotation
        {
            get
            {
                return this.Flags & 3;
            }
        }

        [AoMember(1)]
        public byte Unused { get; set; }

        [AoMember(2)]
        public short TileX1 { get; set; }

        [AoMember(3)]
        public short TileY1 { get; set; }

        [AoMember(4)]
        public short TileX2 { get; set; }

        [AoMember(5)]
        public short TileY2 { get; set; }

        [AoMember(6)]
        public Vector3 TemplatePosition { get; set; }

        [AoMember(7)]
        public List<DoorConnection> DoorConnections { get; set; }

        /// <summary>With flag 0x80: at most 32 bytes.</summary>
        [AoMember(8)]
        public string Name { get; set; }

        /// <summary>Whether <see cref="Lightmap"/> is packed (zlib); 0, the client takes no lightmap.</summary>
        [AoMember(9)]
        public int LightmapPacked { get; set; }

        /// <summary>The lightmap image as sent, zlib-packed (a two-byte header, then deflate).</summary>
        [AoMember(10)]
        public byte[] Lightmap { get; set; }

        /// <summary>A positive 0x3F1 list; any other count fails the read.</summary>
        [AoMember(11)]
        public List<WaterMesh> Water { get; set; }

        /// <summary>From format version 5.</summary>
        [AoMember(12)]
        public List<CameraAttractor> CameraAttractors { get; set; }

        #endregion
    }
}
