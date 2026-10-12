// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcgBuildingGenerator.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the AcgBuildingGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The client's ACGBuildingGeneratorData_t, which a generated dungeon's PlayfieldAnarchyF carries inline: the
    /// building's size, its template playfield, its ambient colour and its rooms. The client does not generate the
    /// layout; every room is listed. Version 3 only; a width or height of 0, a floor height over 1000, no template, a
    /// room count outside 1..10000 or a room out of range fail the read.
    /// </summary>
    public class AcgBuildingGenerator
    {
        #region Constants

        public const short CurrentVersion = 3;

        #endregion

        #region Constructors and Destructors

        public AcgBuildingGenerator()
        {
            this.Version = CurrentVersion;
            this.Rooms = new List<AcgRoom>();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public short Version { get; set; }

        /// <summary>In blocks.</summary>
        [AoMember(1)]
        public ushort Width { get; set; }

        /// <summary>In blocks.</summary>
        [AoMember(2)]
        public ushort Height { get; set; }

        [AoMember(3)]
        public ushort FloorHeight { get; set; }

        [AoMember(4)]
        public int TemplatePlayfield { get; set; }

        [AoMember(5)]
        public byte AmbientRed { get; set; }

        [AoMember(6)]
        public byte AmbientGreen { get; set; }

        [AoMember(7)]
        public byte AmbientBlue { get; set; }

        [AoMember(8)]
        public List<AcgRoom> Rooms { get; set; }

        #endregion
    }
}
