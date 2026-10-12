// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RaidSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the RaidSerializer type.
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

    public class RaidSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(RaidMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new RaidMessage();
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = new Identity
            {
                Type = (IdentityType)streamReader.ReadInt32(),
                Instance = streamReader.ReadInt32()
            };
            message.Unknown = streamReader.ReadByte();
            message.Command = streamReader.ReadInt16();
            if (message.Command != RaidMessage.LocksCommand)
            {
                return message;
            }

            message.Version = streamReader.ReadInt32();
            if (message.Version != RaidMessage.LockListVersion)
            {
                return message;
            }

            var count = streamReader.ReadInt32();
            var locks = new List<RaidLock>();
            for (var i = 0; i < count && !streamReader.Failed; i++)
            {
                locks.Add(new RaidLock
                {
                    Unknown1 = streamReader.ReadInt32(),
                    Id = streamReader.ReadInt32(),
                    Unknown2 = streamReader.ReadInt32(),
                    Value = streamReader.ReadUInt32(),
                    Expires = streamReader.ReadInt32()
                });
            }

            message.Locks = locks.ToArray();
            return message;
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
                        <RaidSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <RaidSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var message = (RaidMessage)value;
            streamWriter.WriteInt32((int)message.N3MessageType);
            streamWriter.WriteInt32((int)message.Identity.Type);
            streamWriter.WriteInt32(message.Identity.Instance);
            streamWriter.WriteByte(message.Unknown);
            streamWriter.WriteInt16(message.Command);
            if (message.Command != RaidMessage.LocksCommand)
            {
                return;
            }

            streamWriter.WriteInt32(message.Version);
            if (message.Version != RaidMessage.LockListVersion)
            {
                return;
            }

            var locks = message.Locks ?? new RaidLock[0];
            streamWriter.WriteInt32(locks.Length);
            foreach (var raidLock in locks)
            {
                streamWriter.WriteInt32(raidLock.Unknown1);
                streamWriter.WriteInt32(raidLock.Id);
                streamWriter.WriteInt32(raidLock.Unknown2);
                streamWriter.WriteUInt32(raidLock.Value);
                streamWriter.WriteInt32(raidLock.Expires);
            }
        }

        #endregion
    }
}
