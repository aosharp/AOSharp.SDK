// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestReward.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestReward type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The reward a quest carries (versions 3 to 6), as the client reads it. Unknowns are named by their offset in
    /// the client's object. Two identity lists and an int are read and not kept.
    /// </summary>
    public class QuestReward
    {
        #region Constructors and Destructors

        public QuestReward()
        {
            this.Version = 6;
            this.Items = new List<AcgItem>();
            this.Identities1 = new List<Identity>();
            this.Identities2 = new List<Identity>();
            this.Item28 = new AcgItem();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Cash { get; set; }

        /// <summary>After the cash; not stored by the client.</summary>
        [AoMember(2)]
        public int Unknown1 { get; set; }

        [AoMember(3)]
        public int Experience { get; set; }

        /// <summary>After the experience (at most 30000 each); not stored by the client.</summary>
        [AoMember(4)]
        public List<Identity> Identities1 { get; set; }

        [AoMember(5)]
        public List<Identity> Identities2 { get; set; }

        [AoMember(6)]
        public List<AcgItem> Items { get; set; }

        /// <summary>From version 4.</summary>
        [AoMember(7)]
        public int Unknown08 { get; set; }

        /// <summary>From version 4.</summary>
        [AoMember(8)]
        public int Unknown14 { get; set; }

        /// <summary>From version 4.</summary>
        [AoMember(9)]
        public int Unknown18 { get; set; }

        /// <summary>From version 5.</summary>
        [AoMember(10)]
        public int Unknown0C { get; set; }

        /// <summary>From version 5.</summary>
        [AoMember(11)]
        public int Unknown10 { get; set; }

        /// <summary>From version 6.</summary>
        [AoMember(12)]
        public AcgItem Item28 { get; set; }

        #endregion
    }
}
