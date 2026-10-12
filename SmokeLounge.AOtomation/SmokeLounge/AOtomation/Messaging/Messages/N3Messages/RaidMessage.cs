// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RaidMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the RaidMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// RaidIIR_c: a short command; for <see cref="LocksCommand"/> a list version, then (version 1) the raid locks.
    /// Read and written by RaidSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.Raid)]
    public class RaidMessage : N3Message
    {
        #region Constants

        public const short CreatedCommand = 0;

        public const short LocksCommand = 1;

        /// <summary>The only lock list version the client reads.</summary>
        public const int LockListVersion = 1;

        #endregion

        #region Constructors and Destructors

        public RaidMessage()
        {
            this.N3MessageType = N3MessageType.Raid;
            this.Version = LockListVersion;
            this.Locks = new RaidLock[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public short Command { get; set; }

        /// <summary>The lock list's version (only with <see cref="LocksCommand"/>); another than 1 has no locks.</summary>
        [AoMember(1)]
        public int Version { get; set; }

        [AoMember(2)]
        public RaidLock[] Locks { get; set; }

        #endregion
    }
}
