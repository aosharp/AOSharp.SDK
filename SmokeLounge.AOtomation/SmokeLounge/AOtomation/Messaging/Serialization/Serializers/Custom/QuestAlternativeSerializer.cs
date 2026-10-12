// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestAlternativeSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestAlternativeSerializer type.
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
    using System.Collections.Generic;

    public class QuestAlternativeSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(QuestAlternativeMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new QuestAlternativeMessage();
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = QuestStream.ReadIdentity(streamReader);
            message.Unknown = streamReader.ReadByte();

            // ACGQuestIIR_t: the character must be a SimpleChar with an instance.
            if (message.Identity.Type != IdentityType.SimpleChar || message.Identity.Instance == 0)
            {
                streamReader.Fail();
                return message;
            }

            message.Version = streamReader.ReadByte();
            if (message.Version != 4)
            {
                streamReader.Fail();
                return message;
            }

            var sliders = message.MissionSliders;
            sliders.Difficulty = streamReader.ReadByte();
            sliders.GoodBad = streamReader.ReadByte();
            sliders.OrderChaos = streamReader.ReadByte();
            sliders.OpenHidden = streamReader.ReadByte();
            sliders.PhysicalMystical = streamReader.ReadByte();
            sliders.HeadonStealth = streamReader.ReadByte();
            sliders.CreditsXp = streamReader.ReadByte();
            message.Seed = streamReader.ReadInt32();
            message.Scope = (MissionScope)streamReader.ReadByte();
            message.Terminal = QuestStream.ReadIdentity(streamReader);
            if (streamReader.Failed)
            {
                return message;
            }

            // Each dimension a signed byte in -100..100; the difficulty 1..11, the originator 1..8, the terminal set.
            foreach (var dimension in new[] { sliders.GoodBad, sliders.OrderChaos, sliders.OpenHidden, sliders.PhysicalMystical, sliders.HeadonStealth, sliders.CreditsXp })
            {
                if ((byte)((sbyte)dimension + 100) >= 0xc9)
                {
                    streamReader.Fail();
                    return message;
                }
            }

            if ((uint)(sliders.Difficulty - 1) > 10 || (uint)((int)message.Scope - 1) > 7
                || (message.Terminal.Type == 0 && message.Terminal.Instance == 0))
            {
                streamReader.Fail();
                return message;
            }

            var count = streamReader.ReadByte();
            if (count > 5)
            {
                streamReader.Fail();
                return message;
            }

            var missions = new List<MissionInfo>(count);
            for (var i = 0; i < count; i++)
            {
                var mission = new MissionInfo();
                mission.Quest.Identity = QuestStream.ReadIdentity(streamReader);
                if (!QuestStream.ReadQuest(streamReader, mission.Quest))
                {
                    return message;
                }

                mission.Unknown1 = streamReader.ReadByte();
                missions.Add(mission);
            }

            message.MissionDetails = missions.ToArray();
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
                        <QuestAlternativeSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <QuestAlternativeSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var message = (QuestAlternativeMessage)value;
            streamWriter.WriteInt32((int)message.N3MessageType);
            QuestStream.WriteIdentity(streamWriter, message.Identity);
            streamWriter.WriteByte(message.Unknown);
            var sliders = message.MissionSliders ?? new MissionSliders();
            streamWriter.WriteByte(message.Version);
            streamWriter.WriteByte(sliders.Difficulty);
            streamWriter.WriteByte(sliders.GoodBad);
            streamWriter.WriteByte(sliders.OrderChaos);
            streamWriter.WriteByte(sliders.OpenHidden);
            streamWriter.WriteByte(sliders.PhysicalMystical);
            streamWriter.WriteByte(sliders.HeadonStealth);
            streamWriter.WriteByte(sliders.CreditsXp);
            streamWriter.WriteInt32(message.Seed);
            streamWriter.WriteByte((byte)message.Scope);
            QuestStream.WriteIdentity(streamWriter, message.Terminal);
            var missions = message.MissionDetails ?? new MissionInfo[0];
            streamWriter.WriteByte((byte)missions.Length);
            foreach (var mission in missions)
            {
                QuestStream.WriteIdentity(streamWriter, mission.Quest.Identity);
                QuestStream.WriteQuest(streamWriter, mission.Quest);
                streamWriter.WriteByte(mission.Unknown1);
            }
        }

        #endregion
    }
}
