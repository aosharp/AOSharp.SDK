// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ScriptMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// ScriptIIR_t. What the fields are is read off the client's handler: the id keys its script slots, the kind
    /// is 0..4, the script is its text (kind 3: the music layer), the start is a time shifted by the client /
    /// server clock offset taken from the sent time, the duration is added to it, the district name is used by
    /// kinds 1 and 3, and X / Z are a position (kind 4). The client's reader returns success whatever the
    /// stream's state.
    /// </summary>
    [AoContract((int)N3MessageType.Script)]
    public class ScriptMessage : N3Message
    {
        #region Constructors and Destructors

        public ScriptMessage()
        {
            this.N3MessageType = N3MessageType.Script;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Id { get; set; }

        [AoMember(1)]
        public int Unknown1 { get; set; }

        [AoMember(2)]
        public int Kind { get; set; }

        [AoMember(3, SerializeSize = ArraySizeType.Int16)]
        public string Script { get; set; }

        [AoMember(4)]
        public int Start { get; set; }

        [AoMember(5)]
        public int Duration { get; set; }

        [AoMember(6)]
        public int SentAt { get; set; }

        [AoMember(7, SerializeSize = ArraySizeType.Int16)]
        public string DistrictName { get; set; }

        [AoMember(8)]
        public int X { get; set; }

        [AoMember(9)]
        public int Z { get; set; }

        #endregion
    }
}
