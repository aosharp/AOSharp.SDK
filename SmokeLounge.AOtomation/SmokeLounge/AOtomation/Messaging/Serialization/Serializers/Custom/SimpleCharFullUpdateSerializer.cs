// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleCharFullUpdateSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SimpleCharFullUpdateSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    // TODO: Check the client side of this message for the possibly missing parts.
    public class SimpleCharFullUpdateSerializer : ISerializer
    {
        #region Fields

        private readonly Type type;

        #endregion

        #region Constructors and Destructors

        public SimpleCharFullUpdateSerializer()
        {
            this.type = typeof(SimpleCharFullUpdateMessage);
        }

        #endregion

        #region Public Properties

        public Type Type
        {
            get
            {
                return this.type;
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader, 
            SerializationContext serializationContext, 
            PropertyMetaData propertyMetaData = null)
        {
            // SimpleCharFullUpdateIIR_t as the client reads it. Where its reader throws, the read fails here (the
            // message is dropped).
            var m = new SimpleCharFullUpdateMessage();
            var r = streamReader;
            m.N3MessageType = (N3MessageType)r.ReadInt32();
            m.Identity = ReadIdentity(r);
            m.Unknown = r.ReadByte();

            // Versions 0x39 and 0x3a only.
            m.Version = r.ReadByte();
            if ((byte)(m.Version - 0x39) > 1)
            {
                r.Fail();
                return m;
            }

            m.Flags = (SimpleCharFullUpdateFlags)r.ReadInt32();
            var flags = (int)m.Flags;
            if ((flags & 0x40) != 0)
            {
                m.PlayfieldId = r.ReadInt32();
            }

            if ((flags & 0x20) != 0)
            {
                m.FightingTarget = ReadIdentity(r);
            }

            m.Position = ReadVector3(r);
            m.Heading = (flags & 0x200) != 0
                            ? new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle())
                            : Quaternion.Identity;
            m.Appearance = new Appearance { Value = r.ReadUInt32() };

            // A byte length, then the bytes; the client forces the last to NUL, so the name is at most length - 1.
            var nameLength = r.ReadByte();
            var name = r.ReadString(nameLength);
            m.Name = name.Length >= nameLength && nameLength > 0 ? name.Substring(0, nameLength - 1) : name;
            m.CharacterFlags = (CharacterFlags)r.ReadInt32();

            // One int: its high half, then its low half.
            m.AccountFlags = r.ReadInt16();
            m.Expansions = r.ReadInt16();

            if ((flags & 1) == 0)
            {
                var p = new SimpleCharInfo.PlayerInfo();
                m.CharacterInfo = p;
                p.CurrentNano = r.ReadUInt32();
                p.Team = r.ReadInt32();
                p.Swim = r.ReadInt16();
                p.StrengthBase = r.ReadInt16();
                p.AgilityBase = r.ReadInt16();
                p.StaminaBase = r.ReadInt16();
                p.IntelligenceBase = r.ReadInt16();
                p.SenseBase = r.ReadInt16();
                p.PsychicBase = r.ReadInt16();
                if (((int)m.CharacterFlags & 0x400000) != 0)
                {
                    p.FirstName = ReadString16(r);
                    p.LastName = ReadString16(r);
                }

                if ((flags & 0x4000000) != 0)
                {
                    if (m.Version > 0x39)
                    {
                        p.OrgId = r.ReadInt32();
                    }

                    p.OrgName = ReadString16(r);
                }
            }
            else
            {
                var n = new SimpleCharInfo.NPCInfo();
                m.CharacterInfo = n;
                n.Family = (flags & 0x20000) != 0 ? r.ReadByte() : r.ReadInt16();
                n.LosHeight = (flags & 0x80000) != 0 ? r.ReadByte() : r.ReadInt16();

                // A byte with 0x2000000, else an int16; then an int16 count, and a byte only when it is positive.
                n.Unknown1 = (flags & 0x2000000) != 0 ? r.ReadByte() : r.ReadInt16();
                n.Unknown2 = r.ReadInt16();
                if (n.Unknown2 > 0)
                {
                    m.ScfuTowerUnk = r.ReadByte();
                }
            }

            m.Level = (flags & 0x1000) != 0 ? r.ReadInt16() : r.ReadByte();
            m.Health = (flags & 0x800) != 0 ? r.ReadUInt16() : r.ReadInt32();
            m.HealthDamage = (flags & 0x4000) != 0 ? r.ReadByte()
                             : (flags & 0x800) != 0 ? r.ReadUInt16()
                             : r.ReadInt32();
            m.MonsterData = r.ReadUInt32();
            m.MonsterScale = r.ReadInt16();
            m.VisualFlags = r.ReadInt16();
            m.VisibleTitle = r.ReadByte();

            // An int length, then that many bytes.
            var movement = r.ReadBytes(r.ReadInt32());
            if (movement.Length >= CharMovementStatus.Size)
            {
                m.MovementStatus = CharMovementStatus.FromBytes(movement);
            }
            else
            {
                m.UnkMovementStatus = movement;
            }

            if ((flags & 0x80) != 0)
            {
                m.HeadMesh = r.ReadInt32();
            }

            m.RunSpeedBase = (flags & 0x2000) != 0 ? r.ReadInt16() : r.ReadByte();
            if ((flags & 0x400) != 0)
            {
                if ((flags & 0x20) != 0)
                {
                    m.EarlyFightingTarget = m.FightingTarget;
                }

                m.FightingTarget = ReadIdentity(r);
            }

            // 32 name bytes, the texture, two ints (an unchecked 0x3F1 count; a negative one fails the read).
            if ((flags & 0x10) != 0)
            {
                var count = ReadCount(r, false);
                if (count < 0)
                {
                    r.Fail();
                    return m;
                }

                var overrides = new List<SimpleCharInfo.TextureOverride>();
                for (var i = 0; i < count && !r.Failed; i++)
                {
                    overrides.Add(new SimpleCharInfo.TextureOverride
                    {
                        Name = r.ReadString(32),
                        TextureId = r.ReadInt32(),
                        Unknown1 = r.ReadInt32(),
                        Unknown2 = r.ReadInt32()
                    });
                }

                m.TextureOverrides = overrides.ToArray();
            }

            if ((flags & 0x800000) != 0)
            {
                m.ImmuneUnknown = r.ReadByte();
            }

            if ((flags & 0x1000000) != 0)
            {
                m.Flag3Unknown = r.ReadByte();
            }

            // The active nanos: identity, nano instance, two times.
            var nanoCount = ReadCount(r, true);
            if (nanoCount < 0)
            {
                return m;
            }

            var nanos = new List<SimpleCharInfo.ActiveNano>();
            for (var i = 0; i < nanoCount && !r.Failed; i++)
            {
                nanos.Add(new SimpleCharInfo.ActiveNano
                {
                    Identity = ReadIdentity(r),
                    NanoInstance = r.ReadInt32(),
                    Time1 = r.ReadInt32(),
                    Time2 = r.ReadInt32()
                });
            }

            m.ActiveNanos = nanos.ToArray();

            if ((flags & 0x10000) != 0)
            {
                m.WaypointTarget = ReadIdentity(r);
                var points = r.ReadInt32();
                m.Waypoints = new List<Vector3>();
                for (var i = 0; i < points && !r.Failed; i++)
                {
                    m.Waypoints.Add(ReadVector3(r));
                }
            }

            // The clothes: a packed int (low half the place; a positive high half adds two ints), then two ints.
            m.Textures = ReadClothes(r);

            // The meshes: byte, int, int, byte.
            var meshCount = ReadCount(r, true);
            if (meshCount < 0)
            {
                return m;
            }

            var meshes = new List<Mesh>();
            for (var i = 0; i < meshCount && !r.Failed; i++)
            {
                meshes.Add(new Mesh
                {
                    Position = r.ReadByte(),
                    Id = r.ReadUInt32(),
                    OverrideTextureId = r.ReadInt32(),
                    Layer = r.ReadByte()
                });
            }

            m.Meshes = meshes.ToArray();

            // Four ints each.
            if ((flags & 0x100) != 0)
            {
                var pairs = ReadCount(r, true);
                if (pairs < 0)
                {
                    return m;
                }

                var weaponPairs = new List<SimpleCharInfo.WeaponPair>();
                for (var i = 0; i < pairs && !r.Failed; i++)
                {
                    weaponPairs.Add(new SimpleCharInfo.WeaponPair
                    {
                        Unknown1 = r.ReadInt32(),
                        Unknown2 = r.ReadInt32(),
                        Unknown3 = r.ReadInt32(),
                        Unknown4 = r.ReadInt32()
                    });
                }

                m.WeaponPairs = weaponPairs.ToArray();
            }

            if ((flags & 0x20000000) != 0)
            {
                m.Flag4Unknown = r.ReadByte();
            }

            // An identity list.
            if ((flags & 0x40000000) != 0)
            {
                var ids = ReadCount(r, true);
                if (ids < 0)
                {
                    return m;
                }

                var identities = new List<Identity>();
                for (var i = 0; i < ids && !r.Failed; i++)
                {
                    identities.Add(ReadIdentity(r));
                }

                m.Identities = identities.ToArray();
            }

            m.Flags2 = (ScfuFlags2)r.ReadInt32();
            var flags2 = (int)m.Flags2;
            if ((flags2 & 1) != 0)
            {
                // A count and that many int pairs; then an int and an identity.
                var count = r.ReadInt32();
                var pairList = new List<GameTuple<int, int>>();
                for (var i = 0; i < count && !r.Failed; i++)
                {
                    pairList.Add(new GameTuple<int, int> { Value1 = r.ReadInt32(), Value2 = r.ReadInt32() });
                }

                m.Pairs = pairList.ToArray();
                m.PairsUnknown = r.ReadInt32();
                m.PairsIdentity = ReadIdentity(r);
            }

            if ((flags2 & 2) != 0)
            {
                m.Flags2Unknown = r.ReadByte();
            }

            if ((flags2 & 4) != 0)
            {
                m.Owner = new Identity(IdentityType.SimpleChar, r.ReadInt32());
            }

            m.ScfuUnk2 = r.ReadByte();
            return m;
        }

        public Expression DeserializerExpression(
            ParameterExpression streamReaderExpression, 
            ParameterExpression serializationContextExpression, 
            Expression assignmentTargetExpression, 
            PropertyMetaData propertyMetaData)
        {
            var deserializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                    <SimpleCharFullUpdateSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
                    (o => o.Deserialize);
            var serializerExp = Expression.New(this.GetType());
            var callExp = Expression.Call(
                serializerExp, 
                deserializerMethodInfo, 
                new Expression[]
                    {
                        streamReaderExpression, serializationContextExpression, 
                        Expression.Constant(propertyMetaData, typeof(PropertyMetaData))
                    });

            var assignmentExp = Expression.Assign(
                assignmentTargetExpression, Expression.TypeAs(callExp, assignmentTargetExpression.Type));
            return assignmentExp;
        }

        public void Serialize(
            StreamWriter streamWriter, 
            SerializationContext serializationContext, 
            object value, 
            PropertyMetaData propertyMetaData = null)
        {
            // SimpleCharFullUpdateIIR_t as the client reads it: what is sent follows the message's flags.
            var m = (SimpleCharFullUpdateMessage)value;
            var w = streamWriter;
            w.WriteInt32((int)m.N3MessageType);
            WriteIdentity(w, m.Identity);
            w.WriteByte(m.Unknown);
            w.WriteByte(m.Version);
            w.WriteInt32((int)m.Flags);
            var flags = (int)m.Flags;
            if ((flags & 0x40) != 0)
            {
                w.WriteInt32(m.PlayfieldId ?? 0);
            }

            if ((flags & 0x20) != 0)
            {
                WriteIdentity(w, ((flags & 0x400) != 0 ? m.EarlyFightingTarget : m.FightingTarget) ?? default(Identity));
            }

            WriteVector3(w, m.Position);
            if ((flags & 0x200) != 0)
            {
                w.WriteSingle(m.Heading.X);
                w.WriteSingle(m.Heading.Y);
                w.WriteSingle(m.Heading.Z);
                w.WriteSingle(m.Heading.W);
            }

            w.WriteUInt32(m.Appearance == null ? 0 : m.Appearance.Value);
            // The name with its NUL, which the length counts.
            var name = m.Name ?? string.Empty;
            w.WriteByte((byte)(name.Length + 1));
            w.WriteString(name);
            w.WriteByte(0);
            w.WriteInt32((int)m.CharacterFlags);
            w.WriteInt16(m.AccountFlags);
            w.WriteInt16(m.Expansions);

            if ((flags & 1) == 0)
            {
                var p = m.CharacterInfo as SimpleCharInfo.PlayerInfo ?? new SimpleCharInfo.PlayerInfo();
                w.WriteUInt32(p.CurrentNano);
                w.WriteInt32(p.Team);
                w.WriteInt16(p.Swim);
                w.WriteInt16(p.StrengthBase);
                w.WriteInt16(p.AgilityBase);
                w.WriteInt16(p.StaminaBase);
                w.WriteInt16(p.IntelligenceBase);
                w.WriteInt16(p.SenseBase);
                w.WriteInt16(p.PsychicBase);
                if (((int)m.CharacterFlags & 0x400000) != 0)
                {
                    WriteString16(w, p.FirstName);
                    WriteString16(w, p.LastName);
                }

                if ((flags & 0x4000000) != 0)
                {
                    if (m.Version > 0x39)
                    {
                        w.WriteInt32(p.OrgId);
                    }

                    WriteString16(w, p.OrgName);
                }
            }
            else
            {
                var n = m.CharacterInfo as SimpleCharInfo.NPCInfo ?? new SimpleCharInfo.NPCInfo();
                WriteSized(w, n.Family, (flags & 0x20000) != 0);
                WriteSized(w, n.LosHeight, (flags & 0x80000) != 0);
                WriteSized(w, n.Unknown1, (flags & 0x2000000) != 0);
                w.WriteInt16(n.Unknown2);
                if (n.Unknown2 > 0)
                {
                    w.WriteByte(m.ScfuTowerUnk);
                }
            }

            WriteSized(w, m.Level, (flags & 0x1000) != 0 ? 2 : 1);
            WriteSized(w, m.Health, (flags & 0x800) != 0 ? 2 : 4);
            WriteSized(w, m.HealthDamage, (flags & 0x4000) != 0 ? 1 : (flags & 0x800) != 0 ? 2 : 4);
            w.WriteUInt32(m.MonsterData);
            w.WriteInt16(m.MonsterScale);
            w.WriteInt16(m.VisualFlags);
            w.WriteByte(m.VisibleTitle);

            var movement = m.MovementStatus.HasValue ? m.MovementStatus.Value.ToBytes() : m.UnkMovementStatus ?? new byte[0];
            w.WriteInt32(movement.Length);
            w.WriteBytes(movement);

            if ((flags & 0x80) != 0)
            {
                w.WriteInt32(m.HeadMesh ?? 0);
            }

            WriteSized(w, m.RunSpeedBase, (flags & 0x2000) != 0 ? 2 : 1);
            if ((flags & 0x400) != 0)
            {
                WriteIdentity(w, m.FightingTarget ?? default(Identity));
            }

            if ((flags & 0x10) != 0)
            {
                var overrides = m.TextureOverrides ?? new SimpleCharInfo.TextureOverride[0];
                WriteCount(w, overrides.Length);
                foreach (var o in overrides)
                {
                    w.WriteString(o.Name ?? string.Empty, 32);
                    w.WriteInt32(o.TextureId);
                    w.WriteInt32(o.Unknown1);
                    w.WriteInt32(o.Unknown2);
                }
            }

            if ((flags & 0x800000) != 0)
            {
                w.WriteByte(m.ImmuneUnknown);
            }

            if ((flags & 0x1000000) != 0)
            {
                w.WriteByte(m.Flag3Unknown);
            }

            var nanos = m.ActiveNanos ?? new SimpleCharInfo.ActiveNano[0];
            WriteCount(w, nanos.Length);
            foreach (var nano in nanos)
            {
                WriteIdentity(w, nano.Identity);
                w.WriteInt32(nano.NanoInstance);
                w.WriteInt32(nano.Time1);
                w.WriteInt32(nano.Time2);
            }

            if ((flags & 0x10000) != 0)
            {
                WriteIdentity(w, m.WaypointTarget);
                var points = m.Waypoints ?? new List<Vector3>();
                w.WriteInt32(points.Count);
                foreach (var point in points)
                {
                    WriteVector3(w, point);
                }
            }

            WriteClothes(w, m.Textures);

            var meshes = m.Meshes ?? new Mesh[0];
            WriteCount(w, meshes.Length);
            foreach (var mesh in meshes)
            {
                w.WriteByte(mesh.Position);
                w.WriteUInt32(mesh.Id);
                w.WriteInt32(mesh.OverrideTextureId);
                w.WriteByte(mesh.Layer);
            }

            if ((flags & 0x100) != 0)
            {
                var weaponPairs = m.WeaponPairs ?? new SimpleCharInfo.WeaponPair[0];
                WriteCount(w, weaponPairs.Length);
                foreach (var pair in weaponPairs)
                {
                    w.WriteInt32(pair.Unknown1);
                    w.WriteInt32(pair.Unknown2);
                    w.WriteInt32(pair.Unknown3);
                    w.WriteInt32(pair.Unknown4);
                }
            }

            if ((flags & 0x20000000) != 0)
            {
                w.WriteByte(m.Flag4Unknown);
            }

            if ((flags & 0x40000000) != 0)
            {
                var identities = m.Identities ?? new Identity[0];
                WriteCount(w, identities.Length);
                foreach (var identity in identities)
                {
                    WriteIdentity(w, identity);
                }
            }

            w.WriteInt32((int)m.Flags2);
            var flags2 = (int)m.Flags2;
            if ((flags2 & 1) != 0)
            {
                var pairs = m.Pairs ?? new GameTuple<int, int>[0];
                w.WriteInt32(pairs.Length);
                foreach (var pair in pairs)
                {
                    w.WriteInt32(pair.Value1);
                    w.WriteInt32(pair.Value2);
                }

                w.WriteInt32(m.PairsUnknown);
                WriteIdentity(w, m.PairsIdentity);
            }

            if ((flags2 & 2) != 0)
            {
                w.WriteByte(m.Flags2Unknown);
            }

            if ((flags2 & 4) != 0)
            {
                w.WriteInt32(m.Owner.HasValue ? m.Owner.Value.Instance : 0);
            }

            w.WriteByte(m.ScfuUnk2);
        }

        public Expression SerializerExpression(
            ParameterExpression streamWriterExpression, 
            ParameterExpression serializationContextExpression, 
            Expression valueExpression, 
            PropertyMetaData propertyMetaData)
        {
            var serializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                    <SimpleCharFullUpdateSerializer, 
                        Action<StreamWriter, SerializationContext, object, PropertyMetaData>>(o => o.Serialize);
            var serializerExp = Expression.New(this.GetType());
            var callExp = Expression.Call(
                serializerExp, 
                serializerMethodInfo, 
                new[]
                    {
                        streamWriterExpression, serializationContextExpression, valueExpression, 
                        Expression.Constant(propertyMetaData, typeof(PropertyMetaData))
                    });
            return callExp;
        }

        #endregion

        #region Methods

        private static Identity ReadIdentity(StreamReader r)
        {
            return new Identity((IdentityType)r.ReadInt32(), r.ReadInt32());
        }

        private static void WriteIdentity(StreamWriter w, Identity identity)
        {
            w.WriteInt32((int)identity.Type);
            w.WriteInt32(identity.Instance);
        }

        private static void WriteVector3(StreamWriter w, Vector3 vector)
        {
            w.WriteSingle(vector.X);
            w.WriteSingle(vector.Y);
            w.WriteSingle(vector.Z);
        }

        private static void WriteString16(StreamWriter w, string text)
        {
            text = text ?? string.Empty;
            w.WriteUInt16((ushort)text.Length);
            w.WriteString(text);
        }

        private static void WriteCount(StreamWriter w, int count)
        {
            w.WriteInt32((count + 1) * 0x3f1);
        }

        /// <summary>A value sent as a byte when <paramref name="small"/>, else as an int16.</summary>
        private static void WriteSized(StreamWriter w, int value, bool small)
        {
            WriteSized(w, value, small ? 1 : 2);
        }

        private static void WriteSized(StreamWriter w, int value, int size)
        {
            if (size == 1)
            {
                w.WriteByte((byte)value);
            }
            else if (size == 2)
            {
                w.WriteInt16((short)value);
            }
            else
            {
                w.WriteInt32(value);
            }
        }

        /// <summary>
        /// The clothes (an unchecked 0x3F1 count): a packed int, the texture and an int; a positive packed int carries
        /// the place in its low half and Unknown1 in its high half, and a positive high half adds two ints.
        /// </summary>
        internal static Texture[] ReadClothes(StreamReader r)
        {
            var count = r.ReadInt32() / 0x3f1 - 1;
            var textures = new List<Texture>();
            for (var i = 0; i < count && !r.Failed; i++)
            {
                var packed = r.ReadInt32();
                var texture = new Texture { Id = r.ReadInt32(), Unknown = r.ReadInt32() };
                if (packed > 0)
                {
                    texture.Place = (short)packed;
                    texture.Unknown1 = (short)(packed >> 16);
                    if (texture.Unknown1 > 0)
                    {
                        texture.Unknown2 = r.ReadInt32();
                        texture.Unknown3 = r.ReadInt32();
                    }
                }
                else
                {
                    texture.Place = packed;
                }

                textures.Add(texture);
            }

            return textures.ToArray();
        }

        internal static void WriteClothes(StreamWriter w, Texture[] textures)
        {
            textures = textures ?? new Texture[0];
            WriteCount(w, textures.Length);
            foreach (var texture in textures)
            {
                w.WriteInt32(texture.Unknown1 > 0 ? (texture.Unknown1 << 16) | (ushort)texture.Place : texture.Place);
                w.WriteInt32(texture.Id);
                w.WriteInt32(texture.Unknown);
                if (texture.Unknown1 > 0)
                {
                    w.WriteInt32(texture.Unknown2);
                    w.WriteInt32(texture.Unknown3);
                }
            }
        }

        private static Vector3 ReadVector3(StreamReader r)
        {
            return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        }

        /// <summary>A 16-bit length, then the bytes: 0 is empty, 0x8000 and above fails the read.</summary>
        private static string ReadString16(StreamReader r)
        {
            var n = r.ReadUInt16();
            if (n == 0)
            {
                return string.Empty;
            }

            if (n >= 0x8000)
            {
                r.Fail();
                return string.Empty;
            }

            return r.ReadString(n);
        }

        /// <summary>
        /// A list count sent as (n + 1) * 0x3f1. The client's Gamecode lists (<paramref name="checkedList"/>) fail the
        /// read on a remainder or more than 30000 entries (-1 here, after failing it); its GameData lists divide without
        /// checking.
        /// </summary>
        private static int ReadCount(StreamReader r, bool checkedList)
        {
            var raw = r.ReadInt32();
            if (!checkedList)
            {
                return raw / 0x3f1 - 1;
            }

            var u = unchecked((uint)raw);
            var n = u / 0x3f1 - 1;
            if (u % 0x3f1 != 0 || n > 30000)
            {
                r.Fail();
                return -1;
            }

            return (int)n;
        }

        #endregion
    }
}