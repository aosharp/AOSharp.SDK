// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShadowLevelMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ShadowLevelMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// ShadowLevelIIR_t: eight ints. The client sets the character's Level, IP, SK, LastXP, NextXP and XPKillRange
    /// from the first six, and reports the seventh as the SK gained.
    /// </summary>
    [AoContract((int)N3MessageType.ShadowLevel)]
    public class ShadowLevelMessage : N3Message
    {
        #region Constructors and Destructors

        public ShadowLevelMessage()
        {
            this.N3MessageType = N3MessageType.ShadowLevel;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Level { get; set; }

        [AoMember(1)]
        public int IP { get; set; }

        [AoMember(2)]
        public int SK { get; set; }

        [AoMember(3)]
        public int LastXP { get; set; }

        [AoMember(4)]
        public int NextXP { get; set; }

        [AoMember(5)]
        public int XPKillRange { get; set; }

        [AoMember(6)]
        public int SKGained { get; set; }

        [AoMember(7)]
        public int Unknown1 { get; set; }

        #endregion
    }
}
