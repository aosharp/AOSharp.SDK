// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestFullUpdateMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestFullUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// QuestFullUpdateIIR_t: the character's quests (a positive 0x3F1 count), then whether each is announced as new.
    /// Read and written by QuestFullUpdateSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.QuestFullUpdate)]
    public class QuestFullUpdateMessage : N3Message
    {
        #region Constructors and Destructors

        public QuestFullUpdateMessage()
        {
            this.N3MessageType = N3MessageType.QuestFullUpdate;
            this.Quests = new Quest[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public Quest[] Quests { get; set; }

        [AoMember(1)]
        public bool Announce { get; set; }

        #endregion
    }
}
