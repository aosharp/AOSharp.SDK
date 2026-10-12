// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestAlternativeMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestAlternativeMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// QuestAlternativeIIR_t (over ACGQuestIIR_t): the missions a terminal offers, and (with none) the request for
    /// them. The client's read fails unless the message's identity is a character, the version is 4, the difficulty
    /// is 1..11, every slider is -100..100 (read as a signed byte), the scope (originator) is 1..8, the terminal is
    /// set, and there are at most 5 missions. Read and written by QuestAlternativeSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.QuestAlternative)]
    public class QuestAlternativeMessage : N3Message
    {
        #region Constructors and Destructors

        public QuestAlternativeMessage()
        {
            this.N3MessageType = N3MessageType.QuestAlternative;
            this.Version = 4;
            this.MissionSliders = new MissionSliders();
            this.MissionDetails = new MissionInfo[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public byte Version { get; set; }

        /// <summary>The difficulty, then the six dimensions as signed bytes.</summary>
        [AoMember(1)]
        public MissionSliders MissionSliders { get; set; }

        [AoMember(2)]
        public int Seed { get; set; }

        /// <summary>The originator (1..8).</summary>
        [AoMember(3)]
        public MissionScope Scope { get; set; }

        [AoMember(4)]
        public Identity Terminal { get; set; }

        [AoMember(5)]
        public MissionInfo[] MissionDetails { get; set; }

        #endregion
    }
}
