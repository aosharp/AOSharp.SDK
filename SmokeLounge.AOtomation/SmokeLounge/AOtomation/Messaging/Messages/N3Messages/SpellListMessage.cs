// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellListMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellListMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using AOSharp.Common.GameData;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// Spells to run on a character (SpellListIIR_t): the spells, the dynel they come from, an identity and a flag,
    /// a name, optionally an item, a flag and an int. The client hands them to the character's spell handler. Read and
    /// written by SpellListSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.SpellList)]
    public class SpellListMessage : N3Message
    {
        #region Constructors and Destructors

        public SpellListMessage()
        {
            this.N3MessageType = N3MessageType.SpellList;
            this.Spells = new SpellData[0];
            this.Name = string.Empty;
        }

        #endregion

        #region AoMember Properties

        /// <summary>A positive 0x3F1 list of at most 999.</summary>
        [AoMember(0)]
        public SpellData[] Spells { get; set; }

        /// <summary>The dynel the spells come from (0:0, none).</summary>
        [AoMember(1)]
        public Identity Source { get; set; }

        [AoMember(2)]
        public Identity Unknown1 { get; set; }

        [AoMember(3)]
        public bool Unknown2 { get; set; }

        /// <summary>A 16-bit length (0x8000 and above fails the read), then its bytes.</summary>
        [AoMember(4)]
        public string Name { get; set; }

        [AoMember(5)]
        public bool HasItem { get; set; }

        /// <summary>With <see cref="HasItem"/>: an item the spells use, taken when the target is a character.</summary>
        [AoMember(6)]
        public Identity Item { get; set; }

        [AoMember(7)]
        public bool Unknown3 { get; set; }

        [AoMember(8)]
        public int Unknown4 { get; set; }

        #endregion
    }
}
