// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MissionInfo.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the MissionInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>One mission a terminal offers (a QuestAlternativeIIR_t entry): the quest, then a byte.</summary>
    public class MissionInfo
    {
        #region Constructors and Destructors

        public MissionInfo()
        {
            this.Quest = new Quest();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public Quest Quest { get; set; }

        [AoMember(1)]
        public byte Unknown1 { get; set; }

        #endregion
    }
}
