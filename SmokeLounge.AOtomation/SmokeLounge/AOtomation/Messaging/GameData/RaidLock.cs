// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RaidLock.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the RaidLock type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// One raid lock of RaidMessage's list, as the client keeps it (two int pairs and an int): its lock list
    /// prints Id as the lock's id (and looks its playfield name up by it), prints Value as it is and counts the
    /// time left from Expires.
    /// </summary>
    public class RaidLock
    {
        #region AoMember Properties

        [AoMember(0)]
        public int Unknown1 { get; set; }

        [AoMember(1)]
        public int Id { get; set; }

        [AoMember(2)]
        public int Unknown2 { get; set; }

        [AoMember(3)]
        public uint Value { get; set; }

        [AoMember(4)]
        public int Expires { get; set; }

        #endregion
    }
}
