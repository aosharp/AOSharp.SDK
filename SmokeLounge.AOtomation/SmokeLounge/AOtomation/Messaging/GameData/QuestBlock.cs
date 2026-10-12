// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestBlock.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestBlock type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// An entry of a quest's block list: an identity and a length-prefixed block. The client reads past the block
    /// without looking into it, so its layout is not known; it is kept as sent.
    /// </summary>
    public class QuestBlock
    {
        #region Constructors and Destructors

        public QuestBlock()
        {
            this.Data = new byte[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public Identity Identity { get; set; }

        [AoMember(1)]
        public byte[] Data { get; set; }

        #endregion
    }
}
