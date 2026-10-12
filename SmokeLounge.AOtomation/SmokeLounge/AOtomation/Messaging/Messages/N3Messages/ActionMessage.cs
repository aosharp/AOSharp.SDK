// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ActionMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// ActionIIR_t: an action on an item dynel (the message's identity): a version, then for version 1 the code and the
    /// character that did it; for a container 100 opens it and 0x66 closes it. Any other version is read as done with
    /// both left 0, and the client runs it so. Read and written by ActionSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.Action)]
    public class ActionMessage : N3Message
    {
        #region Constants

        public const int CurrentVersion = 1;

        #endregion

        #region Constructors and Destructors

        public ActionMessage()
        {
            this.N3MessageType = N3MessageType.Action;
            this.Version = CurrentVersion;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Code { get; set; }

        [AoMember(2)]
        public Identity Actor { get; set; }

        #endregion
    }
}
