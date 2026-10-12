// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// QuestIIR_t: a version that must be 1, an int, the quest and a second identity. The client removes the quest
    /// from the message's character whatever the int is, and sends one with 0 and the quest to delete a mission.
    /// </summary>
    [AoContract((int)N3MessageType.Quest)]
    public class QuestMessage : N3Message
    {
        #region Constructors and Destructors

        public QuestMessage()
        {
            this.N3MessageType = N3MessageType.Quest;
            this.Version = 1;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Action { get; set; }

        [AoMember(2)]
        public Identity Quest { get; set; }

        [AoMember(3)]
        public Identity Other { get; set; }

        #endregion
    }
}
