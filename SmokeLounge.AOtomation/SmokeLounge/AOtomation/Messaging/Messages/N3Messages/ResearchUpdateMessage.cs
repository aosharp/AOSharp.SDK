// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResearchUpdateMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ResearchUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// ResearchUpdateIIR: a byte, then research lines (a perk id and three ints) until a perk id of 0. Read and
    /// written by ResearchUpdateSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.ResearchUpdate)]
    public class ResearchUpdateMessage : N3Message
    {
        #region Constructors and Destructors

        public ResearchUpdateMessage()
        {
            this.N3MessageType = N3MessageType.ResearchUpdate;
            this.ResearchLines = new ResearchLine[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public byte Unknown1 { get; set; }

        [AoMember(1)]
        public ResearchLine[] ResearchLines { get; set; }

        #endregion
    }
}
