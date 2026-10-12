// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellListSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellListSerializer type.
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

    public class SpellListSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(SpellListMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            // SpellListIIR_t as the client reads it.
            var m = new SpellListMessage();
            var r = streamReader;
            m.N3MessageType = (N3MessageType)r.ReadInt32();
            m.Identity = PlayfieldStream.ReadIdentity(r);
            m.Unknown = r.ReadByte();

            // The spells: a positive multiple of 0x3F1, at most 999 of them.
            var raw = r.ReadInt32();
            if (raw % 0x3f1 != 0 || raw <= 0 || raw / 0x3f1 - 1 > 999)
            {
                r.Fail();
                return m;
            }

            var spells = new List<SpellData>();
            for (var i = 0; i < raw / 0x3f1 - 1 && !r.Failed; i++)
            {
                spells.Add(SpellStream.ReadSpell(r));
            }

            m.Spells = spells.ToArray();
            if (r.Failed)
            {
                return m;
            }

            m.Source = PlayfieldStream.ReadIdentity(r);
            m.Unknown1 = PlayfieldStream.ReadIdentity(r);
            m.Unknown2 = r.ReadByte() != 0;
            var length = r.ReadUInt16();
            if (length >= 0x8000)
            {
                r.Fail();
                return m;
            }

            m.Name = length > 0 ? r.ReadString(length) : string.Empty;
            m.HasItem = r.ReadByte() != 0;
            if (m.HasItem)
            {
                m.Item = PlayfieldStream.ReadIdentity(r);
            }

            m.Unknown3 = r.ReadByte() != 0;
            m.Unknown4 = r.ReadInt32();
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
                        <SpellListSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <SpellListSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var m = (SpellListMessage)value;
            var w = streamWriter;
            w.WriteInt32((int)m.N3MessageType);
            PlayfieldStream.WriteIdentity(w, m.Identity);
            w.WriteByte(m.Unknown);
            var spells = m.Spells ?? new SpellData[0];
            w.WriteInt32((spells.Length + 1) * 0x3f1);
            foreach (var spell in spells)
            {
                SpellStream.WriteSpell(w, spell);
            }

            PlayfieldStream.WriteIdentity(w, m.Source);
            PlayfieldStream.WriteIdentity(w, m.Unknown1);
            w.WriteByte(m.Unknown2 ? (byte)1 : (byte)0);
            var name = m.Name ?? string.Empty;
            w.WriteUInt16((ushort)name.Length);
            w.WriteString(name);
            w.WriteByte(m.HasItem ? (byte)1 : (byte)0);
            if (m.HasItem)
            {
                PlayfieldStream.WriteIdentity(w, m.Item);
            }

            w.WriteByte(m.Unknown3 ? (byte)1 : (byte)0);
            w.WriteInt32(m.Unknown4);
        }

        #endregion
    }
}
