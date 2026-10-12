// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Quest.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the Quest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The client's Quest_t (0xf8 bytes), versions 7 to 15: each later field is read only from the version that added
    /// it, taking the default otherwise. Unknowns are named by their offset in it; those the client reads and does
    /// not store, by their order.
    /// </summary>
    public class Quest
    {
        #region Constructors and Destructors

        public Quest()
        {
            this.Version = 15;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.Reward = new QuestReward();
            this.Actions = new List<QuestActionInfo>();
            this.Receivers = new List<Identity>();
            this.Receivers2 = new List<Identity>();
            this.Ints60 = new List<int>();
            this.Ints50 = new List<int>();
            this.PairsC8 = new List<KeyValuePair<Identity, int>>();
            this.Blocks = new List<QuestBlock>();
            this.UnknownA8 = 6;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public Identity Identity { get; set; }

        [AoMember(1)]
        public int Version { get; set; }

        /// <summary>After the version; not stored by the client.</summary>
        [AoMember(2)]
        public int Unknown1 { get; set; }

        /// <summary>After <see cref="Unknown1"/>; not stored by the client.</summary>
        [AoMember(3)]
        public int Unknown2 { get; set; }

        /// <summary>Bit 8: a team mission; bit 10: the client does not let it be deleted.</summary>
        [AoMember(4)]
        public int Flags { get; set; }

        public bool IsTeamMission
        {
            get
            {
                return (this.Flags >> 8 & 1) != 0;
            }
        }

        public bool CanDelete
        {
            get
            {
                return (this.Flags >> 10 & 1) == 0;
            }
        }

        /// <summary>At most 31 characters (the client's field is 32 bytes).</summary>
        [AoMember(5)]
        public string Name { get; set; }

        /// <summary>1 to 4096 characters.</summary>
        [AoMember(6)]
        public string Description { get; set; }

        [AoMember(7)]
        public Identity Identity38 { get; set; }

        [AoMember(8)]
        public QuestReward Reward { get; set; }

        [AoMember(9)]
        public Identity Identity40 { get; set; }

        [AoMember(10)]
        public int Icon { get; set; }

        [AoMember(11)]
        public int Unknown70 { get; set; }

        [AoMember(12)]
        public int Unknown74 { get; set; }

        [AoMember(13)]
        public List<QuestActionInfo> Actions { get; set; }

        /// <summary>The first identity list; the client merges it with <see cref="Receivers2"/>.</summary>
        [AoMember(14)]
        public List<Identity> Receivers { get; set; }

        /// <summary>Masked to 0x7ffffff.</summary>
        [AoMember(15)]
        public List<int> Ints60 { get; set; }

        /// <summary>Masked to 0x7ffffff.</summary>
        [AoMember(16)]
        public List<int> Ints50 { get; set; }

        /// <summary>After <see cref="Ints50"/>; not stored by the client.</summary>
        [AoMember(17)]
        public List<QuestBlock> Blocks { get; set; }

        /// <summary>From version 8 (6 before).</summary>
        [AoMember(18)]
        public int UnknownA8 { get; set; }

        /// <summary>From version 9.</summary>
        [AoMember(19)]
        public List<Identity> Receivers2 { get; set; }

        /// <summary>From version 10.</summary>
        [AoMember(20)]
        public int Unknown4C { get; set; }

        /// <summary>From version 11.</summary>
        [AoMember(21)]
        public int UnknownB0 { get; set; }

        /// <summary>From version 12.</summary>
        [AoMember(22)]
        public Identity IdentityB4 { get; set; }

        /// <summary>From version 12.</summary>
        [AoMember(23)]
        public int UnknownBC { get; set; }

        /// <summary>From version 12.</summary>
        [AoMember(24)]
        public int UnknownC0 { get; set; }

        /// <summary>From version 13.</summary>
        [AoMember(25)]
        public List<KeyValuePair<Identity, int>> PairsC8 { get; set; }

        /// <summary>From version 14.</summary>
        [AoMember(26)]
        public int UnknownD4 { get; set; }

        /// <summary>From version 15; null before.</summary>
        [AoMember(27)]
        public List<Identity> IdentitiesD8 { get; set; }

        #endregion
    }
}
