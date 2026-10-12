// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterDeletedMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CharacterDeletedMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.SystemMessages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A character was deleted. The client reads no body: the one deleted is the one it asked for.
    /// </summary>
    [AoContract((int)SystemMessageType.CharacterDeleted)]
    public class CharacterDeletedMessage : SystemMessage
    {
        #region Constructors and Destructors

        public CharacterDeletedMessage()
        {
            this.SystemMessageType = SystemMessageType.CharacterDeleted;
        }

        #endregion
    }
}
