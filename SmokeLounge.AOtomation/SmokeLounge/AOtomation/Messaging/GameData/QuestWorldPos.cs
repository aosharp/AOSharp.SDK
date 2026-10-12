// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestWorldPos.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestWorldPos type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The client's WorldPos_c in a quest action: a playfield, an x and z, and an offset; the position is the offset
    /// plus (x, 0, z). It must be valid: all zero, or both halves of the playfield set with the position and the
    /// offset each x &gt; 0, y &gt;= 0, z &gt; 0.
    /// </summary>
    public class QuestWorldPos
    {
        #region AoMember Properties

        [AoMember(0)]
        public Identity Playfield { get; set; }

        [AoMember(1)]
        public int X { get; set; }

        [AoMember(2)]
        public int Z { get; set; }

        [AoMember(3)]
        public Vector3 Offset { get; set; }

        public Vector3 Position
        {
            get
            {
                return new Vector3(this.Offset.X + this.X, this.Offset.Y, this.Offset.Z + this.Z);
            }
        }

        public bool IsNull
        {
            get
            {
                return this.Playfield.Type == 0 && this.Playfield.Instance == 0 && IsZero(this.Position) && IsZero(this.Offset);
            }
        }

        public bool IsValid
        {
            get
            {
                return this.IsNull
                       || (this.Playfield.Type != 0 && this.Playfield.Instance != 0 && IsPositive(this.Position)
                           && IsPositive(this.Offset));
            }
        }

        #endregion

        #region Methods

        private static bool IsZero(Vector3 v)
        {
            return v.X == 0f && v.Y == 0f && v.Z == 0f;
        }

        private static bool IsPositive(Vector3 v)
        {
            return v.X > 0f && v.Y >= 0f && v.Z > 0f;
        }

        #endregion
    }
}
