// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CorpseFullUpdateSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CorpseFullUpdateSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    public class CorpseFullUpdateSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(CorpseFullUpdateMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            // CorpseFullUpdateIIR_t as the client reads it; where it rejects the data, the read fails here.
            var m = new CorpseFullUpdateMessage();
            var r = streamReader;
            m.N3MessageType = (N3MessageType)r.ReadInt32();
            m.Identity = ReadIdentity(r);
            m.Unknown = r.ReadByte();

            m.Version = r.ReadInt32();
            if (m.Version != 8)
            {
                r.Fail();
                return m;
            }

            m.Unknown2 = r.ReadInt32();
            m.Owner = ReadIdentity(r);
            if (m.Owner.Type == 0)
            {
                m.Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                m.Heading = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }

            m.PlayfieldId = r.ReadInt32();
            m.StateMachine = ReadIdentity(r);
            m.Unknown3 = r.ReadInt16();

            // The stats: an unchecked 0x3F1 count.
            var statCount = r.ReadInt32() / 0x3f1 - 1;
            var stats = new List<GameTuple<Stat, int>>();
            for (var i = 0; i < statCount && !r.Failed; i++)
            {
                stats.Add(new GameTuple<Stat, int> { Value1 = (Stat)r.ReadInt32(), Value2 = r.ReadInt32() });
            }

            m.Stats = stats.ToArray();
            var nameLength = r.ReadInt32();
            m.Name = nameLength > 0 ? r.ReadString(nameLength) : string.Empty;

            m.Unknown4 = r.ReadInt32();
            m.Unknown5 = r.ReadInt32();
            var raw = r.ReadUInt32();
            var identityCount = raw / 0x3f1 - 1;
            if (raw % 0x3f1 != 0 || identityCount >= 0x7531)
            {
                r.Fail();
                return m;
            }

            var identities = new List<Identity>();
            for (uint i = 0; i < identityCount && !r.Failed; i++)
            {
                identities.Add(ReadIdentity(r));
            }

            m.UnknownArray = identities.ToArray();
            m.Unknown6 = r.ReadInt32();

            // The spells: a positive multiple of 0x3F1, at most 999 of them.
            var spellRaw = r.ReadInt32();
            if (spellRaw % 0x3f1 != 0 || spellRaw <= 0 || spellRaw / 0x3f1 - 1 > 999)
            {
                r.Fail();
                return m;
            }

            var spells = new List<SpellData>();
            for (var i = 0; i < spellRaw / 0x3f1 - 1 && !r.Failed; i++)
            {
                spells.Add(SpellStream.ReadSpell(r));
            }

            m.Spells = spells.ToArray();
            if (r.Failed)
            {
                return m;
            }

            m.UnknownIdentity = ReadIdentity(r);
            m.Textures = SimpleCharFullUpdateSerializer.ReadClothes(r);
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
                        <CorpseFullUpdateSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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

        public Expression SerializerExpression(
            ParameterExpression streamWriterExpression,
            ParameterExpression serializationContextExpression,
            Expression valueExpression,
            PropertyMetaData propertyMetaData)
        {
            var serializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                        <CorpseFullUpdateSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
                        (o => o.Serialize);
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

        public void Serialize(
            StreamWriter streamWriter,
            SerializationContext serializationContext,
            object value,
            PropertyMetaData propertyMetaData = null)
        {
            var m = (CorpseFullUpdateMessage)value;
            var w = streamWriter;
            w.WriteInt32((int)m.N3MessageType);
            WriteIdentity(w, m.Identity);
            w.WriteByte(m.Unknown);
            w.WriteInt32(m.Version);
            w.WriteInt32(m.Unknown2);
            WriteIdentity(w, m.Owner);
            if (m.Owner.Type == 0)
            {
                w.WriteSingle(m.Position.X);
                w.WriteSingle(m.Position.Y);
                w.WriteSingle(m.Position.Z);
                w.WriteSingle(m.Heading.X);
                w.WriteSingle(m.Heading.Y);
                w.WriteSingle(m.Heading.Z);
                w.WriteSingle(m.Heading.W);
            }

            w.WriteInt32(m.PlayfieldId);
            WriteIdentity(w, m.StateMachine);
            w.WriteInt16(m.Unknown3);
            var stats = m.Stats ?? new GameTuple<Stat, int>[0];
            w.WriteInt32((stats.Length + 1) * 0x3f1);
            foreach (var stat in stats)
            {
                w.WriteInt32((int)stat.Value1);
                w.WriteInt32(stat.Value2);
            }

            var name = m.Name ?? string.Empty;
            w.WriteInt32(name.Length);
            w.WriteString(name);
            w.WriteInt32(m.Unknown4);
            w.WriteInt32(m.Unknown5);
            var identities = m.UnknownArray ?? new Identity[0];
            w.WriteInt32((identities.Length + 1) * 0x3f1);
            foreach (var identity in identities)
            {
                WriteIdentity(w, identity);
            }

            w.WriteInt32(m.Unknown6);
            var spells = m.Spells ?? new SpellData[0];
            w.WriteInt32((spells.Length + 1) * 0x3f1);
            foreach (var spell in spells)
            {
                SpellStream.WriteSpell(w, spell);
            }

            WriteIdentity(w, m.UnknownIdentity);
            SimpleCharFullUpdateSerializer.WriteClothes(w, m.Textures);
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

        #endregion
    }
}
