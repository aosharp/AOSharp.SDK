// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellCriterion.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellCriterion type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>A requirement: two values and an operator (below 146, else the read fails).</summary>
    public class SpellCriterion
    {
        #region AoMember Properties

        [AoMember(0)]
        public int Value1 { get; set; }

        [AoMember(1)]
        public int Value2 { get; set; }

        [AoMember(2)]
        public int Operator { get; set; }

        #endregion
    }
}
