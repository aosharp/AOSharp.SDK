// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCharacterMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CreateCharacterMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.SystemMessages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// Creates a character: a CharacterData_t (version 4, id 0, its CharacterInfo_c carrying the choices), then the
    /// starter area. Written and read by CreateCharacterSerializer.
    /// </summary>
    [AoContract((int)SystemMessageType.CreateCharacter)]
    public class CreateCharacterMessage : SystemMessage
    {
        #region Constructors and Destructors

        public CreateCharacterMessage()
        {
            this.SystemMessageType = SystemMessageType.CreateCharacter;
            this.Character = new GameData.LoginCharacterInfo();
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public GameData.LoginCharacterInfo Character { get; set; }

        [AoMember(1)]
        public StarterArea StarterArea { get; set; }

        #endregion
    }
}
