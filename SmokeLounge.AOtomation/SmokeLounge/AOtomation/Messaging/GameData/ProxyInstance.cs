// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyInstance.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ProxyInstance type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A proxy instance's contents (identity types 0xC77B and 0xC77D), as a PlayfieldAnarchyF carries them inline: an
    /// int, for 0xC77B a block more, then the dynels in it.
    /// </summary>
    public class ProxyInstance
    {
        #region Constructors and Destructors

        public ProxyInstance()
        {
            this.Dynels = new List<ProxyDynel>();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Unknown1 { get; set; }

        /// <summary>0xC77B only.</summary>
        [AoMember(1)]
        public ProxyInstanceData Data { get; set; }

        [AoMember(2)]
        public List<ProxyDynel> Dynels { get; set; }

        #endregion
    }
}
