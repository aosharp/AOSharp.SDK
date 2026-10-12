// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestFullUpdateSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestFullUpdateSerializer type.
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

    public class QuestFullUpdateSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(QuestFullUpdateMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new QuestFullUpdateMessage();
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = QuestStream.ReadIdentity(streamReader);
            message.Unknown = streamReader.ReadByte();
            Quest[] quests;
            if (!QuestStream.ReadQuestList(streamReader, out quests))
            {
                return message;
            }

            message.Quests = quests;
            message.Announce = streamReader.ReadByte() != 0;
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
                        <QuestFullUpdateSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <QuestFullUpdateSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var message = (QuestFullUpdateMessage)value;
            streamWriter.WriteInt32((int)message.N3MessageType);
            QuestStream.WriteIdentity(streamWriter, message.Identity);
            streamWriter.WriteByte(message.Unknown);
            QuestStream.WriteQuestList(streamWriter, message.Quests);
            streamWriter.WriteByte(message.Announce ? (byte)1 : (byte)0);
        }

        #endregion
    }
}
