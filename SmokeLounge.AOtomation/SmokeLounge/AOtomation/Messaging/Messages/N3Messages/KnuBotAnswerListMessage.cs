// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnuBotAnswerListMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the KnuBotAnswerListMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// KnubotAnswerListIIR_c: a count, then that many answers, each an int length and its text. The client fails
    /// the read for a count or a length outside 1..1000. Read by KnuBotAnswerListSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.KnubotAnswerList)]
    public class KnuBotAnswerListMessage : N3Message
    {
        #region Constructors and Destructors

        public KnuBotAnswerListMessage()
        {
            this.N3MessageType = N3MessageType.KnubotAnswerList;
            this.Unknown1 = 2;
        }

        #endregion

        #region AoMember Properties

        /// <summary>KnubotBaseIIR_c's version: 2 (another fails the client's read).</summary>
        [AoMember(0)]
        public short Unknown1 { get; set; }

        /// <summary>The NPC.</summary>
        [AoMember(1)]
        public Identity Target { get; set; }

        [AoMember(2, SerializeSize = ArraySizeType.Int32)]
        public KnuBotDialogOption[] DialogOptions { get; set; }

        #endregion
    }
}
