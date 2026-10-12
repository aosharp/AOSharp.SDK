// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResearchUpdateSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ResearchUpdateSerializer type.
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

    public class ResearchUpdateSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(ResearchUpdateMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new ResearchUpdateMessage();
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = new Identity
            {
                Type = (IdentityType)streamReader.ReadInt32(),
                Instance = streamReader.ReadInt32()
            };
            message.Unknown = streamReader.ReadByte();
            message.Unknown1 = streamReader.ReadByte();

            // Lines until a perk id of 0 (one that ran out of message reads as 0 too).
            var lines = new List<ResearchLine>();
            while (true)
            {
                var id = streamReader.ReadInt32();
                if (id == 0)
                {
                    break;
                }

                lines.Add(new ResearchLine
                {
                    ResearchId = id,
                    Unknown1 = streamReader.ReadInt32(),
                    Unknown2 = streamReader.ReadInt32(),
                    Unknown3 = streamReader.ReadInt32()
                });
            }

            message.ResearchLines = lines.ToArray();
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
                        <ResearchUpdateSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <ResearchUpdateSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var message = (ResearchUpdateMessage)value;
            streamWriter.WriteInt32((int)message.N3MessageType);
            streamWriter.WriteInt32((int)message.Identity.Type);
            streamWriter.WriteInt32(message.Identity.Instance);
            streamWriter.WriteByte(message.Unknown);
            streamWriter.WriteByte(message.Unknown1);
            foreach (var line in message.ResearchLines ?? new ResearchLine[0])
            {
                streamWriter.WriteInt32(line.ResearchId);
                streamWriter.WriteInt32(line.Unknown1);
                streamWriter.WriteInt32(line.Unknown2);
                streamWriter.WriteInt32(line.Unknown3);
            }

            streamWriter.WriteInt32(0);
        }

        #endregion
    }
}
