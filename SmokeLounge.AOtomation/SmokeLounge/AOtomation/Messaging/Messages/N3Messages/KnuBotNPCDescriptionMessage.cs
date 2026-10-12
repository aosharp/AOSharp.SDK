// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnuBotNPCDescriptionMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the KnuBotNPCDescriptionMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// KnubotNPCDescriptionIIR_c, which the client sends to ask for an NPC's description: the base alone (the
    /// version and the NPC).
    /// </summary>
    [AoContract((int)N3MessageType.KnubotNPCDescription)]
    public class KnuBotNPCDescriptionMessage : N3Message
    {
        #region Constructors and Destructors

        public KnuBotNPCDescriptionMessage()
        {
            this.N3MessageType = N3MessageType.KnubotNPCDescription;
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

        #endregion
    }
}
