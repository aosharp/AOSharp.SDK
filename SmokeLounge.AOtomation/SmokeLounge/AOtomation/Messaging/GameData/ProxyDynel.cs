// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyDynel.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ProxyDynel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A dynel in a proxy instance: its type, three ints and its instance.
    /// </summary>
    public class ProxyDynel
    {
        #region AoMember Properties

        [AoMember(0)]
        public IdentityType IdentityType { get; set; }

        [AoMember(1)]
        public int Unknown1 { get; set; }

        [AoMember(2)]
        public int Unknown2 { get; set; }

        [AoMember(3)]
        public int Unknown3 { get; set; }

        [AoMember(4)]
        public int Instance { get; set; }

        #endregion
    }
}
