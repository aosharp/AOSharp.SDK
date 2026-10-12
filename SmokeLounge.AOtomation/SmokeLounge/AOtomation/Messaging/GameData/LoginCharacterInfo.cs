// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginCharacterInfo.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the LoginCharacterInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A character as the login server lists it: the client's CharacterData_t (its id, its PlayfieldProxy_t and
    /// whether it is created) with its CharacterInfo_c, and in a list the status word after it. Both records carry a
    /// version; older ones lack later fields, which keep the defaults here. Read and written by LoginStream.
    /// </summary>
    public class LoginCharacterInfo
    {
        #region Constructors and Destructors

        public LoginCharacterInfo()
        {
            this.Version = 4;
            this.PlayfieldProxyVersion = (byte)'a';
            this.CharacterInfoVersion = 5;
            this.Name = "name unknown";
            this.AreaName = "area unknown";
            this.BanReason = string.Empty;
        }

        #endregion

        #region AoMember Properties

        /// <summary>CharacterData_t's version (4 is written).</summary>
        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Id { get; set; }

        /// <summary>'a' (from version 4); any other fails the read.</summary>
        [AoMember(2)]
        public byte PlayfieldProxyVersion { get; set; }

        /// <summary>Where the character is: its instance is the location the character select window shows.</summary>
        [AoMember(3)]
        public Identity PlayfieldId { get; set; }

        [AoMember(4)]
        public int PlayfieldAttribute { get; set; }

        [AoMember(5)]
        public int ExitDoor { get; set; }

        [AoMember(6)]
        public Identity ExitDoorId { get; set; }

        [AoMember(7)]
        public bool Created { get; set; }

        /// <summary>Data versions below 3 only: an int after <see cref="Created"/>, not stored by the client.</summary>
        [AoMember(8)]
        public int Unknown1 { get; set; }

        /// <summary>CharacterInfo_c's version (5 is written).</summary>
        [AoMember(9)]
        public int CharacterInfoVersion { get; set; }

        /// <summary>The instance CharacterInfo_c carries; the client replaces it with <see cref="Id"/>.</summary>
        [AoMember(10)]
        public int CharacterId { get; set; }

        /// <summary>From info version 5.</summary>
        [AoMember(11)]
        public int OrgInstance { get; set; }

        /// <summary>A length of 0xff or more is read as "ERROR-CHANGE-NAME", its bytes left unread, as the client does.</summary>
        [AoMember(12)]
        public string Name { get; set; }

        [AoMember(13)]
        public Breed Breed { get; set; }

        [AoMember(14)]
        public Gender Gender { get; set; }

        [AoMember(15)]
        public Profession Profession { get; set; }

        [AoMember(16)]
        public int Level { get; set; }

        /// <summary>A length of 0xff or more is read as "ERROR-CHANGE-AREA", its bytes left unread.</summary>
        [AoMember(17)]
        public string AreaName { get; set; }

        [AoMember(18)]
        public int Banned { get; set; }

        [AoMember(19)]
        public string BanReason { get; set; }

        /// <summary>From info version 4.</summary>
        [AoMember(20)]
        public int HeadMesh { get; set; }

        /// <summary>From info version 4: the height.</summary>
        [AoMember(21)]
        public int MonsterScale { get; set; }

        /// <summary>From info version 4: the width.</summary>
        [AoMember(22)]
        public Fatness Fatness { get; set; }

        /// <summary>In a character list: bit 0 is "active".</summary>
        [AoMember(23)]
        public CharacterStatus Status { get; set; }

        #endregion
    }
}
