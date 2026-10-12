// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellArgument.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellArgument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using AOSharp.Common.GameData;

    /// <summary>One argument of a spell format: an int of a property, or a text slot (an int length, then its bytes).</summary>
    public class SpellArgument
    {
        #region Public Properties

        public bool IsString { get; private set; }

        public SpellPropertyOperator Property { get; private set; }

        public int StringIndex { get; private set; }

        #endregion

        #region Public Methods and Operators

        public static implicit operator SpellArgument(SpellPropertyOperator property)
        {
            return new SpellArgument { Property = property };
        }

        public static SpellArgument Text(int index)
        {
            return new SpellArgument { IsString = true, StringIndex = index };
        }

        #endregion
    }
}
