// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnuBotAppendTextMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the KnuBotAppendTextMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// KnubotAppendTextIIR_c: the kind, then the text (an int length, 0..10000, and its bytes); the client shows the
    /// text in the chat window as the kind says. Read by KnuBotAppendTextSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.KnubotAppendText)]
    public class KnuBotAppendTextMessage : N3Message
    {
        #region Constructors and Destructors

        public KnuBotAppendTextMessage()
        {
            this.N3MessageType = N3MessageType.KnubotAppendText;
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

        [AoMember(2)]
        public int Kind { get; set; }

        [AoMember(3, SerializeSize = ArraySizeType.Int32)]
        public string Text { get; set; }

        #endregion
    }
}
