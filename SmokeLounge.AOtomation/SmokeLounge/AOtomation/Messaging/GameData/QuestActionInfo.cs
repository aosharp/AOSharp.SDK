// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestActionInfo.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestActionInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>The client's QuestAction_t (0x8c bytes). Unknowns are named by their offset in it.</summary>
    public class QuestActionInfo
    {
        #region Constructors and Destructors

        public QuestActionInfo()
        {
            this.Floats2C = new float[4];
            this.Floats44 = new float[4];
            this.WorldPos = new QuestWorldPos();
        }

        #endregion

        #region AoMember Properties

        /// <summary>The action's type (MissionActionType).</summary>
        [AoMember(0)]
        public int Type { get; set; }

        [AoMember(1)]
        public Identity Identity04 { get; set; }

        [AoMember(2)]
        public Identity Identity0C { get; set; }

        [AoMember(3)]
        public Identity Identity14 { get; set; }

        [AoMember(4)]
        public Identity Identity1C { get; set; }

        [AoMember(5)]
        public Identity Identity24 { get; set; }

        [AoMember(6)]
        public float[] Floats2C { get; set; }

        [AoMember(7)]
        public Identity Identity3C { get; set; }

        [AoMember(8)]
        public float[] Floats44 { get; set; }

        [AoMember(9)]
        public Identity Identity54 { get; set; }

        /// <summary>When it expires (the client's remaining time counts to it).</summary>
        [AoMember(10)]
        public uint ExpiryTime { get; set; }

        [AoMember(11)]
        public int Unknown60 { get; set; }

        [AoMember(12)]
        public Identity Identity64 { get; set; }

        [AoMember(13)]
        public QuestWorldPos WorldPos { get; set; }

        #endregion
    }
}
