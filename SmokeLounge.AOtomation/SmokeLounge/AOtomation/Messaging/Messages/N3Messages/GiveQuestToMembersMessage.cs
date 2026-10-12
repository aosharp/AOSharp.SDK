// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GiveQuestToMembersMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the GiveQuestToMembersMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// GiveQuestToMembersIIR_t, which the client sends to share a mission with its team: the quest's instance only
    /// (the client sends it only for a quest identity, type 0xdac3, with an instance).
    /// </summary>
    [AoContract((int)N3MessageType.GiveQuestToMembers)]
    public class GiveQuestToMembersMessage : N3Message
    {
        #region Constructors and Destructors

        public GiveQuestToMembersMessage()
        {
            this.N3MessageType = N3MessageType.GiveQuestToMembers;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int QuestInstance { get; set; }

        #endregion
    }
}
