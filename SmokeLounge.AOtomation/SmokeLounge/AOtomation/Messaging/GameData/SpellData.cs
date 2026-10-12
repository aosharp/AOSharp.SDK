// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellData.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// One spell as the client sends it (SpellData_t): its function, its criteria, then its arguments, which
    /// <see cref="SpellFormats"/> lays out per function: four every function has (Duration, Interval, ApplyOn,
    /// TargetList), then the function's own. Hit adds the check that Min and Max are not both 0, Criteria a
    /// criteria list and Expression an expression.
    /// </summary>
    public class SpellData
    {
        #region Constructors and Destructors

        public SpellData()
        {
            this.Version = SpellFormats.Version;
            this.Criteria = new SpellCriterion[0];
            this.Properties = new Dictionary<SpellPropertyOperator, int>();
            this.Strings = new string[SpellFormats.StringCount];
            this.TextureMarker = SpellFormats.TextureMarker;
            this.SpecialCriteria = new SpellCriterion[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public SpellFunction Function { get; set; }

        [AoMember(1)]
        public int Unknown1 { get; set; }

        /// <summary>The format's version (4); any other fails the read.</summary>
        [AoMember(2)]
        public int Version { get; set; }

        /// <summary>At most 1000.</summary>
        [AoMember(3)]
        public SpellCriterion[] Criteria { get; set; }

        /// <summary>The function's int arguments by property.</summary>
        [AoMember(4)]
        public Dictionary<SpellPropertyOperator, int> Properties { get; set; }

        /// <summary>The function's text arguments by slot (most have one; SendMail has three).</summary>
        [AoMember(5)]
        public string[] Strings { get; set; }

        /// <summary>
        /// ApplyTexture only: sent after the texture. 100 and up, BodyPart, SeeSkin, Arg1 and Arg2 follow; under 100 it is
        /// the body part itself and only SeeSkin follows (Arg1 and Arg2 are then 0).
        /// </summary>
        [AoMember(6)]
        public int TextureMarker { get; set; }

        /// <summary>The Criteria function only: a 0x3F1 list after the arguments.</summary>
        [AoMember(7)]
        public SpellCriterion[] SpecialCriteria { get; set; }

        /// <summary>Expression only: the expression after the arguments.</summary>
        [AoMember(8)]
        public SpellExpression Expression { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>An int argument, or 0 when the function has none of that property.</summary>
        public int Get(SpellPropertyOperator property)
        {
            int value;
            return this.Properties.TryGetValue(property, out value) ? value : 0;
        }

        #endregion
    }
}
