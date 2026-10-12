// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayfieldAnarchyFSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the PlayfieldAnarchyFSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Linq.Expressions;
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    public class PlayfieldAnarchyFSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(PlayfieldAnarchyFMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var m = new PlayfieldAnarchyFMessage();
            var r = streamReader;
            m.N3MessageType = (N3MessageType)r.ReadInt32();
            m.Identity = PlayfieldStream.ReadIdentity(r);
            m.Unknown = r.ReadByte();
            m.Version = r.ReadInt32();
            m.CharacterCoordinates = PlayfieldStream.ReadVector3(r);
            if (m.Version > 1)
            {
                m.PlayfieldProxyVersion = r.ReadByte();
                if (m.PlayfieldProxyVersion != (byte)'a')
                {
                    r.Fail();
                    return m;
                }

                m.PlayfieldId1 = PlayfieldStream.ReadIdentity(r);
                m.Unknown3 = r.ReadInt32();
                m.SG = r.ReadInt32();
                m.ProxyId = PlayfieldStream.ReadIdentity(r);
            }

            // The inline object: an identity, and when it is not 0:0, the object of that type from it on. Live
            // servers leave the identity off when only the world block follows.
            m.HasInlineIdentity = m.Version > 3 && r.PeekUntilEnd() >= 16;
            if (m.HasInlineIdentity)
            {
                m.InlineIdentity = PlayfieldStream.ReadIdentity(r);
                var type = (int)m.InlineIdentity.Type;
                if (type != 0 || m.InlineIdentity.Instance != 0)
                {
                    m.InlineVersion = r.ReadInt32();
                    if (type == PlayfieldRecord.RecordType)
                    {
                        m.PlayfieldRecord = PlayfieldStream.ReadPlayfield(r, m.InlineIdentity.Instance);
                    }
                    else if (type == GeneratorType || type == EntranceType)
                    {
                        m.BuildingGenerator = PlayfieldStream.ReadGenerator(r);
                    }
                    else if (type == ProxyInstanceType || type == ProxyInstanceDataType)
                    {
                        m.ProxyInstance = PlayfieldStream.ReadProxyInstance(r, type == ProxyInstanceDataType);
                    }
                    else
                    {
                        r.Fail();
                        return m;
                    }
                }
            }

            // PlayfieldAnarchyFIIR_t's world block.
            m.HasWorld = r.PeekUntilEnd() >= 8;
            if (m.HasWorld)
            {
                m.PlayfieldX = r.ReadInt32();
                m.PlayfieldZ = r.ReadInt32();
            }

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
                        <PlayfieldAnarchyFSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <PlayfieldAnarchyFSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var m = (PlayfieldAnarchyFMessage)value;
            var w = streamWriter;
            w.WriteInt32((int)m.N3MessageType);
            PlayfieldStream.WriteIdentity(w, m.Identity);
            w.WriteByte(m.Unknown);
            w.WriteInt32(m.Version);
            PlayfieldStream.WriteVector3(w, m.CharacterCoordinates);
            if (m.Version > 1)
            {
                w.WriteByte(m.PlayfieldProxyVersion);
                PlayfieldStream.WriteIdentity(w, m.PlayfieldId1);
                w.WriteInt32(m.Unknown3);
                w.WriteInt32(m.SG);
                PlayfieldStream.WriteIdentity(w, m.ProxyId);
            }

            if (m.Version > 3 && m.HasInlineIdentity)
            {
                PlayfieldStream.WriteIdentity(w, m.InlineIdentity);
                var type = (int)m.InlineIdentity.Type;
                if (type != 0 || m.InlineIdentity.Instance != 0)
                {
                    w.WriteInt32(m.InlineVersion);
                    if (type == PlayfieldRecord.RecordType)
                    {
                        PlayfieldStream.WritePlayfield(w, m.PlayfieldRecord ?? new PlayfieldRecord());
                    }
                    else if (type == GeneratorType || type == EntranceType)
                    {
                        PlayfieldStream.WriteGenerator(w, m.BuildingGenerator ?? new AcgBuildingGenerator());
                    }
                    else if (type == ProxyInstanceType || type == ProxyInstanceDataType)
                    {
                        PlayfieldStream.WriteProxyInstance(
                            w,
                            m.ProxyInstance ?? new ProxyInstance(),
                            type == ProxyInstanceDataType);
                    }
                }
            }

            if (m.HasWorld)
            {
                w.WriteInt32(m.PlayfieldX);
                w.WriteInt32(m.PlayfieldZ);
            }
        }

        #endregion

        #region Constants

        /// <summary>A generated building (ACGBuildingGeneratorData_t).</summary>
        private const int GeneratorType = 0xC79F;

        /// <summary>A dungeon entrance; live servers send the generated building under it.</summary>
        private const int EntranceType = 0xC7A1;

        private const int ProxyInstanceType = 0xC77D;

        /// <summary>A proxy instance with its block before the dynels.</summary>
        private const int ProxyInstanceDataType = 0xC77B;

        #endregion
    }
}
