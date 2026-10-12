// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterListMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CharacterListMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.SystemMessages
{
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The account's characters: a count, each character (CharacterData_t, then its status word), the slots, the
    /// expansions and whether shadowlands professions are on. Servers that leave the last off are read as the client
    /// reads them (off, the read not failed). Read and written by CharacterListSerializer.
    /// </summary>
    [AoContract((int)SystemMessageType.CharacterList)]
    public class CharacterListMessage : SystemMessage
    {
        #region Constructors and Destructors

        public CharacterListMessage()
        {
            this.SystemMessageType = SystemMessageType.CharacterList;
            this.Characters = new LoginCharacterInfo[0];
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public LoginCharacterInfo[] Characters { get; set; }

        /// <summary>The character slots.</summary>
        [AoMember(1)]
        public int AllowedCharacters { get; set; }

        [AoMember(2)]
        public int Expansions { get; set; }

        [AoMember(3)]
        public bool ShadowlandsProfessionsEnabled { get; set; }

        #endregion
    }
}
