// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellExpression.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellExpression type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A node of a spell's expression, by <see cref="Kind"/>: 1 a value, 2 a stat and a flag, 3 (compound) and 4
    /// (function) an operator with two nodes under it. Any other kind fails the read.
    /// </summary>
    public class SpellExpression
    {
        #region Constants

        public const int ValueKind = 1;

        public const int StatKind = 2;

        public const int CompoundKind = 3;

        public const int FunctionKind = 4;

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Kind { get; set; }

        [AoMember(1)]
        public float Value { get; set; }

        [AoMember(2)]
        public int Stat { get; set; }

        [AoMember(3)]
        public int Flag { get; set; }

        [AoMember(4)]
        public int Operator { get; set; }

        [AoMember(5)]
        public SpellExpression Left { get; set; }

        [AoMember(6)]
        public SpellExpression Right { get; set; }

        #endregion
    }
}
