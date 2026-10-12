// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginStream.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the LoginStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using SmokeLounge.AOtomation.Messaging.GameData;

    /// <summary>The client's CharacterData_t and CharacterInfo_c, read and written as it does (MessageProtocol).</summary>
    internal static class LoginStream
    {
        #region Public Methods and Operators

        /// <summary>CharacterData_t's ReadDataStream then ReadBlobStream; false (the read failed) on a bad proxy version.</summary>
        public static bool ReadCharacter(StreamReader r, LoginCharacterInfo c)
        {
            c.Version = r.ReadInt32();
            c.Id = r.ReadInt32();
            if (c.Version < 4)
            {
                // An old record carries only the playfield; the two types are the client's constants.
                var playfield = r.ReadInt32();
                c.PlayfieldProxyVersion = 0;
                c.PlayfieldId = new Identity { Type = (IdentityType)0xc79c, Instance = playfield };
                c.ExitDoorId = new Identity { Type = (IdentityType)0x9c50, Instance = playfield };
            }
            else
            {
                c.PlayfieldProxyVersion = r.ReadByte();
                if (c.PlayfieldProxyVersion != (byte)'a')
                {
                    r.Fail();
                    return false;
                }

                c.PlayfieldId = new Identity { Type = (IdentityType)r.ReadInt32(), Instance = r.ReadInt32() };
                c.PlayfieldAttribute = r.ReadInt32();
                c.ExitDoor = r.ReadInt32();
                c.ExitDoorId = new Identity { Type = (IdentityType)r.ReadInt32(), Instance = r.ReadInt32() };
            }

            c.Created = c.Version < 2 || r.ReadInt32() != 0;
            if (c.Version < 3)
            {
                c.Unknown1 = r.ReadInt32();
            }

            ReadInfo(r, c);
            return true;
        }

        /// <summary>CharacterData_t's WriteStream: always version 4, then the info (version 5).</summary>
        public static void WriteCharacter(StreamWriter w, LoginCharacterInfo c)
        {
            w.WriteInt32(4);
            w.WriteInt32(c.Id);
            w.WriteByte((byte)'a');
            w.WriteInt32((int)c.PlayfieldId.Type);
            w.WriteInt32(c.PlayfieldId.Instance);
            w.WriteInt32(c.PlayfieldAttribute);
            w.WriteInt32(c.ExitDoor);
            w.WriteInt32((int)c.ExitDoorId.Type);
            w.WriteInt32(c.ExitDoorId.Instance);
            w.WriteInt32(c.Created ? 1 : 0);
            w.WriteInt32(5);
            w.WriteInt32(c.CharacterId);
            w.WriteInt32(c.OrgInstance);
            WriteString(w, c.Name);
            w.WriteInt32((int)c.Breed);
            w.WriteInt32((int)c.Gender);
            w.WriteInt32((int)c.Profession);
            w.WriteInt32(c.Level);
            WriteString(w, c.AreaName);
            w.WriteInt32(c.Banned);
            WriteString(w, c.BanReason);
            w.WriteInt32(c.HeadMesh);
            w.WriteInt32(c.MonsterScale);
            w.WriteInt32((int)c.Fatness);
        }

        #endregion

        #region Methods

        /// <summary>CharacterInfo_c's ReadStream.</summary>
        private static void ReadInfo(StreamReader r, LoginCharacterInfo c)
        {
            c.CharacterInfoVersion = r.ReadInt32();
            if (c.CharacterInfoVersion == 2)
            {
                c.Breed = (Breed)r.ReadInt32();
                c.Gender = (Gender)r.ReadInt32();
                c.Name = r.ReadString(0x20);
                return;
            }

            c.CharacterId = r.ReadInt32();
            if (c.CharacterInfoVersion > 4)
            {
                c.OrgInstance = r.ReadInt32();
            }

            var nameLength = r.ReadInt32();
            if (nameLength != 0)
            {
                c.Name = nameLength < 0xff ? r.ReadString(nameLength) : "ERROR-CHANGE-NAME";
            }

            c.Breed = (Breed)r.ReadInt32();
            c.Gender = (Gender)r.ReadInt32();
            c.Profession = (Profession)r.ReadInt32();
            c.Level = r.ReadInt32();

            var areaLength = r.ReadInt32();
            if (areaLength != 0)
            {
                c.AreaName = areaLength < 0xff ? r.ReadString(areaLength) : "ERROR-CHANGE-AREA";
            }

            c.Banned = r.ReadInt32();
            var banLength = r.ReadInt32();
            if (banLength != 0)
            {
                c.BanReason = r.ReadString(banLength);
            }

            if (c.CharacterInfoVersion > 3)
            {
                c.HeadMesh = r.ReadInt32();
                c.MonsterScale = r.ReadInt32();
                c.Fatness = (Fatness)r.ReadInt32();
            }
        }

        /// <summary>A std::string as the client writes it: its length, then its bytes.</summary>
        private static void WriteString(StreamWriter w, string text)
        {
            text = text ?? string.Empty;
            w.WriteInt32(text.Length);
            w.WriteString(text);
        }

        #endregion
    }
}
