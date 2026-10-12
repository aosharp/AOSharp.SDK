// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyInstanceData.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ProxyInstanceData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The block a 0xC77B proxy instance carries before its dynels.
    /// </summary>
    public class ProxyInstanceData
    {
        #region AoMember Properties

        [AoMember(0)]
        public int Unknown1 { get; set; }

        [AoMember(1)]
        public Identity Unknown2 { get; set; }

        [AoMember(2)]
        public int Unknown3 { get; set; }

        [AoMember(3)]
        public Vector3 Unknown4 { get; set; }

        [AoMember(4)]
        public int Unknown5 { get; set; }

        [AoMember(5)]
        public int Unknown6 { get; set; }

        [AoMember(6)]
        public float Unknown7 { get; set; }

        [AoMember(7)]
        public int Unknown8 { get; set; }

        [AoMember(8)]
        public int Unknown9 { get; set; }

        #endregion
    }
}
